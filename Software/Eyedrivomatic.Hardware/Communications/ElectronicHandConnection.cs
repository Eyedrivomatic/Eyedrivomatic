using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Services;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Hardware.Communications
{
    internal class ElectronicHandConnection : IDeviceConnection
    {
        private SerialPort _serialPort;
        private readonly IList<IElectronicHandDeviceInfo> _infos;
        private Subject<IObservable<char>> _connectionSubject;

        private bool _disposed;

        public ElectronicHandConnection(IEnumerable<IElectronicHandDeviceInfo> infos, string connectionString)
        {
            ConnectionString = connectionString;
            _infos = infos.ToList();
        }

        public string ConnectionString { get; }

        public event EventHandler ConnectionStateChanged;
        private void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new EventArgs());
        }

        public Version FirmwareVersion { get; protected set; }

        private bool _isConnecting;
        public bool IsConnecting
        {
            get => _isConnecting;
            private set
            {
                _isConnecting = value;
                OnConnectionStateChanged();
            }
        }

        public bool IsConnected => !_disposed && (_serialPort?.IsOpen ?? false);

        public bool ConnectionFailed { get; private set; }

        public ConnectionState State => IsConnected
            ? ConnectionState.Connected
            : IsConnecting
                ? ConnectionState.Connecting
                : ConnectionFailed 
                    ? ConnectionState.Error 
                    : ConnectionState.Disconnected;

        public IObservable<IObservable<char>> DataStream => Observable.Defer(ObservableFactory).AsObservable();

        private IObservable<IObservable<char>> ObservableFactory()
        {
            if (_connectionSubject == null)
            {
                _connectionSubject = new Subject<IObservable<char>>();
                return _connectionSubject.AsObservable().StartWith(CreateDataStream());
            }

            return _connectionSubject;
        }

        public async Task ConnectAsync(CancellationToken ctsToken)
        {
            try
            {
                Disconnect();
            }
            catch (IOException)
            {
                //Failure is an option.
            }

            try
            {
                IsConnecting = true;
                _serialPort = await OpenSerialPortAsync(ConnectionString);
                _connectionSubject?.OnNext(Observable.Defer(CreateDataStream));
            }
            catch
            {
                ConnectionFailed = true;
                throw;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public void Disconnect()
        {
            if (IsConnected) Log.Info(this, "Disconnecting");

            var tmp = _serialPort;
            _serialPort = null; //IsConnected will now return false.
            tmp?.BaseStream.Close();
            tmp?.Close();
            tmp?.Dispose();

            OnConnectionStateChanged();
        }

        public void SendMessage(string message)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");
            Log.Debug(this, $"sending [{message}].");
            _serialPort.WriteLine(message);
        }

        private async Task<SerialPort> OpenSerialPortAsync(string port)
        {
            try
            {
                Log.Info(this, $"Opening port [{port}].");
                var serialPort = new SerialPort(port, 19200)
                {
                    DtrEnable = false,
                    ReadTimeout = 5000
                };

                serialPort.Open();

                serialPort.DiscardInBuffer();
                serialPort.DtrEnable = true;//this will reset the Arduino.

                if (!serialPort.IsOpen) return null;

                var reader = new StreamReader(serialPort.BaseStream, Encoding.ASCII); //Do not dispose. It will close the underlying stream.
                var firstMessage = await reader.ReadLineAsync();
                Log.Debug(typeof(ElectronicHandEnumerationService), $"First message on port [{serialPort.PortName}] is [{firstMessage}].");

                if (!VerifyStartupMessage(firstMessage))
                {
                    Log.Info(this, $"Device not found on port [{port}]");
                    serialPort.Dispose();
                    return null;
                }

                return serialPort;
            }
            catch (UnauthorizedAccessException)
            {
                // Access is denied to the port. 
                // -or -
                // The current process, or another process on the system, already has the specified COM port open either by a SerialPort instance or in unmanaged code.
                Log.Error(typeof(ElectronicHandEnumerationService), $"COM port [{port}] is in use.");
                return null;
            }
            catch (ArgumentException)
            {
                //Configuration shold start with "COM"
                Log.Error(typeof(ElectronicHandEnumerationService), $"Invalid port name [{port}].");
                return null;
            }
            catch (IOException ex)
            {
                //The port is in an invalid state.
                // -or -
                //An attempt to set the state of the underlying port failed. For example, the parameters passed from this SerialPort object were invalid.
                Log.Error(typeof(ElectronicHandEnumerationService), $"Failed to open the com port [{ex}]");
                return null;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(typeof(ElectronicHandEnumerationService), $"Failed to open the com port [{ex}]");
                return null;
            }
        }

        private IObservable<char> CreateDataStream()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");

            return Observable
                .Using(() => new StreamReader(_serialPort.BaseStream, Encoding.ASCII, false, _serialPort.ReadBufferSize, true), reader => ReadFromSerial(reader, _serialPort.ReadBufferSize))
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(new SynchronizationContextScheduler(SynchronizationContext.Current));
        }

        private IObservable<char> ReadFromSerial(StreamReader reader, int bufferSize)
        {
            var readerObservable = Observable.FromAsync(async () =>
            {
                var buffer = new char[bufferSize];
                try
                {
                    var read = await reader.ReadAsync(buffer, 0, buffer.Length);
                    var result = new char[read];

                    Array.Copy(buffer, result, read);
                    return result;
                }
                catch (IOException)
                {
                    Log.Info(this, "Disconnected.");
                    //The connection was closed.
                    return new char[0];
                }
            });

            //Using Select + Concat rather than SelectMany to ensure order of streamed bytes.
            //http://stackoverflow.com/questions/26300072/reactive-extensions-selectmany-and-concat/26301216
            return readerObservable
                .Repeat()
                .TakeWhile(message => message.Any())
                .Select(message => message.ToObservable()).Concat();
        }

        private bool VerifyStartupMessage(string firstMessage)
        {
            FirmwareVersion = _infos.Select(i => i.VerifyStartupMessage(firstMessage)).FirstOrDefault(v => v != null);
            return FirmwareVersion != null;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _isConnecting = false;
            _serialPort?.Dispose();
            _connectionSubject.OnCompleted();
            _serialPort = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Infrastructure;
using Prism.Events;

namespace Eyedrivomatic.ButtonDriver.Hardware.Communications
{
    [Export(typeof(IBrainBoxConnection))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class BrainBoxConnection : IBrainBoxConnection
    {
        private SerialPort _serialPort;

        private readonly Subject<IObservable<char>> _connectionSubject = new Subject<IObservable<char>>();

        private IEventAggregator Events { get; }

        public string ConnectionString => _serialPort?.PortName ?? string.Empty;

        [ImportingConstructor]
        internal BrainBoxConnection(IEventAggregator events)
        {
            Events = events;
        }


        public event EventHandler ConnectionStateChanged;
        private void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new EventArgs());
            Events?.GetEvent<ConnectionStateEvent>().Publish(State);
        }

        public IEnumerable<Tuple<string, string>> GetAvailableDevices()
        {
            return BrainBoxPortFinder.GetAvailableDevices();
        }

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

        public IObservable<IObservable<char>> DataStream => _connectionSubject.AsObservable();

        public async Task AutoConnectAsync()
        {
            try
            {
                Disconnect();
            }
            catch (IOException)
            {
                //Failure is an option.
            }

            IsConnecting = true;

            try
            {
                _serialPort = await BrainBoxPortFinder.DetectDeviceAsync(CancellationToken.None);
                if (_serialPort == null)
                {
                    Log.Warn(this, "Failed to locate an Eyedrivomatic BrainBox device.");
                    ConnectionFailed = true;
                    return;
                }
                _connectionSubject.OnNext(CreateDataStream());
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

        public async Task ConnectAsync(string configuration)
        {
            if (string.IsNullOrEmpty(configuration)) throw new ArgumentException("Configuration not specified.");

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
                _serialPort = await BrainBoxPortFinder.OpenSerialPortAsync(configuration);
                _connectionSubject.OnNext(CreateDataStream());
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

        private IObservable<char> CreateDataStream()
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");

            return Observable
                .Using(() => new StreamReader(_serialPort.BaseStream, Encoding.ASCII, false, _serialPort.ReadBufferSize, true), ReadFromSerial)
                .SubscribeOn(TaskPoolScheduler.Default)
                .ObserveOn(new SynchronizationContextScheduler(SynchronizationContext.Current));
        }

        private IObservable<char> ReadFromSerial(StreamReader reader)
        {
            var readerObservable = Observable.FromAsync(async () =>
            {
                var buffer = new char[_serialPort.ReadBufferSize];
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


        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _isConnecting = false;
            _serialPort?.Dispose();
            _connectionSubject.OnCompleted();
        }
    }
}

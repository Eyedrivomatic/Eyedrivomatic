//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
        private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(3);

        private bool _disposed;

        public ElectronicHandConnection(IEnumerable<IElectronicHandDeviceInfo> infos, DeviceDescriptor device)
        {
            Device = device;
            _infos = infos.ToList();
        }

        public DeviceDescriptor Device { get; }
        public string ConnectionString => Device.ConnectionString;

        public event EventHandler ConnectionStateChanged;
        private void OnConnectionStateChanged()
        {
            ConnectionStateChanged?.Invoke(this, new EventArgs());
        }

        public VersionInfo VersionInfo { get; protected set; } = new VersionInfo(new Version(0, 0, 0, 0));

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

        public async Task ConnectAsync(CancellationToken cancellationToken)
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
                _serialPort = await OpenSerialPortAsync(ConnectionString, cancellationToken);
                _connectionSubject?.OnNext(Observable.Defer(CreateDataStream));
            }
            catch (Exception ex)
            {
                ConnectionFailed = true;
                Log.Warn(this, $"Failed to connect to deice on {ConnectionString} - [{ex}]");
                _serialPort?.Dispose();
                _serialPort = null;
            }
            finally
            {
                IsConnecting = false;
            }
        }

        public void Disconnect()
        {
            if (_serialPort == null) return;
            if (IsConnected || IsConnecting) Log.Info(this, "Disconnecting");

            var tmp = _serialPort;
            _serialPort = null; //IsConnected will now return false.
            if (tmp != null)
            {
                tmp.BaseStream.Close();
                tmp.DtrEnable = false;
                Thread.Sleep(250);
                tmp.DtrEnable = true;
                Thread.Sleep(250);
                tmp.Close();
                tmp.Dispose();
            }
            OnConnectionStateChanged();
        }

        public void SendMessage(string message)
        {
            if (!IsConnected) throw new InvalidOperationException("Not connected.");
            Log.Debug(this, $"sending [{message}].");
            _serialPort.WriteLine(message);
        }

        private async Task<SerialPort> OpenSerialPortAsync(string port, CancellationToken cancellationToken)
        {
            try
            {
                Log.Info(this, $"Opening port [{port}].");

                try
                {
                    return await OpenSerialPortAsync(port, 19200, cancellationToken);
                }
                catch (TimeoutException)
                {
                    Log.Warn(this, $"Failed to read first message on [{port}] at [19200] baud. Attempting the slower speed of previous versions of [9600] baud.");
                    await Task.Delay(500, cancellationToken); //The serial port has a background thread that needs some time to close.
                    return await OpenSerialPortAsync(port, 9600, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Access is denied to the port. 
                // -or -
                // The current process, or another process on the system, already has the specified COM port open either by a SerialPort instance or in unmanaged code.
                Log.Error(this, $"COM port [{port}] is in use.");
                return null;
            }
            catch (ArgumentException)
            {
                //Configuration shold start with "COM"
                Log.Error(this, $"Invalid port name [{port}].");
                return null;
            }
            catch (TimeoutException)
            {
                Log.Warn(this, $"Failed to read first message on [{port}] at [9600] baud.");
                return null;
            }
            catch (IOException ex)
            {
                cancellationToken.ThrowIfCancellationRequested();

                //The port is in an invalid state.
                // -or -
                //An attempt to set the state of the underlying port failed. For example, the parameters passed from this SerialPort object were invalid.
                Log.Error(this, $"Failed to open the com port [{port}] [{ex.Message}]");
                return null;
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(this, $"Failed to open the com port [{port}] [{ex}]");
                return null;
            }
        }

        private async Task<SerialPort> OpenSerialPortAsync(string port, int speed, CancellationToken cancellationToken)
        {
            var serialPort = new SerialPort(port, speed)
            {
                DtrEnable = false,
            };

            var timeoutSource = new CancellationTokenSource(_connectionTimeout);

            try
            {
                serialPort.Open();

                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
                // ReSharper disable once AccessToDisposedClosure - closure used locally.
                using (cts.Token.Register(() => serialPort.Dispose()))
                {
                    var firstMessage = await ReadFirstMessageAsync(serialPort);
                    if (cancellationToken.IsCancellationRequested) return null;

                    if (!VerifyStartupMessage(firstMessage))
                    {
                        Log.Info(this, $"Device not found on port [{port}]");
                        serialPort.Dispose();
                        return null;
                    }
                }

            }
            catch (IOException) when (timeoutSource.IsCancellationRequested)
            {
                serialPort.Dispose();
                throw new TimeoutException();
            }
            catch (Exception)
            {
                serialPort.Dispose();
                throw;
            }

            return serialPort;
        }


        private async Task<string> ReadFirstMessageAsync(SerialPort serialPort)
        {
            serialPort.DiscardInBuffer();
            serialPort.DtrEnable = true; //this will reset the Arduino.

            if (!serialPort.IsOpen) return null;
            var reader = new StreamReader(serialPort.BaseStream, Encoding.ASCII); //Do not dispose. It will close the underlying stream.
            var firstMessage = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(firstMessage)) firstMessage = await reader.ReadLineAsync(); //In an earlier version of the software, a newline was sent first.
            Log.Debug(this, $"First message on port [{serialPort.PortName}] is [{firstMessage}].");
            return firstMessage;
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
            var version = _infos.Select(i => i.VerifyStartupMessage(firstMessage)).FirstOrDefault(v => v != null);

            VersionInfo = version ?? VersionInfo.Unknown;
            return version != null;
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

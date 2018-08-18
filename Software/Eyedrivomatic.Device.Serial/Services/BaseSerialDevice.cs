using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Serial.Communications;
using Eyedrivomatic.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.Device.Serial.Services
{
    public abstract class BaseSerialDevice : BindableBase, IDevice, IDisposable
    {
        private readonly IList<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> _messageProcessors;

        private IDisposable _connectionDatastream;

        protected readonly IDeviceCommands Commands;

        public static uint SwitchCount = 4;

        protected BaseSerialDevice(
            IDeviceConnection connection,
            IDeviceCommands commandFactory,
            IEnumerable<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>> messageProcessors,
            IDeviceStatus deviceStatus,
            IDeviceSettings deviceSettings)
        {
            Commands = commandFactory;
            _messageProcessors = new List<Lazy<IDeviceSerialMessageProcessor, IMessageProcessorMetadata>>(messageProcessors);
            DeviceStatus = deviceStatus;
            DeviceSettings = deviceSettings;

            deviceStatus.PropertyChanged += OnDeviceStatusChanged;

            Connection = connection;
            Connection.ConnectionStateChanged += OnConnectionStateChanged;
            Connection.DataStream.Subscribe(AttachToDataStream);
        }

        public bool DeviceReady => Connection?.State == ConnectionState.Connected && DeviceStatus.IsKnown;

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private void OnDeviceStatusChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceStatus.IsKnown))
            {
                RaisePropertyChanged(nameof(DeviceReady));
            }

            RaisePropertyChanged(nameof(DeviceStatus));
        }

        private void AttachToDataStream(IObservable<char> observable)
        {
            var dataStream = observable.Publish();
            var sendStream = new AnonymousObserver<string>(message => Connection?.SendMessage(message));

            foreach (var lazy in _messageProcessors)
            {
                Log.Debug(this, $"Attaching datastream to [{lazy.Metadata.Name}].");
                lazy.Value.Attach(dataStream, sendStream);
            }

            _connectionDatastream = dataStream.Connect();

            Commands.GetConfiguration(SettingNames.All);
            Commands.GetStatus();
        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        private void OnConnectionStateChanged(object sender, EventArgs eventArgs)
        {
            RaisePropertyChanged(nameof(ConnectionState));
            RaisePropertyChanged(nameof(DeviceReady));
        }

        #region Connection
        public IDeviceConnection Connection { get; }

        #endregion Connection

        public IDeviceSettings DeviceSettings { get; }

        public IDeviceStatus DeviceStatus { get; }

        public virtual ConnectionState ConnectionState => Connection.State;

        public Task<bool> Move(Point point, TimeSpan duration)
        {
            Log.Info(this, $"Move Point[{point}].");
            return Commands.Move(point, duration);
        }

        public virtual Task<bool> Go(Vector vector, TimeSpan duration)
        {
            Log.Info(this, $"Go Vector:[{vector}].");
            return Commands.Go(vector, duration);
        }

        public virtual void Stop()
        {
            Log.Info(this, "Stop.");
            Commands.Stop();
        }

        public async Task CycleSwitchAsync(uint relay, uint repeat = 1, uint toggleDelayMs = 500, uint repeatDelayMs = 1000)
        {
            Log.Info(this, $"Cycling relay {relay} for {toggleDelayMs} ms, {repeat} times with a delay of {repeatDelayMs} ms.");
            try
            {
                for (int i = 0; i < repeat; i++)
                {
                    if (i > 0) await Task.Delay(TimeSpan.FromMilliseconds(repeatDelayMs));

                    Log.Info(this, $"Cycling relay {relay}.");
                    await Commands.ToggleSwitch(relay, TimeSpan.FromMilliseconds(toggleDelayMs));
                    await Task.Delay(TimeSpan.FromMilliseconds(220 + toggleDelayMs)); //the relay cycles over 200ms. Add a few ms to allow for communication.
                }
                Log.Info(this, $"Cycling relay {relay} done.");
            }
            catch (Exception e)
            {
                Log.Error(this, $"Failed to toggle relay command - [{e}]");
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Connection?.Dispose();
            _connectionDatastream?.Dispose();
            _connectionDatastream = null;
        }
        #endregion IDisposable
    }
}
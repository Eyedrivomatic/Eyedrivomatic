using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.Device.Configuration.ViewModels
{
    public abstract class DeviceViewModelBase : BindableBase, IDisposable
    {
        private IDevice _device;

        protected IDeviceService DeviceService { get; }

        protected IDevice Device
        {
            get => _device;
            private set
            {
                if (_device != null)
                {
                    _device.PropertyChanged -= OnDeviceStateChanged;
                    _device.DeviceSettings.PropertyChanged -= OnDriverSettingsChanged;
                }
                SetProperty(ref _device, value);
                if (_device != null)
                {
                    _device.PropertyChanged += OnDeviceStateChanged;
                    _device.DeviceSettings.PropertyChanged += OnDriverSettingsChanged;
                }
            }
        }

        protected DeviceViewModelBase(IDeviceService deviceService)
        {
            DeviceService = deviceService;
            DeviceService.ConnectedDeviceChanged += (sender, args) => Device = deviceService.ConnectedDevice;
            Device = DeviceService.ConnectedDevice;
        }

        protected virtual void OnDeviceStateChanged(object sender, PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(e.PropertyName);
        }

        protected virtual void OnDriverSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected void LogSettingChange(object value, [CallerMemberName] string settingName = null)
        {
            Log.Info(this, $"Set [{settingName}] to [{value}].");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Device = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
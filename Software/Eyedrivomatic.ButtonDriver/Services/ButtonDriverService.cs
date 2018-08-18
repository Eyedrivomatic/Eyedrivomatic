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
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Device;
using Eyedrivomatic.Device.Services;
using Eyedrivomatic.Logging;
using NullGuard;

namespace Eyedrivomatic.ButtonDriver.Services
{
    [Export(typeof(IButtonDriverService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ButtonDriverService : IButtonDriverService
    {
        private readonly IButtonDriverConfigurationService _configurationService;
        private readonly IDeviceService _deviceService;
        private readonly Func<IDevice, IButtonDriver> _buttonDriverFactory;

        [ImportingConstructor]
        public ButtonDriverService(IButtonDriverConfigurationService configurationService, IDeviceService deviceService, Func<IDevice, IButtonDriver> buttonDriverFactory)
        {
            _configurationService = configurationService;
            _deviceService = deviceService;
            _buttonDriverFactory = buttonDriverFactory;
            _deviceService.ConnectedDeviceChanged += DeviceServiceOnConnectedDeviceChanged;
        }

        private void DeviceServiceOnConnectedDeviceChanged(object sender, EventArgs eventArgs)
        {
            var device = _deviceService.ConnectedDevice;
            LoadedButtonDriver = device == null ? null : _buttonDriverFactory(device);
        }

        private IButtonDriver _loadedButtonDriver;
        public event EventHandler LoadedButtonDriverChanged;

        [AllowNull]
        public IButtonDriver LoadedButtonDriver
        {
            get => _loadedButtonDriver;
            set
            {
                _loadedButtonDriver = value;
                ApplySettings();
                LoadedButtonDriverChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Task InitializeAsync()
        {
            Log.Debug(this, "Device Initialization Service Initializing.");
            ApplySettings();
            return Task.CompletedTask;
        }

        private void ApplySettings()
        {
            if (_loadedButtonDriver == null) return;
            _loadedButtonDriver.Profile = _configurationService.CurrentProfile;
        }

        public void Dispose()
        {
            LoadedButtonDriver = null;
        }
    }
}

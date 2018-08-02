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


using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

using System.Threading.Tasks;
using System;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.ButtonDriver.Device.Services
{
    [Export(typeof(IDeviceInitializationService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DeviceInitializationService : IDeviceInitializationService
    {
        private readonly IButtonDriverConfigurationService _configurationService;

        [ImportingConstructor]
        public DeviceInitializationService([ImportMany]IButtonDriver[] availableDrivers, IButtonDriverConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _availableDrivers = new ObservableCollection<IButtonDriver>(availableDrivers);
            LoadedButtonDriver = _availableDrivers.FirstOrDefault();
        }

        public ObservableCollection<IButtonDriver> AvailableDrivers => _availableDrivers;

        private IButtonDriver _loadedButtonDriver;
        private readonly ObservableCollection<IButtonDriver> _availableDrivers;

        public event EventHandler CurrentDriverChanged;

        public IButtonDriver LoadedButtonDriver
        {
            get => _loadedButtonDriver;
            set
            {
                _loadedButtonDriver = value;
                ApplySettings();
                CurrentDriverChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Task InitializeAsync()
        {
            Log.Debug(this, "Device Initialization Service Initializing.");

            //TODO: Find refresh the list of drivers that are actually plugged in.
            ApplySettings();

            return Task.FromResult(true);
        }

        private void ApplySettings()
        {
            if (_loadedButtonDriver == null) return;
            _loadedButtonDriver.Profile = _configurationService.CurrentProfile;
        }

        public void Dispose()
        {
            _loadedButtonDriver?.Dispose();
        }
    }
}

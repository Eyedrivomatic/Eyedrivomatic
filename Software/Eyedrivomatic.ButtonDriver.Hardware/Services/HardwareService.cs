// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;

using System.Threading.Tasks;
using System;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver.Hardware.Services
{
    [Export(typeof(IHardwareService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class HardwareService : IHardwareService
    {
        private readonly IButtonDriverConfigurationService _configurationService;

        [ImportingConstructor]
        public HardwareService([ImportMany]IButtonDriver[] availableDrivers, IButtonDriverConfigurationService configurationService)
        {
            _configurationService = configurationService;
            _availableDrivers = new ObservableCollection<IButtonDriver>(availableDrivers);
            CurrentDriver = _availableDrivers.FirstOrDefault();
        }

        public ObservableCollection<IButtonDriver> AvailableDrivers => _availableDrivers;

        private IButtonDriver _currentDriver;
        private readonly ObservableCollection<IButtonDriver> _availableDrivers;

        public event EventHandler CurrentDriverChanged;

        public IButtonDriver CurrentDriver
        {
            get => _currentDriver;
            set
            {
                _currentDriver = value;
                ApplySettings();
                CurrentDriverChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Task InitializeAsync()
        {
            Log.Debug(this, "Initializing the Hardware Service.");

            //TODO: Find refresh the list of drivers that are actually plugged in.
            ApplySettings();

            return Task.FromResult(true);
        }

        private void ApplySettings()
        {
            if (_currentDriver == null) return;
            _currentDriver.Profile = _configurationService.CurrentProfile;
        }

        public void Dispose()
        {
            _currentDriver?.Dispose();
        }
    }
}

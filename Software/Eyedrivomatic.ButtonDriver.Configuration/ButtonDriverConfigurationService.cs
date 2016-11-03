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


using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

using Prism.Logging;
using Prism.Mvvm;


namespace Eyedrivomatic.ButtonDriver.Configuration
{
    [InheritedExport(typeof(IButtonDriverConfigurationService)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ButtonDriverConfigurationService : BindableBase, IButtonDriverConfigurationService
    {
        private readonly ButtonDriverConfiguration _configuration;
        private bool _hasChanges;

        [Import]
        public ILoggerFacade Logger { get; set; }

        [ImportingConstructor]
        internal ButtonDriverConfigurationService(ButtonDriverConfiguration configuration)
        {
            Contract.Requires<ArgumentNullException>(configuration != null, nameof(configuration));

            _configuration = configuration;
            _configuration.PropertyChanged += Configuration_PropertyChanged;

            if (_configuration.SettingsVersion < 1)
            {
                _configuration.Upgrade();
                _configuration.SettingsVersion = 1;
            }
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AutoConnect) ||
                e.PropertyName == nameof(AutoSaveDeviceSettingsOnExit) ||
                e.PropertyName == nameof(ConnectionString))
            {
                _hasChanges = true;
                OnPropertyChanged(e.PropertyName);
            }
        }

        public bool AutoConnect
        {
            get { return _configuration.AutoConnect; }
            set { _configuration.AutoConnect = value; }
        }

        public bool AutoSaveDeviceSettingsOnExit
        {
            get { return _configuration.AutoSaveDeviceSettingsOnExit; }
            set { _configuration.AutoSaveDeviceSettingsOnExit = value; }
        }

        public string ConnectionString
        {
            get { return _configuration.ConnectionString; }
            set { _configuration.ConnectionString = value; }
        }

        public bool SafetyBypass
        {
            get { return _configuration.SafetyBypass; }
            set { _configuration.SafetyBypass= value; }
        }

        public bool HasChanges => _hasChanges;

        public void Save()
        {
            if (!_hasChanges) return;

            Logger.Log("Saving Changes", Category.Info, Priority.None);

            _configuration.Save();
            _hasChanges = false;
        }
    }
}

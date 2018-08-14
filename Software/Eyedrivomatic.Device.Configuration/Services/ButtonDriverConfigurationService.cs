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
using System.ComponentModel;
using System.ComponentModel.Composition;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.Device.Configuration.Services
{
    public static class ButtonDriverConfigurationProvider
    {
        [Export]
        internal static DeviceConfiguration DefaultConfiguration => DeviceConfiguration.Default;
    }

    [Export(typeof(IDeviceConfigurationService)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class DeviceConfigurationService : BindableBase, IDeviceConfigurationService
    {
        private readonly DeviceConfiguration _configuration;
        private bool _hasChanges;

        [ImportingConstructor]
        internal DeviceConfigurationService(DeviceConfiguration configuration)
        {
            _configuration = configuration;
            _configuration.PropertyChanged += ConfigurationSectionPropertyChanged;
            _configuration.SettingsLoaded += (sender, args) => HasChanges = false;
            _configuration.Upgrade();
            _configuration.WriteToLog();

            HasChanges = false;
        }

        #region Change event handlers
        private void ConfigurationSectionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender == _configuration)
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(args.PropertyName);
                HasChanges = true;
            }
        }
        #endregion Change event handlers

        [Export("DeviceVariant")]
        public string Variant => _configuration.Variant;

        public bool AutoConnect
        {
            get => _configuration.AutoConnect;
            set { if (_configuration.AutoConnect != value) _configuration.AutoConnect = value; }
        }

        public string ConnectionString
        {
            get => _configuration.ConnectionString;
            set { if (_configuration.ConnectionString != value) _configuration.ConnectionString = value; }
        }

        [Export(nameof(CommandTimeout))]
        public TimeSpan CommandTimeout
        {
            get => TimeSpan.FromMilliseconds(_configuration.CommandTimeout);
            set { if (Math.Abs(_configuration.CommandTimeout - value.TotalMilliseconds) >= 1) _configuration.CommandTimeout = value.TotalMilliseconds; }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            private set => SetProperty(ref _hasChanges, value);
        } 

        public void Save()
        {
            if (!HasChanges) return;

            Log.Info(this, "Saving Changes");

            _configuration.Save();
            HasChanges = false;
        }
    }
}

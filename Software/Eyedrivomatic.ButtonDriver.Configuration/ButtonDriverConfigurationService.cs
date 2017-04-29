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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Prism.Logging;
using Prism.Mvvm;


namespace Eyedrivomatic.ButtonDriver.Configuration
{
    public static class ButtonDriverConfigurationProvider
    {
        [Export]
        internal static ButtonDriverConfiguration DefaultConfiguration => ButtonDriverConfiguration.Default;
    }

    [Export(typeof(IButtonDriverConfigurationService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    internal class ButtonDriverConfigurationService : BindableBase, IButtonDriverConfigurationService
    {
        private readonly ButtonDriverConfiguration _configuration;
        private bool _hasChanges;

        [Import]
        public ILoggerFacade Logger { get; set; }

        [ImportingConstructor]
        internal ButtonDriverConfigurationService(ButtonDriverConfiguration configuration)
        {
            _configuration = configuration;
            _configuration.PropertyChanged += ConfigurationSectionPropertyChanged;
            _configuration.DrivingProfiles.CollectionChanged += DrivingProfilesOnCollectionChanged;
            ((INotifyPropertyChanged)_configuration.DrivingProfiles).PropertyChanged += ProfileOnPropertyChanged;

            if (_configuration.SettingsVersion < 1)
            {
                _configuration.Upgrade();
                _configuration.SettingsVersion = 1;
            }
        }

        #region Change event handlers
        private void DrivingProfilesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            HasChanges = true;

            if (args.Action == NotifyCollectionChangedAction.Reset ||
                args.Action == NotifyCollectionChangedAction.Remove ||
                args.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var profile in args.OldItems.Cast<Profile>())
                {
                    profile.PropertyChanged -= ProfileOnPropertyChanged;
                    profile.Speeds.CollectionChanged -= SpeedsOnCollectionChanged;
                    ((INotifyPropertyChanged)profile.Speeds).PropertyChanged += ProfileOnPropertyChanged;

                    if (CurrentProfile == profile) CurrentProfile = DrivingProfiles.FirstOrDefault();
                }
            }

            if (args.Action == NotifyCollectionChangedAction.Add||
                args.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var profile in args.NewItems.Cast<Profile>())
                {
                    profile.PropertyChanged += ProfileOnPropertyChanged;
                    profile.Speeds.CollectionChanged += SpeedsOnCollectionChanged;
                    ((INotifyPropertyChanged)profile.Speeds).PropertyChanged -= ProfileOnPropertyChanged;

                    if (CurrentProfile == null) CurrentProfile = profile;
                }
            }
        }

        private void ProfileOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            HasChanges = true;
        }

        private void SpeedsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            HasChanges = true;

            if (args.Action == NotifyCollectionChangedAction.Reset ||
                args.Action == NotifyCollectionChangedAction.Remove ||
                args.Action == NotifyCollectionChangedAction.Replace)
            {
                var currentSpeed = CurrentProfile?.CurrentSpeed;
                foreach (var profileSpeed in args.OldItems.Cast<ProfileSpeed>())
                {
                    profileSpeed.PropertyChanged -= ProfileSpeedOnPropertyChanged;

                    if (currentSpeed == profileSpeed)
                        CurrentProfile.CurrentSpeed = CurrentProfile.Speeds.FirstOrDefault();
                }
            }

            if (args.Action == NotifyCollectionChangedAction.Add ||
                args.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var profileSpeed in args.NewItems.Cast<ProfileSpeed>())
                {
                    profileSpeed.PropertyChanged += ProfileOnPropertyChanged;
                }
            }
        }

        private void ProfileSpeedOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            HasChanges = true;
        }

        private void ConfigurationSectionPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (sender == _configuration)
            {
                RaisePropertyChanged(args.PropertyName);
            }
        }
        #endregion Change event handlers

        public bool AutoConnect
        {
            get => _configuration.AutoConnect;
            set => _configuration.AutoConnect = value;
        }

        public bool AutoSaveDeviceSettingsOnExit
        {
            get => _configuration.AutoSaveDeviceSettingsOnExit;
            set => _configuration.AutoSaveDeviceSettingsOnExit = value;
        }

        public string ConnectionString
        {
            get => _configuration.ConnectionString;
            set => _configuration.ConnectionString = value;
        }

        public bool SafetyBypass
        {
            get => _configuration.SafetyBypass;
            set => _configuration.SafetyBypass= value;
        }

        [Export(nameof(CommandTimeout))]
        public TimeSpan CommandTimeout
        {
            get => TimeSpan.FromMilliseconds(_configuration.CommandTimeout);
            set => _configuration.CommandTimeout = value.TotalMilliseconds;
        }

        public ObservableCollection<Profile> DrivingProfiles => _configuration.DrivingProfiles;

        public Profile CurrentProfile
        {
            get => _configuration.DrivingProfiles.CurrentProfile;
            set
            {
                if (_configuration.DrivingProfiles.CurrentProfile == value) return;

                _configuration.DrivingProfiles.CurrentProfile = value;
                RaisePropertyChanged();
            }
        }

        public bool HasChanges
        {
            get => _hasChanges;
            private set => SetProperty(ref _hasChanges, value);
        } 

        public void Save()
        {
            if (!HasChanges) return;

            Logger.Log("Saving Changes", Category.Info, Priority.None);

            _configuration.Save();
            HasChanges = false;
        }
    }
}

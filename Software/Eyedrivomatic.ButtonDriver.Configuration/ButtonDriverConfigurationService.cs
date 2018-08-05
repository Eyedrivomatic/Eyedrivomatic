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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Prism.Mvvm;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using NullGuard;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    public static class ButtonDriverConfigurationProvider
    {
        [Export]
        internal static ButtonDriverConfiguration DefaultConfiguration => ButtonDriverConfiguration.Default;
    }

    [Export(typeof(IButtonDriverConfigurationService)), PartCreationPolicy(CreationPolicy.Shared)]
    internal class ButtonDriverConfigurationService : BindableBase, IButtonDriverConfigurationService
    {
        private readonly ButtonDriverConfiguration _configuration;
        private bool _hasChanges;

        [ImportingConstructor]
        internal ButtonDriverConfigurationService(ButtonDriverConfiguration configuration)
        {
            _configuration = configuration;
            _configuration.PropertyChanged += ConfigurationSectionPropertyChanged;
            _configuration.SettingsLoaded += (sender, args) => HasChanges = false;
            _configuration.Upgrade();
            _configuration.WriteToLog();

            _configuration.DrivingProfiles.CollectionChanged += DrivingProfilesOnCollectionChanged;
            ((INotifyPropertyChanged)_configuration.DrivingProfiles).PropertyChanged += ProfileOnPropertyChanged;

            foreach (var profile in _configuration.DrivingProfiles)
            {
                profile.PropertyChanged += ProfileOnPropertyChanged;
                profile.Speeds.CollectionChanged += SpeedsOnCollectionChanged;
            }
            foreach (var speed in _configuration.DrivingProfiles.SelectMany(p => p.Speeds))
            {
                speed.PropertyChanged += ProfileSpeedOnPropertyChanged;
            }

            HasChanges = false;
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
                    ((INotifyPropertyChanged)profile.Speeds).PropertyChanged -= ProfileOnPropertyChanged;
                    foreach (var speed in profile.Speeds)
                    {
                        speed.PropertyChanged -= ProfileSpeedOnPropertyChanged;
                    }

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
                    ((INotifyPropertyChanged)profile.Speeds).PropertyChanged += ProfileOnPropertyChanged;
                    foreach (var speed in profile.Speeds)
                    {
                        speed.PropertyChanged += ProfileSpeedOnPropertyChanged;
                    }

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

            if (CurrentProfile != null && (
                args.Action == NotifyCollectionChangedAction.Reset ||
                args.Action == NotifyCollectionChangedAction.Remove ||
                args.Action == NotifyCollectionChangedAction.Replace))
            {
                var currentSpeed = CurrentProfile.CurrentSpeed;
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
                    profileSpeed.PropertyChanged += ProfileSpeedOnPropertyChanged;
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

        public bool SafetyBypass
        {
            get => _configuration.SafetyBypass;
            set { if (_configuration.SafetyBypass != value) _configuration.SafetyBypass = value; }
        }

        [Export(nameof(CommandTimeout))]
        public TimeSpan CommandTimeout
        {
            get => TimeSpan.FromMilliseconds(_configuration.CommandTimeout);
            set { if (Math.Abs(_configuration.CommandTimeout - value.TotalMilliseconds) >= 1) _configuration.CommandTimeout = value.TotalMilliseconds; }
        }

        [Export(typeof(IEnumerable<Profile>))]
        public ObservableCollection<Profile> DrivingProfiles => _configuration.DrivingProfiles;

        [AllowNull]
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

            Log.Info(this, "Saving Changes");

            _configuration.Save();
            HasChanges = false;
        }
    }
}

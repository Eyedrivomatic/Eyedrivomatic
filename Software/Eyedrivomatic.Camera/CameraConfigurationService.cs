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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Accord.Video.DirectShow;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Prism.Mvvm;
using NullGuard;

namespace Eyedrivomatic.Camera
{
    public static class DefaultConfigurationProvider
    {
        [Export]
        internal static CameraConfiguration DefaultConfiguration => CameraConfiguration.Default;
    }

    [Export(typeof(ICameraConfigurationService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class CameraConfigurationService : BindableBase, ICameraConfigurationService
    {
        private readonly CameraConfiguration _configuration;
        private readonly Func<IEnumerable<FilterInfo>> _getCameras;

        [ImportingConstructor]
        internal CameraConfigurationService(CameraConfiguration configuration, [Import("GetCameras")] Func<IEnumerable<FilterInfo>> getCameras)
        {
            _configuration = configuration;
            _getCameras = getCameras;
            _configuration.PropertyChanged += Configuration_PropertyChanged;
            _configuration.SettingsLoaded += (sender, args) => HasChanges = false;
            _configuration.Upgrade();
            _configuration.WriteToLog();

            HasChanges = false;
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CameraConfiguration.CameraEnabled) ||
                e.PropertyName == nameof(CameraConfiguration.Camera) ||
                e.PropertyName == nameof(CameraConfiguration.OverlayOpacity) ||
                e.PropertyName == nameof(CameraConfiguration.Stretch))
            {
                HasChanges = true;
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(e.PropertyName);
            }
        }

        public bool CameraEnabled
        {
            get => _configuration.CameraEnabled;
            set => _configuration.CameraEnabled = value;
        }

        [AllowNull]
        public FilterInfo Camera
        {
            get => AvailableCameras.FirstOrDefault(fi => fi.MonikerString == _configuration.Camera) ?? AvailableCameras.FirstOrDefault();
            set => _configuration.Camera = value?.MonikerString;
        }

        public double OverlayOpacity
        {
            get => _configuration.OverlayOpacity;
            set => _configuration.OverlayOpacity = value;
        }

        public Stretch Stretch
        {
            get => (Stretch)Enum.Parse(typeof(Stretch), _configuration.Stretch);
            set => _configuration.Stretch = value.ToString();
        }

        public IEnumerable<FilterInfo> AvailableCameras => _getCameras();

        private bool _hasChanges;
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

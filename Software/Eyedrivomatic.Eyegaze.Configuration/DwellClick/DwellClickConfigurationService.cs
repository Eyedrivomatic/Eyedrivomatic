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


using System.ComponentModel;
using System.ComponentModel.Composition;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Infrastructure;
using Prism.Mvvm;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Eyegaze.Configuration.DwellClick
{
    public static class DefaultConfigurationProvider
    {
        [Export]
        internal static DwellClickConfiguration DefaultConfiguration => DwellClickConfiguration.Default;
    }


    [Export(typeof(IDwellClickConfigurationService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class DwellClickConfigurationService : BindableBase, IDwellClickConfigurationService
    {
        private readonly DwellClickConfiguration _configuration;

        [ImportingConstructor]
        internal DwellClickConfigurationService(DwellClickConfiguration configuration)
        {
            _configuration = configuration;
            _configuration.PropertyChanged += Configuration_PropertyChanged;
            _configuration.SettingsLoaded += (sender, args) => HasChanges = false;
            _configuration.Upgrade();
            _configuration.WriteToLog();

            HasChanges = false;
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Log.Debug(this, $"Configuration property [{e.PropertyName}] changed.");

            if (e.PropertyName == nameof(_configuration.EnableDwellClick) ||
                e.PropertyName == nameof(_configuration.Provider) ||
                e.PropertyName.EndsWith("DwellTimeMilliseconds") ||
                e.PropertyName == nameof(_configuration.DwellTimeoutMilliseconds) ||
                e.PropertyName == nameof(_configuration.RepeatDelayMilliseconds) )
            {
                HasChanges = true;
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(e.PropertyName);
            }
        }

        public bool EnableDwellClick
        {
            get => _configuration.EnableDwellClick;
            set => _configuration.EnableDwellClick = value;
        }

        public string Provider
        {
            get => _configuration.Provider;
            set => _configuration.Provider = value;
        }

        public int StandardDwellTimeMilliseconds
        {
            get => _configuration.StandardDwellTimeMilliseconds;
            set => _configuration.StandardDwellTimeMilliseconds = value;
        }

        public int DirectionButtonDwellTimeMilliseconds
        {
            get => _configuration.DirectionButtonDwellTimeMilliseconds;
            set => _configuration.DirectionButtonDwellTimeMilliseconds = value;
        }

        public int StopButtonDwellTimeMilliseconds
        {
            get => _configuration.StopButtonDwellTimeMilliseconds;
            set => _configuration.StopButtonDwellTimeMilliseconds = value;
        }

        public int StartButtonDwellTimeMilliseconds
        {
            get => _configuration.StartButtonDwellTimeMilliseconds;
            set => _configuration.StartButtonDwellTimeMilliseconds = value;
        }

        public int DwellTimeoutMilliseconds
        {
            get => _configuration.DwellTimeoutMilliseconds;
            set => _configuration.DwellTimeoutMilliseconds = value;
        }

        public int RepeatDelayMilliseconds
        {
            get => _configuration.RepeatDelayMilliseconds;
            set => _configuration.RepeatDelayMilliseconds = value;
        }

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

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
//    Eyedrivomaticis distributed in the hope that it will be useful,
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

using Eyedrivomatic.Controls;

namespace Eyedrivomatic.Configuration
{
    public static class DefaultConfigurationProvider
    {
        [Export]
        internal static DwellClickConfiguration DefaultConfiguration => DwellClickConfiguration.Default;
    }

    [InheritedExport(typeof(IDwellClickConfigurationService)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DwellClickConfigurationService : BindableBase, IDwellClickConfigurationService
    {
        private readonly DwellClickConfiguration _configuration;
        private bool _hasChanges;

        [Import]
        public ILoggerFacade Logger { get; set; }

        [ImportingConstructor]
        internal DwellClickConfigurationService(DwellClickConfiguration configuration)
        {
            Contract.Requires<ArgumentNullException>(configuration != null, nameof(configuration));

            _configuration = configuration;
            _configuration.PropertyChanged += Configuration_PropertyChanged;
            _configuration.Upgrade();
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EnableDwellClick) ||
                e.PropertyName == nameof(DwellTimeMilliseconds) ||
                e.PropertyName == nameof(DwellTimeoutMilliseconds) ||
                e.PropertyName == nameof(RepeatDelayMilliseconds) )
            {
                _hasChanges = true;
                OnPropertyChanged(e.PropertyName);
            }
        }

        public bool EnableDwellClick
        {
            get { return _configuration.EnableDwellClick; }
            set { _configuration.EnableDwellClick = value; }
        }

        public int DwellTimeMilliseconds
        {
            get { return _configuration.DwellTimeMilliseconds; }
            set { _configuration.DwellTimeMilliseconds = value; }
        }

        public int DwellTimeoutMilliseconds
        {
            get { return _configuration.DwellTimeoutMilliseconds; }
            set { _configuration.DwellTimeoutMilliseconds = value; }
        }

        public int RepeatDelayMilliseconds
        {
            get { return _configuration.RepeatDelayMilliseconds; }
            set { _configuration.RepeatDelayMilliseconds = value; }
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

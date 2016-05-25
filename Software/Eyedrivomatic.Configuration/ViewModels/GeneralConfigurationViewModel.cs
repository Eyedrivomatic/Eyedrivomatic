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
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

using Prism.Commands;
using Prism.Mvvm;

using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.Configuration.ViewModels
{
    [Export]
    public class GeneralConfigurationViewModel : BindableBase, IHeaderInfoProvider<string>
    {
        private readonly IDwellClickConfigurationService _dwellClickConfigurationService;

        public string HeaderInfo => Strings.ViewName_GeneralConfiguration;

        [ImportingConstructor]
        public GeneralConfigurationViewModel(IDwellClickConfigurationService dwellClickConfigurationService)
        {
            Contract.Requires<ArgumentNullException>(dwellClickConfigurationService != null, nameof(dwellClickConfigurationService));

            _dwellClickConfigurationService = dwellClickConfigurationService;
            _dwellClickConfigurationService.PropertyChanged += DwellClickConfiguration_PropertyChanged;

            SaveCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
        }

        private void DwellClickConfiguration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged();

            SaveCommand.RaiseCanExecuteChanged();
        }

        public DelegateCommand SaveCommand { get; }

        public bool DwellClickEnabled
        {
            get { return _dwellClickConfigurationService.EnableDwellClick; }
            set { _dwellClickConfigurationService.EnableDwellClick = value; }
        }

        public int DwellTimeMilliseconds
        {
            get { return _dwellClickConfigurationService.DwellTimeMilliseconds; }
            set { _dwellClickConfigurationService.DwellTimeMilliseconds = value; }
        }

        public int DwellTimeoutMilliseconds
        {
            get { return _dwellClickConfigurationService.DwellTimeoutMilliseconds; }
            set { _dwellClickConfigurationService.DwellTimeoutMilliseconds = value; }
        }

        public int DwellRepeatDelayMilliseconds
        {
            get { return _dwellClickConfigurationService.RepeatDelayMilliseconds; }
            set { _dwellClickConfigurationService.RepeatDelayMilliseconds = value; }
        }

        protected void SaveChanges()
        {
            _dwellClickConfigurationService.Save();
            SaveCommand.RaiseCanExecuteChanged();
        }

        protected bool CanSaveChanges()
        {
            return _dwellClickConfigurationService.HasChanges;
        }
    }
}

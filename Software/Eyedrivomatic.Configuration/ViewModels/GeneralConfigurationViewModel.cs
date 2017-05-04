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


using System.ComponentModel.Composition;

using Prism.Commands;
using Prism.Mvvm;

using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

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
            _dwellClickConfigurationService = dwellClickConfigurationService;
            _dwellClickConfigurationService.PropertyChanged += DwellClickConfiguration_PropertyChanged;
        }

        private void DwellClickConfiguration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(string.Empty);
        }

        public bool DwellClickEnabled
        {
            get => _dwellClickConfigurationService.EnableDwellClick;
            set => _dwellClickConfigurationService.EnableDwellClick = value;
        }

        public int DwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.DwellTimeMilliseconds;
            set => _dwellClickConfigurationService.DwellTimeMilliseconds = value;
        }

        public int DwellTimeoutMilliseconds
        {
            get => _dwellClickConfigurationService.DwellTimeoutMilliseconds;
            set => _dwellClickConfigurationService.DwellTimeoutMilliseconds = value;
        }

        public int DwellRepeatDelayMilliseconds
        {
            get => _dwellClickConfigurationService.RepeatDelayMilliseconds;
            set => _dwellClickConfigurationService.RepeatDelayMilliseconds = value;
        }
    }
}

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
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using Prism.Mvvm;

namespace Eyedrivomatic.Eyegaze.ViewModels
{
    [Export]
    public class EyegazeConfigruationViewModel : BindableBase, IHeaderInfoProvider<string>
    {
        private readonly IDwellClickConfigurationService _dwellClickConfigurationService;

        public string HeaderInfo => Strings.ViewName_GeneralConfiguration;

        [ImportingConstructor]
        public EyegazeConfigruationViewModel(IDwellClickConfigurationService dwellClickConfigurationService)
        {
            _dwellClickConfigurationService = dwellClickConfigurationService;
            _dwellClickConfigurationService.PropertyChanged += DwellClickConfiguration_PropertyChanged;
        }

        private void DwellClickConfiguration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }

        public bool DwellClickEnabled
        {
            get => _dwellClickConfigurationService.EnableDwellClick;
            set => _dwellClickConfigurationService.EnableDwellClick = value;
        }

        public int StandardDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.StandardDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.StandardDwellTimeMilliseconds = value;
        }

        public int StartButtonDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.StartButtonDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.StartButtonDwellTimeMilliseconds = value;
        }

        public int StopButtonDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.StopButtonDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.StopButtonDwellTimeMilliseconds = value;
        }

        public int DirectionButtonsDwellTimeMilliseconds
        {
            get => _dwellClickConfigurationService.DirectionButtonDwellTimeMilliseconds;
            set => _dwellClickConfigurationService.DirectionButtonDwellTimeMilliseconds = value;
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

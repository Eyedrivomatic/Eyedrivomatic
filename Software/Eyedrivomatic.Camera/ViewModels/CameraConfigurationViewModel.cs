// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Camera Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomatic is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Camera Public License for more details.
//
//    You should have received a copy of the GNU Camera Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Accord.Video.DirectShow;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using Prism.Commands;
using Prism.Mvvm;

namespace Eyedrivomatic.Camera.ViewModels
{
    [Export]
    public class CameraConfigurationViewModel : BindableBase, IHeaderInfoProvider<string>
    {
        private readonly ICameraConfigurationService _cameraConfigurationService;

        public string HeaderInfo => Strings.ViewName_CameraConfiguration;

        [ImportingConstructor]
        public CameraConfigurationViewModel(ICameraConfigurationService cameraConfigurationService)
        {
            _cameraConfigurationService = cameraConfigurationService;
            _cameraConfigurationService.PropertyChanged += CameraConfigurationPropertyChanged;
        }

        private void CameraConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }

        public FilterInfo Camera
        {
            get => _cameraConfigurationService.Camera;
            set => _cameraConfigurationService.Camera = value;
        }

        public IEnumerable<FilterInfo> AvailableCameras => _cameraConfigurationService.AvailableCameras;

        public ICommand SaveCommand => new DelegateCommand(() => _cameraConfigurationService.Save(), () => _cameraConfigurationService.HasChanges);
    }
}

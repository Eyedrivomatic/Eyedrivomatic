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
using Prism.Mvvm;
using Eyedrivomatic.ButtonDriver.Hardware.Services;


namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public abstract class ButtonDriverViewModelBase : BindableBase
    {
        private IButtonDriver _driver;
        protected IHardwareService HardwareService { get; }

        protected IButtonDriver Driver
        {
            get => _driver;
            private set
            {
                if (_driver != null) _driver.PropertyChanged -= OnDriverStatusChanged;
                SetProperty(ref _driver, value);
                if (_driver != null) _driver.PropertyChanged += OnDriverStatusChanged;
            }
        }

        protected ButtonDriverViewModelBase(IHardwareService hardwareService)
        {
            hardwareService.CurrentDriverChanged += (sender, args) => Driver = hardwareService.CurrentDriver;
            Driver = hardwareService.CurrentDriver;
        }

        protected virtual void OnDriverStatusChanged(object sender, EventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }
    }
}

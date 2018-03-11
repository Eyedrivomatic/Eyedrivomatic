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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Prism.Mvvm;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Logging;


namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public abstract class ButtonDriverViewModelBase : BindableBase, IDisposable
    {
        private IButtonDriver _driver;

        protected IHardwareService HardwareService { get; }

        protected IButtonDriver Driver
        {
            get => _driver;
            private set
            {
                if (_driver != null)
                {
                    _driver.PropertyChanged -= OnDriverStateChanged;
                    _driver.DeviceSettings.PropertyChanged -= OnDriverSettingsChanged;
                }
                SetProperty(ref _driver, value);
                if (_driver != null)
                {
                    _driver.PropertyChanged += OnDriverStateChanged;
                    _driver.DeviceSettings.PropertyChanged += OnDriverSettingsChanged;
                }
            }
        }

        protected ButtonDriverViewModelBase(IHardwareService hardwareService)
        {
            HardwareService = hardwareService;
            HardwareService.CurrentDriverChanged += (sender, args) => Driver = hardwareService.CurrentDriver;
            Driver = HardwareService.CurrentDriver;
        }

        protected virtual void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(e.PropertyName);
        }

        protected virtual void OnDriverSettingsChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        protected void LogSettingChange(object value, [CallerMemberName] string settingName = null)
        {
            Log.Info(this, $"Set [{settingName}] on [{Driver.Profile.Name}] to [{value}].");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Driver = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

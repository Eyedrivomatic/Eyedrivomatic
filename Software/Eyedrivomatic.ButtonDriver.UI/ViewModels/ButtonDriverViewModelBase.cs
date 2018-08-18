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
using Eyedrivomatic.ButtonDriver.Services;
using Eyedrivomatic.Logging;
using Prism.Mvvm;

namespace Eyedrivomatic.ButtonDriver.UI.ViewModels
{
    public abstract class ButtonDriverViewModelBase : BindableBase, IDisposable
    {
        protected readonly IButtonDriver Driver;

        protected ButtonDriverViewModelBase(IButtonDriver driver)
        {
            Driver = driver;
            Driver.PropertyChanged += OnDriverStateChanged;
        }

        protected virtual void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(e.PropertyName);
        }

        protected void LogSettingChange(object value, [CallerMemberName] string settingName = null)
        {
            Log.Info(this, $"Set [{settingName}] on [{Driver.Profile.Name}] to [{value}].");
        }

        protected virtual void Dispose(bool disposing)
        {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

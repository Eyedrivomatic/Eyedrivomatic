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


using System.Windows;

using System;
using System.Globalization;
using System.Threading;

using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Startup;

namespace Eyedrivomatic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : Application, IDisposable
    {
        private readonly Bootstrapper _bootstrapper = new Bootstrapper();

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Initialize();
            Log.Info(this, "Application Startup");

            //Thread.CurrentThread.CurrentCulture = new CultureInfo("de");
            //Thread.CurrentThread.CurrentUICulture = new CultureInfo("de");

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += OnUnhandledException;

            base.OnStartup(e);
            _bootstrapper.Run();
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Error(this, $"Unhandled Exception! - {e.ExceptionObject}");
        }

        public void Dispose()
        {
            _bootstrapper.Dispose();
        }
    }
   
}

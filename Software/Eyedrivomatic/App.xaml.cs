﻿//	Copyright (c) 2018 Eyedrivomatic Authors
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
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Startup;

namespace Eyedrivomatic
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : IDisposable
    {
        private readonly Bootstrapper _bootstrapper = new Bootstrapper();

        public void Dispose()
        {
            _bootstrapper.Dispose();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Initialize();
            Log.Info(this, $"Application Startup - Version: {Assembly.GetExecutingAssembly().GetName().Version}");

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += OnUnhandledException;
            Dispatcher.UnhandledException += OnDispatcherUnhandledException;
            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

            base.OnStartup(e);
            _bootstrapper.Run();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject as Exception);
            if (e.IsTerminating) Log.Warn(nameof(App), "Exception has caused application to terminate.");
        }

        private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }

        private static void HandleException(Exception exception)
        {
            Log.Error(typeof(App), $"Unhandled Exception! - [{exception}]");

            var message = "Unhandled Exception!" + Environment.NewLine;
            var i = 0;
            
            while (exception != null)
            {
                message += new string(' ', i++) + exception.Message + Environment.NewLine;
                exception = exception.InnerException;
            }

            MessageBox.Show(message, "Exception!", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Info(nameof(App), $"Exiting - Exit Code: [{e.ApplicationExitCode}]");
            Dispose();
            base.OnExit(e);
        }
    }
}
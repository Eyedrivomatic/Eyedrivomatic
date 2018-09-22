using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic
{
    [Export(typeof(IAutoUpdateService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class AutoUpdateService : IDisposable, IAutoUpdateService
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly List<Type> _errors = new List<Type>();
        private readonly Application _application;

        [ImportingConstructor]
        public AutoUpdateService() : this(Application.Current)
        { }

        public AutoUpdateService(Application application)
        {
            _application = application;
            _timer.Tick += TimerOnTick;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
        }

        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {

            if (args == null)
            {
                Log.Warn(this, "Failed to check for update");
                return;
            }

            if (args.IsUpdateAvailable)
            {
                Log.Info(this, $"An update is available! This Version: [{args.InstalledVersion}], Available Version: [{args.CurrentVersion}], Manditory: [{args.Mandatory}], URI: [{args.DownloadURL}]");
                AutoUpdater.ShowUpdateForm();
            }
            else
            {
                Log.Debug(this, $"No new update is available! This Version: [{args.InstalledVersion}], Available Version: [{args.CurrentVersion}]");
            }
        }

        public void Start(TimeSpan checkInterval)
        {
            TimerOnTick(this, EventArgs.Empty); //check once now.

            if (checkInterval > TimeSpan.Zero)
            {
                _timer.Interval = checkInterval;
                _timer.Start();
            }
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            Log.Info(this, "Auto update starting. Closing application...");
            _application.Shutdown(1);
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            try
            {
                AutoUpdater.Start($"https://eyedrivomatic.org/Download/ApplicationVersions.{CultureInfo.CurrentUICulture.TwoLetterISOLanguageName}.xml");
                _errors.Clear();
            }
            catch (Exception e)
            {
                //prevent bombing the log with non-stop errors if systemic failure. One of each error type thrown is enough.
                if (_errors.Contains(e.GetType())) return;
                _errors.Add(e.GetType());

                Log.Error(this, $"Auto-update check failed - [{e}]");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AutoUpdateService()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
                _timer.Stop();
        }
    }
}

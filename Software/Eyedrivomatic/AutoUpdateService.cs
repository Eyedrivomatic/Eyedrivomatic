using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic
{
    public class AutoUpdateService : IDisposable
    {
        private readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly List<Type> _errors = new List<Type>();
        private readonly Application _application;

        public AutoUpdateService(Application application)
        {
            _application = application;
            _timer.Interval = TimeSpan.FromMinutes(10);
            _timer.Tick += TimerOnTick;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
            _timer.Start();
        }

        private void AutoUpdater_ApplicationExitEvent()
        {
            Log.Info(this, "Auto update starting. Closing application...");
            Thread.Sleep(5000);
            _application.Shutdown(1);
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            try
            {
                AutoUpdater.Start("https://eyedrivomatic.org/Download/ApplicationVersions.xml");
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
            _timer.Stop();
        }
    }
}

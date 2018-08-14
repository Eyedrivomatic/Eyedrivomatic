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
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;
using Eyedrivomatic.ButtonDriver;
using Prism.Logging;
using Prism.Mef;
using Prism.Modularity;

using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros;
using Eyedrivomatic.ButtonDriver.UI;
using Eyedrivomatic.Camera;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Eyegaze;
using Eyedrivomatic.Eyegaze.Configuration;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Configuration;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.Startup
{
    public sealed class Bootstrapper : MefBootstrapper, IDisposable
    { 
        protected override ILoggerFacade CreateLogger()
        {
            return new PrismLogger();
        }

        protected override DependencyObject CreateShell()
        {
            return Container.GetExportedValue<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow?.Show();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Bootstrapper).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CommonUiModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ResourcesModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ControlsModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(EyegazeModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(EyegazeConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(DeviceConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MacrosModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(CameraModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverUiModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(IDeviceConnection).Assembly));
        }

        #region IDisposable Support
        private bool _disposed;
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                var modules = Container.GetExportedValues<IModule>().OfType<IDisposable>();
                foreach (var module in modules)
                {
                    Log.Debug(this, $"Disposing [{module.GetType().Name}]");
                    module.Dispose();
                }

                (Shell as IDisposable)?.Dispose();
                AggregateCatalog.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
#endregion
    }
}

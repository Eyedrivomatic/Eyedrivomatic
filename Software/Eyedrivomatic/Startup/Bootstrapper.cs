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
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Windows;

using Microsoft.Practices.ServiceLocation;

using Prism.Logging;
using Prism.Mef;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver;
using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.Startup
{
    public sealed class Bootstrapper : MefBootstrapper, IDisposable
    { 
        protected override ILoggerFacade CreateLogger()
        {
            return new Log4NetLogger();
        }

        protected override DependencyObject CreateShell()
        {
            return ServiceLocator.Current.GetInstance<Shell>();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();

            Application.Current.MainWindow = (Window)Shell;
            Application.Current.MainWindow.Show();
        }

        private void AddModule<T>()
        {
            var type = typeof(T);
            ModuleCatalog.AddModule(new ModuleInfo(type.Name, type.AssemblyQualifiedName));
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
        }

        protected override void ConfigureAggregateCatalog()
        {
            base.ConfigureAggregateCatalog();

            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Bootstrapper).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(InfrastructureModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ControlsModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverHardwareModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(MacrosModule).Assembly));
        }

        protected override IRegionBehaviorFactory ConfigureDefaultRegionBehaviors()
        {
            return base.ConfigureDefaultRegionBehaviors();
        }


        #region IDisposable Support
        private bool _disposed = false;
        void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                var modules = ServiceLocator.Current.GetAllInstances<IModule>().OfType<IDisposable>();
                foreach (var module in modules)
                {
                    Logger?.Log($"Disposing [{module.GetType().Name}]", Category.Debug, Priority.None);
                    module.Dispose();
                }
            }

            (Shell as Shell)?.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}

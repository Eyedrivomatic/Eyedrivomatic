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
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System.ComponentModel.Composition.Hosting;
using System.Windows;

using Microsoft.Practices.ServiceLocation;

using Prism.Mef;
using Prism.Modularity;
using Prism.Logging;

using Eyedrivomatic.ButtonDriver;
using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.Controls;

namespace Eyedrivomatic.Startup
{
    public class Bootstrapper : MefBootstrapper
    { 
        protected override ILoggerFacade CreateLogger()
        {
            var logger = new Log4NetLogger();
            DwellClickAdorner.Logger = logger;
            DwellClickBehavior.Logger = logger;
            return logger;
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

        protected override void ConfigureModuleCatalog()
        {
            base.ConfigureModuleCatalog();

            var type = typeof(ButtonDriverConfigurationModule);
            ModuleCatalog.AddModule(new ModuleInfo(type.Name, type.AssemblyQualifiedName));

            type = typeof(ButtonDriverHardwareModule);
            ModuleCatalog.AddModule(new ModuleInfo(type.Name, type.AssemblyQualifiedName));

            type = typeof(ButtonDriverModule);
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
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverHardwareModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(ButtonDriverConfigurationModule).Assembly));
            AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(DwellClickBehavior).Assembly));
        }

    }
}

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


using System;

using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Logging;
using Prism.Regions;

using Eyedrivomatic.Configuration.Views;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.Configuration
{
    [ModuleExport(typeof(ConfigurationModule), InitializationMode = InitializationMode.WhenAvailable, DependsOnModuleNames = new[] { nameof(InfrastructureModule) })]
    public class ConfigurationModule : IModule
    {
        private readonly IRegionManager RegionManager;
        private readonly ILoggerFacade Logger;

        [ImportingConstructor]
        public ConfigurationModule(IRegionManager regionManager, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(regionManager != null, nameof(regionManager));

            Logger = logger;
            Logger?.Log($"Creating Module {nameof(ConfigurationModule)}.", Category.Info, Priority.None);

            RegionManager = regionManager;
        }

        [Import]
        public IDwellClickConfigurationService DwellClickConfigurationService { get; set; }

        public void Initialize()
        {
            DwellClickBehavior.DefaultConfiguration = DwellClickConfigurationService;

            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(ConfigurationView));
            RegionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(GeneralConfigurationView));
            RegionManager.RegisterViewWithRegion(RegionNames.SleepButtonRegion, typeof(SleepButton));
        }
    }
}

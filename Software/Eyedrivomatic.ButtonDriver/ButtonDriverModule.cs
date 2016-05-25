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

using Prism.Logging;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Views;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver
{
    [ModuleExport(typeof(ButtonDriverModule), DependsOnModuleNames = new [] { nameof(ButtonDriverHardwareModule), nameof(ButtonDriverConfigurationModule), nameof(InfrastructureModule) }, InitializationMode = InitializationMode.WhenAvailable)]
    public class ButtonDriverModule : IModule
    {
        private readonly IHardwareService HardwareService;
        private readonly IButtonDriverConfigurationService ConfigurationService;
        private readonly IRegionManager RegionManager;
        private readonly ILoggerFacade Logger;

        [ImportingConstructor]
        public ButtonDriverModule(IRegionManager regionManager, IHardwareService hardwareService, IButtonDriverConfigurationService configurationService, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(regionManager != null, nameof(regionManager));
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));
            Contract.Requires<ArgumentNullException>(configurationService != null, nameof(configurationService));

            Logger = logger;
            Logger?.Log($"Creating Module {nameof(ButtonDriverModule)}.", Category.Info, Priority.None);

            RegionManager = regionManager;
            HardwareService = hardwareService;
            ConfigurationService = configurationService;
        }

        public async void Initialize()
        {
            Logger?.Log($"Initializing Module {nameof(ButtonDriverModule)}.", Category.Info, Priority.None);

            RegionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusView));

            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(OutdoorDrivingView));
            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(TrimView));
            RegionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(ConfigurationView));

            try
            {
                await HardwareService.InitializeAsync();

                if (ConfigurationService.AutoConnect && !string.IsNullOrWhiteSpace(ConfigurationService.ConnectionString))
                {
                    Logger?.Log($"Navigating to \"{nameof(OutdoorDrivingView)}\".", Category.Debug, Priority.None);
                    RegionManager.RequestNavigate(RegionNames.GridRegion, nameof(OutdoorDrivingView));

                    await HardwareService.CurrentDriver?.ConnectAsync(ConfigurationService.ConnectionString);
                }

                if (!HardwareService.CurrentDriver?.IsConnected ?? false)
                {
                    Logger?.Log($"Navigating to \"{nameof(ConfigurationView)}\".", Category.Debug, Priority.None);
                    RegionManager.RequestNavigate(RegionNames.GridRegion, nameof(ConfigurationView));

                }

            }
            catch (Exception ex)
            {
                Logger?.Log($"Hardware Initialization Failed - {ex}", Category.Exception, Priority.None);
                //TODO: Open the configuration page.
            }

        }
    }
}

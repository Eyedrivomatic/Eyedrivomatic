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
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;

using Prism.Logging;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Macros;
using Eyedrivomatic.ButtonDriver.Views;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.ButtonDriver
{
    [ModuleExport(typeof(ButtonDriverModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(ButtonDriverHardwareModule), nameof(ButtonDriverConfigurationModule), nameof(InfrastructureModule), nameof(MacrosModule) })]
    public class ButtonDriverModule : IModule, IDisposable
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
            Logger?.Log($"Creating Module {nameof(ButtonDriverModule)}.", Category.Debug, Priority.None);

            RegionManager = regionManager;
            HardwareService = hardwareService;
            ConfigurationService = configurationService;
        }

        public async void Initialize()
        {
            Logger?.Log($"Initializing Module {nameof(ButtonDriverModule)}.", Category.Debug, Priority.None);

            RegionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusView));

            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(OutdoorDrivingView));
            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(TrimView));
            RegionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(DeviceConfigurationView));

            try
            {
                await HardwareService.InitializeAsync();

                Logger?.Log($"HardwareService Initialized. AutoConnect: [{ConfigurationService.AutoConnect}]", Category.Debug, Priority.None);
                if (ConfigurationService.AutoConnect)
                {
                    NavigateToDriver();

                    var connectionString = ConfigurationService.ConnectionString;
                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        Logger.Log($"Connection string: [{connectionString}]", Category.Info, Priority.None);
                        await HardwareService.CurrentDriver?.ConnectAsync(connectionString);
                    }
                    else
                    {
                        Logger?.Log("Connection string not specified. Attempting to auto-detect.", Category.Warn, Priority.None);
                        await HardwareService?.CurrentDriver.AutoDetectDeviceAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger?.Log($"Hardware Initialization Failed - {ex}", Category.Exception, Priority.None);
            }

            if (!HardwareService.CurrentDriver?.IsConnected ?? false) NavigateToConfiguration();
        }

        private void NavigateToConfiguration()
        {
            Logger?.Log($"Navigating to [ConfigurationView]->[{nameof(DeviceConfigurationView)}].", Category.Debug, Priority.None);
            RegionManager.RequestNavigate(RegionNames.GridRegion, "ConfigurationView");
            RegionManager.RequestNavigate(RegionNames.ConfigurationRegion, nameof(DeviceConfigurationView));
        }

        private void NavigateToDriver()
        {
            Logger?.Log($"Navigating to [{nameof(OutdoorDrivingView)}].", Category.Debug, Priority.None);
            RegionManager.RequestNavigate(RegionNames.GridRegion, nameof(OutdoorDrivingView));
        }

        public void Dispose()
        {
            if (ConfigurationService.AutoSaveDeviceSettingsOnExit)
            {
                Logger?.Log("Auto saving device settings.", Category.Debug, Priority.None);
                HardwareService.CurrentDriver?.SaveSettings();
            }
            HardwareService?.Dispose();
        }
    }
}

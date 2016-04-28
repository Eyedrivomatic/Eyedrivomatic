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
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.ButtonDriver.Views;
using Prism.Logging;

namespace Eyedrivomatic.ButtonDriver
{
    [ModuleExport(typeof(ButtonDriverModule), DependsOnModuleNames = new [] { nameof(ButtonDriverHardwareModule), "ApplicationSettingsModule" }, InitializationMode = InitializationMode.WhenAvailable)]
    public class ButtonDriverModule : IModule
    {
        private readonly IHardwareService HardwareService;
        private readonly IRegionManager RegionManager;
        private readonly ILoggerFacade Logger;

        [Import("DeviceConnectionString", AllowDefault = true, AllowRecomposition = true)]
        public string ConnectionString { get; set; }

        [Import("AutoConnect", AllowDefault = true, AllowRecomposition = true)]
        public bool AutoConnect { get; set; }

        [ImportingConstructor]
        public ButtonDriverModule(IRegionManager regionManager, IHardwareService hardwareService, ILoggerFacade logger)
        {
            Contract.Requires<ArgumentNullException>(regionManager != null, nameof(regionManager));
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));

            Logger = logger;
            Logger?.Log($"Creating Module {nameof(ButtonDriverModule)}.", Category.Info, Priority.None);

            RegionManager = regionManager;
            HardwareService = hardwareService;
        }

        public async void Initialize()
        {
            Logger?.Log($"Initializing Module {nameof(ButtonDriverModule)}.", Category.Info, Priority.None);

            RegionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusView));

            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(OutdoorDrivingView));
            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(TrimView));
            RegionManager.RegisterViewWithRegion(RegionNames.GridRegion, typeof(ConfigurationView));

            try
            {
                await HardwareService.InitializeAsync();

                if (AutoConnect && !string.IsNullOrWhiteSpace(ConnectionString))
                {
                    await HardwareService.CurrentDriver?.ConnectAsync(ConnectionString);
                }

                var mainView = HardwareService.CurrentDriver?.IsConnected ?? false
                    ? nameof(OutdoorDrivingView)
                    : nameof(ConfigurationView);

                Logger?.Log($"Navigating to \"{mainView}\".", Category.Debug, Priority.None);
                RegionManager.RequestNavigate(RegionNames.GridRegion, mainView);
            }
            catch (Exception ex)
            {
                Logger?.Log($"Hardware Initialization Failed - {ex}", Category.Exception, Priority.None);
                //TODO: Open the configuration page.
            }

        }
    }
}

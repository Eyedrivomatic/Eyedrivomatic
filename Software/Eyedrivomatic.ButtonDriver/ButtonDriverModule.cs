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

using Prism.Logging;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.ButtonDriver.Macros;
using Eyedrivomatic.ButtonDriver.Views;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;
using Microsoft.Practices.ServiceLocation;

namespace Eyedrivomatic.ButtonDriver
{
    [ModuleExport(typeof(ButtonDriverModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(ButtonDriverHardwareModule), nameof(ButtonDriverConfigurationModule), nameof(InfrastructureModule), nameof(MacrosModule) })]
    public class ButtonDriverModule : IModule, IDisposable
    {
        private readonly IHardwareService _hardwareService;
        private readonly IButtonDriverConfigurationService _configurationService;
        private readonly IRegionManager _regionManager;
        private readonly ILoggerFacade _logger;
        private readonly IServiceLocator _serviceLocator;

        [ImportingConstructor]
        public ButtonDriverModule(IRegionManager regionManager, IHardwareService hardwareService, IButtonDriverConfigurationService configurationService, ILoggerFacade logger, IServiceLocator serviceLocator)
        {
            _logger = logger;
            _logger?.Log($"Creating Module {nameof(ButtonDriverModule)}.", Category.Debug, Priority.None);

            _serviceLocator = serviceLocator;
            _regionManager = regionManager;
            _hardwareService = hardwareService;
            _configurationService = configurationService;
        }

        public async void Initialize()
        {
            _logger?.Log($"Initializing Module {nameof(ButtonDriverModule)}.", Category.Debug, Priority.None);

            _regionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(DeviceConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(DrivingView));

            foreach (var profile in _configurationService.DrivingProfiles)
            {
                _regionManager.RegisterViewWithRegion(RegionNames.MainNavigationRegion, () => CreateDriveProfileNavigation(profile));
            }

            try
            {
                await _hardwareService.InitializeAsync();
                _logger?.Log($"HardwareService Initialized. AutoConnect: [{_configurationService.AutoConnect}]", Category.Debug, Priority.None);

                var connection = _hardwareService.CurrentDriver?.Connection;
                if (_configurationService.AutoConnect)
                {
                    if (connection == null)
                    {
                        _logger?.Log("Failed to initialize hardware. No driver selected.", Category.Exception, Priority.None);
                        NavigateToConfiguration();
                        return;
                    }

                    NavigateToConfiguration();

                    var connectionString = _configurationService.ConnectionString;
                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        _logger.Log($"Connection string: [{connectionString}]", Category.Info, Priority.None);
                        await connection.ConnectAsync(connectionString);
                    }
                    else
                    {
                        _logger?.Log("Connection string not specified. Attempting to auto-detect.", Category.Warn, Priority.None);
                        await connection.AutoConnectAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.Log($"Hardware Initialization Failed - {ex}", Category.Exception, Priority.None);
            }
        }

        private RegionNavigationButton CreateDriveProfileNavigation(Profile profile)
        {
            var button = _serviceLocator.GetInstance<RegionNavigationButton>();
            button.Content = Resources.Strings.ResourceManager.GetString($"StandardProfileName_{profile.Name}") ?? profile.Name;
            button.RegionName = RegionNames.MainContentRegion;
            button.Target = new Uri($@"/{nameof(DrivingView)}?profile={profile.Name}", UriKind.Relative);
            return button;
        }

        private void NavigateToConfiguration()
        {
            _logger?.Log($@"Navigating to [/ConfigurationView/{nameof(DeviceConfigurationView)}].", Category.Debug, Priority.None);
            _regionManager.RequestNavigate(RegionNames.MainContentRegion, "/ConfigurationView");
            _regionManager.RequestNavigate(RegionNames.ConfigurationRegion, $"/{nameof(DeviceConfigurationView)}");
        }

        public void Dispose()
        {
            if (_configurationService.AutoSaveDeviceSettingsOnExit && _hardwareService.CurrentDriver != null) 
            {
                _logger?.Log("Auto saving device settings.", Category.Debug, Priority.None);

                _configurationService.SafetyBypass = _hardwareService.CurrentDriver.SafetyBypass == SafetyBypassState.Unsafe;
                _configurationService.Save();
            }
            _hardwareService?.Dispose();
        }
    }
}

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
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.ButtonDriver.Macros;
using Eyedrivomatic.ButtonDriver.Views;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using Prism.Interactivity.InteractionRequest;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

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

        [Import]
        public RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

        [Import(nameof(ShowDisclaimerCommand))]
        public ICommand ShowDisclaimerCommand { get; set; }


        [ImportingConstructor]
        public ButtonDriverModule(IRegionManager regionManager, IHardwareService hardwareService, IButtonDriverConfigurationService configurationService)
        {
            Log.Debug(this, $"Creating Module {nameof(ButtonDriverModule)}.");

            _regionManager = regionManager;
            _hardwareService = hardwareService;
            _configurationService = configurationService;
        }

        public async void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(ButtonDriverModule)}.");

            _regionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(DrivingView));

            RegisterConfigurationViews();
            RegisterDriveProfiles();

            ShowDisclaimerCommand.Execute(null);

            try
            {
                await _hardwareService.InitializeAsync();
                Log.Debug(this, $"HardwareService Initialized. AutoConnect: [{_configurationService.AutoConnect}]");

                if (_configurationService.AutoConnect)
                {
                    var connectionString = _configurationService.ConnectionString;
                    if (!string.IsNullOrWhiteSpace(connectionString))
                    {
                        Log.Info(this, $"Connection string: [{connectionString}]");
                        await _hardwareService.CurrentDriver.ConnectAsync(connectionString, CancellationToken.None);
                    }
                    else
                    {
                        Log.Warn(this, "Connection string not specified. Attempting to auto-detect.");
                        await _hardwareService.CurrentDriver.AutoConnectAsync(CancellationToken.None);
                        if (!string.IsNullOrEmpty(_hardwareService.CurrentDriver.Connection?.ConnectionString))
                            _configurationService.ConnectionString = _hardwareService.CurrentDriver.Connection.ConnectionString;
                    }


                    var connection = _hardwareService.CurrentDriver?.Connection;
                    if (connection == null)
                    {
                        Log.Error(this, "Failed to initialize hardware. No driver selected.");
                        NavigateToConfiguration();
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
               Log.Error(this, $"Hardware Initialization Failed - {ex}");
                NavigateToConfiguration();
                return;
            }

            NavigateToCurrentProfile();
        }

        private void NavigateToCurrentProfile()
        {
            if (_configurationService.CurrentProfile == null)
                _configurationService.CurrentProfile = _configurationService.DrivingProfiles.FirstOrDefault();

            if (_configurationService.CurrentProfile == null)
            {
                NavigateToConfiguration();
            }
            else
            {
                var uri = GetNavigationUri(_configurationService.CurrentProfile);
                Log.Debug(this, $@"Navigating to [{uri}].");
                _regionManager.RequestNavigate(RegionNames.MainContentRegion, uri);
            }
        }

        private void RegisterDriveProfiles()
        {
            foreach (var profile in _configurationService.DrivingProfiles)
            {
                _regionManager.RegisterViewWithRegion(RegionNames.DriveProfileSelectionRegion,
                    () => CreateDriveProfileNavigation(profile));
            }
        }

        private RegionNavigationButton CreateDriveProfileNavigation(Profile profile)
        {
            var button = RegionNavigationButtonFactory.Create(
                Translate.TranslationFor($"DriveProfile_{profile.Name.Replace(" ", "")}", profile.Name),
                RegionNames.MainContentRegion,
                GetNavigationUri(profile), 1);
            button.CanNavigate = () => _hardwareService?.CurrentDriver?.HardwareReady ?? false;

            return button;
        }

        private Uri GetNavigationUri(Profile profile)
        {
            return new Uri($@"/{nameof(DrivingView)}?profile={profile.Name}", UriKind.Relative);
        }

        private void RegisterConfigurationViews()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(DeviceConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
                RegionNavigationButtonFactory.Create(
                    Translate.TranslationFor(nameof(Strings.ViewName_DeviceConfig)),
                    RegionNames.ConfigurationContentRegion,
                    new Uri($@"/{nameof(DeviceConfigurationView)}", UriKind.Relative),
                    3));

            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(ProfileConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
                RegionNavigationButtonFactory.Create(
                Translate.TranslationFor(nameof(Strings.ViewName_ProfileConfig)),
                RegionNames.ConfigurationContentRegion,
                new Uri($@"/{nameof(ProfileConfigurationView)}", UriKind.Relative),
                4));
        }

        private void NavigateToConfiguration()
        {
            Log.Debug(this, $@"Navigating to [/{nameof(DeviceConfigurationView)}].");
            _regionManager.RequestNavigate(RegionNames.ConfigurationContentRegion, $"/{nameof(DeviceConfigurationView)}");
        }

        public void Dispose()
        {
            _hardwareService?.Dispose();
        }
    }
}

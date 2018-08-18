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
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros;
using Eyedrivomatic.ButtonDriver.Services;
using Eyedrivomatic.ButtonDriver.UI.Views;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Device.Configuration;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic.ButtonDriver.UI
{
    [ModuleExport(typeof(ButtonDriverUiModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(DeviceConfigurationModule), nameof(ButtonDriverModule), nameof(ButtonDriverConfigurationModule), nameof(CommonUiModule), nameof(MacrosModule) })]
    public class ButtonDriverUiModule : IModule, IDisposable
    {
        private readonly IButtonDriver _driver;

        private readonly IButtonDriverConfigurationService _configurationService;
        private readonly IRegionManager _regionManager;

        [Import]
        public RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

        [Import(nameof(ShowDisclaimerCommand))]
        public ICommand ShowDisclaimerCommand { get; set; }

        [ImportingConstructor]
        public ButtonDriverUiModule(IRegionManager regionManager, IButtonDriver driver, IButtonDriverConfigurationService configurationService)
        {
            Log.Debug(this, $"Creating Module {nameof(ButtonDriverUiModule)}.");

            _regionManager = regionManager;
            _driver = driver;
            _configurationService = configurationService;
        }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(ButtonDriverUiModule)}.");

            _regionManager.RegisterViewWithRegion(RegionNames.StatusRegion, typeof(StatusView));
            _regionManager.RegisterViewWithRegion(RegionNames.MainContentRegion, typeof(DrivingView));

            RegisterConfigurationViews();
            RegisterDriveProfiles();

            ShowDisclaimerCommand.Execute(null);

            if(_driver.ConnectionState == ConnectionState.Connected)
                NavigateToCurrentProfile();
        }

        private void NavigateToCurrentProfile()
        {
            if (_configurationService.CurrentProfile == null)
                _configurationService.CurrentProfile = _configurationService.DrivingProfiles.FirstOrDefault();

            if (_configurationService.CurrentProfile != null)
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
            button.CanNavigate = () => _driver.DeviceReady;

            return button;
        }

        private Uri GetNavigationUri(Profile profile)
        {
            return new Uri($@"/{nameof(DrivingView)}?profile={profile.Name}", UriKind.Relative);
        }

        private void RegisterConfigurationViews()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(ProfileConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
                RegionNavigationButtonFactory.Create(
                Translate.TranslationFor(nameof(Strings.ViewName_ProfileConfig)),
                RegionNames.ConfigurationContentRegion,
                new Uri($@"/{nameof(ProfileConfigurationView)}", UriKind.Relative),
                4));
        }

        public void Dispose()
        {
        }
    }
}

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

using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

using Eyedrivomatic.Configuration.Views;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Controls.DwellClick;
using Eyedrivomatic.Infrastructure;
using Microsoft.Practices.ServiceLocation;

namespace Eyedrivomatic.Configuration
{
    [ModuleExport(typeof(ConfigurationModule), 
        InitializationMode = InitializationMode.WhenAvailable, 
        DependsOnModuleNames = new[] { nameof(InfrastructureModule), nameof(ControlsModule) })]
    public class ConfigurationModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly IServiceLocator _serviceLocator;

        [ImportingConstructor]
        public ConfigurationModule(IRegionManager regionManager, IServiceLocator serviceLocator)
        {
            Log.Info(this, $"Creating Module {nameof(ConfigurationModule)}.");
            _serviceLocator = serviceLocator;
            _regionManager = regionManager;
        }

        [Import]
        public IDwellClickConfigurationService DwellClickConfigurationService { get; set; }

        public void Initialize()
        {
            Log.Info(this, $"Initializing Module {nameof(ConfigurationModule)}.");

            DwellClickBehavior.DefaultConfiguration = DwellClickConfigurationService;

            _regionManager.RegisterViewWithRegion(RegionNames.SleepButtonRegion, typeof(SleepButton));
            RegisterConfigurationViews();

            _regionManager.Regions[RegionNames.ConfigurationNavigationRegion].SortComparison = (viewA, viewB) =>
            {
                var buttonA = (RegionNavigationButton) viewA;
                var buttonB = (RegionNavigationButton) viewB;

                if (buttonA.SortOrder == buttonB.SortOrder)
                    return string.CompareOrdinal(buttonA.Name, buttonB.Name);

                return buttonA.SortOrder == buttonB.SortOrder ? 0 : buttonA.SortOrder > buttonB.SortOrder ? 1 : -1;
            };
        }

        private void RegisterConfigurationViews()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(ConfigurationView));

            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(GeneralConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
            {
                var button = _serviceLocator.GetInstance<RegionNavigationButton>();
                button.Content = Resources.Strings.ViewName_GeneralConfiguration;
                button.RegionName = RegionNames.ConfigurationContentRegion;
                button.Target = new Uri($@"/{nameof(GeneralConfigurationView)}", UriKind.Relative);
                button.SortOrder = 0;
                return button;
            });

            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(EyegazeConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
            {
                var button = _serviceLocator.GetInstance<RegionNavigationButton>();
                button.Content = Resources.Strings.ViewName_EyegazeConfig;
                button.RegionName = RegionNames.ConfigurationContentRegion;
                button.Target = new Uri($@"/{nameof(EyegazeConfigurationView)}", UriKind.Relative);
                button.SortOrder = 1;
                return button;
            });
        }

    }
}

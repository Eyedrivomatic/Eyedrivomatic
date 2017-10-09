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
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Prism.Commands;

namespace Eyedrivomatic.Configuration
{
    [ModuleExport(typeof(ConfigurationModule), 
        InitializationMode = InitializationMode.WhenAvailable, 
        DependsOnModuleNames = new[] { nameof(InfrastructureModule), nameof(ControlsModule) })]
    public class ConfigurationModule : IModule
    {
        private readonly IRegionManager _regionManager;

        [Import]
        public RegionNavigationButtonFactory RegionNavigationButtonFactory { get; set; }

        [ImportingConstructor]
        public ConfigurationModule(IRegionManager regionManager)
        {
            Log.Info(this, $"Creating Module {nameof(ConfigurationModule)}.");
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            Log.Info(this, $"Initializing Module {nameof(ConfigurationModule)}.");

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

        public const string SaveAllConfigurationCommandName = nameof(SaveAllConfigurationCommand);
        [Export(SaveAllConfigurationCommandName)]
        public CompositeCommand SaveAllConfigurationCommand { get; } = new CompositeCommandAny();

        private void RegisterConfigurationViews()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationRegion, typeof(ConfigurationView));

            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(GeneralConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion,
                () => RegionNavigationButtonFactory.Create(
                    Resources.Strings.ViewName_GeneralConfiguration,
                    RegionNames.ConfigurationContentRegion,
                    new Uri($@"/{nameof(GeneralConfigurationView)}", UriKind.Relative),
                    0));
        }
    }
}

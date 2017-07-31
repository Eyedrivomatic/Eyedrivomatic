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
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows;
using Eyedrivomatic.Configuration;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Eyegaze.Views;
using Eyedrivomatic.Infrastructure;
using Microsoft.Practices.ServiceLocation;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic.Eyegaze
{
    /// <summary>
    /// The purpose of this module is to register custom controls and their dependencies.
    /// </summary>
    [ModuleExport(typeof(EyegazeModule), 
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames =  new[] { nameof(InfrastructureModule), nameof(ControlsModule), nameof(ConfigurationModule) })]
    public class EyegazeModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly AggregateCatalog _catalog;
        public IServiceLocator ServiceLocator { get; set; }

        [Import]
        public IDwellClickConfigurationService DwellClickConfigurationService { get; set; }

        [Export(typeof(DwellClickAdornerFactory))]
        public static DwellClickAdorner CreateDwellClickAdorner(UIElement adornedElement)
        {
            return new DwellClickPieAdorner(adornedElement);
        }

        [ImportingConstructor]
        public EyegazeModule(IRegionManager regionManager, IServiceLocator serviceLocator, AggregateCatalog catalog)
        {
            Log.Debug(this, $"Creating Module {nameof(EyegazeModule)}.");

            _regionManager = regionManager;
            _catalog = catalog;
            ServiceLocator = serviceLocator;
        }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(EyegazeModule)}.");

            var thisDir = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).AbsolutePath);
            _catalog.Catalogs.Add(new DirectoryCatalog(thisDir ?? ".", "Eyedrivomatic.Eyegaze.Interfaces.*.dll"));

            DwellClickBehaviorFactory.Create = ServiceLocator.GetInstance<DwellClickBehavior>;
            DwellClickAdornerFactory.Create = adornedElement => new DwellClickPieAdorner(adornedElement);

            DwellClickBehavior.DefaultConfiguration = DwellClickConfigurationService;

            _regionManager.RegisterViewWithRegion(RegionNames.SleepButtonRegion, typeof(SleepButton));
            RegisterConfigurationViews();
        }

        private void RegisterConfigurationViews()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(EyegazeConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
            {
                var button = ServiceLocator.GetInstance<RegionNavigationButton>();
                button.Content = Resources.Strings.ViewName_EyegazeConfig;
                button.RegionName = RegionNames.ConfigurationContentRegion;
                button.Target = new Uri($@"/{nameof(EyegazeConfigurationView)}", UriKind.Relative);
                button.SortOrder = 1;
                return button;
            });
        }

    }
}

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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Accord.Video.DirectShow;
using Eyedrivomatic.Camera.Views;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Microsoft.Practices.ServiceLocation;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Prism.Regions;

namespace Eyedrivomatic.Camera
{
    [ModuleExport(typeof(CameraModule),
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(InfrastructureModule) })]
    public class CameraModule : IModule
    {
        private readonly IRegionManager _regionManager;
        private readonly IServiceLocator _serviceLocator;

        [ImportingConstructor]
        public CameraModule(IRegionManager regionManager, IServiceLocator serviceLocator)
        {
            Log.Info(this, $"Creating Module {nameof(CameraModule)}.");
            _regionManager = regionManager;
            _serviceLocator = serviceLocator;
        }

        [Export("GetCameras")]
        Func<IEnumerable<FilterInfo>> GetCameras => () => new FilterInfoCollection(FilterCategory.VideoInputDevice).Cast<FilterInfo>();

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(CameraModule)}.");

            _regionManager.RegisterViewWithRegion(RegionNames.ForwardViewCameraRegion, typeof(CameraView));

            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationContentRegion, typeof(CameraConfigurationView));
            _regionManager.RegisterViewWithRegion(RegionNames.ConfigurationNavigationRegion, () =>
            {
                var button = _serviceLocator.GetInstance<RegionNavigationButton>();
                button.Content = Translate.TranslationFor(nameof(Resources.Strings.ViewName_CameraConfiguration));
                button.RegionName = RegionNames.ConfigurationContentRegion;
                button.Target = new Uri($@"/{nameof(CameraConfigurationView)}", UriKind.Relative);
                button.SortOrder = 3;
                return button;
            });

        }
    }
}

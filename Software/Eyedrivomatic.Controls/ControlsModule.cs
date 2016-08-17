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
using System.Windows;

using Eyedrivomatic.Controls.DwellClick;
using Prism.Logging;
using Microsoft.Practices.ServiceLocation;
using System.Diagnostics.Contracts;

namespace Eyedrivomatic.Controls
{
    /// <summary>
    /// The purpose of this module is to register custom controls and their dependencies.
    /// </summary>
    [ModuleExport(typeof(ControlsModule), InitializationMode = InitializationMode.WhenAvailable)]
    public class ControlsModule : IModule
    {
        public ILoggerFacade Logger { get; set; }

        public IServiceLocator ServiceLocator { get; set; }

        [Export(typeof(DwellClickAdornerFactory))]
        public static DwellClickAdorner CreateDwellClickAdorner(UIElement adornedElement)
        {
            return new DwellClickPieAdorner(adornedElement);
        }

        [ImportingConstructor]
        public ControlsModule(ILoggerFacade logger, IServiceLocator serviceLocator)
        {
            Contract.Requires<ArgumentNullException>(serviceLocator != null, nameof(serviceLocator));

            Logger = logger;
            Logger?.Log($"Creating Module {nameof(ControlsModule)}.", Category.Debug, Priority.None);

            ServiceLocator = serviceLocator;
        }

        public void Initialize()
        {
            Logger?.Log($"Initializing Module {nameof(ControlsModule)}.", Category.Debug, Priority.None);

            DwellClickBehaviorFactory.Create = ServiceLocator.GetInstance<DwellClickBehavior>;
            DwellClickAdornerFactory.Create = adornedElement => new DwellClickPieAdorner(adornedElement);
        }
    }
}

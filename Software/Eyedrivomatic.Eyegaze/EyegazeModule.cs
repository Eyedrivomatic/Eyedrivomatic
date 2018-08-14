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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Logging;
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
        DependsOnModuleNames =  new[] { nameof(CommonUiModule) })]
    public class EyegazeModule : IModule, IDisposable
    {
        [ImportingConstructor]
        public EyegazeModule(ExportFactory<DwellClickBehavior> dwellClickBehaviorFactory)
        {
            Log.Debug(this, $"Creating Module {nameof(EyegazeModule)}.");

            DwellClickBehaviorFactory.Create = () => dwellClickBehaviorFactory.CreateExport().Value;
        }

        [Import]
        public IRegionManager RegionManager { get; set; }

        public void Initialize()
        {
            Log.Debug(this, $"Initializing Module {nameof(EyegazeModule)}.");

            var thisDir = Path.GetDirectoryName(new Uri(GetType().Assembly.CodeBase).LocalPath);
            var catalog = new AggregateCatalog(new AssemblyCatalog(Assembly.GetExecutingAssembly()),
                new DirectoryCatalog(thisDir ?? ".", "Eyedrivomatic.Eyegaze.Interfaces.*.dll"));
            var container = new CompositionContainer(catalog);
            EyegazeProviders = container.GetExports<IEyegazeProvider, IEyegazeProviderMetadata>().ToList();
        }

        [Export(typeof(IEnumerable<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>>))]
        public List<Lazy<IEyegazeProvider, IEyegazeProviderMetadata>> EyegazeProviders { get; private set; }


        [Export(typeof(Func<UIElement, DwellClickAdorner>))]
        public DwellClickAdorner CreateDwellClickAdorner(UIElement adornedElement)
        {
            return new DwellClickPieAdorner(adornedElement);
        }

        public void Dispose()
        {
            try
            {
                foreach (var providerExport in EyegazeProviders.Where(e => e.IsValueCreated))
                {
                    providerExport.Value.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error(this, $"Error while disposing Eyegaze provider - [{e}]");
            }
        }
    }
}

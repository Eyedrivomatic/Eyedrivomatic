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


using System.Collections.Generic;
using System.ComponentModel.Composition;
using Eyedrivomatic.Common.UI;
using Prism.Mef.Modularity;
using Prism.Modularity;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Resources
{
    [ModuleExport(typeof(ResourcesModule),
        InitializationMode = InitializationMode.WhenAvailable,
        DependsOnModuleNames = new[] { nameof(CommonUiModule) })]
    public class ResourcesModule : IModule
    {
        [ImportingConstructor]
        public ResourcesModule()
        {
            Log.Info(this, $"Creating Module {nameof(ResourcesModule)}.");
        }

        public IEnumerable<ThemeResourceDictionary> Themes { get; private set; }

        public void Initialize()
        {
            Log.Info(this, $"Initializing Module {nameof(ResourcesModule)}.");
            Translate.ResourceManager = Strings.ResourceManager;
        }
    }
}

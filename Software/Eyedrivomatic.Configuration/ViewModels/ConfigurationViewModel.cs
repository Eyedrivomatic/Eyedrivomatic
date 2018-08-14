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
using Eyedrivomatic.Resources;
using Prism.Commands;

namespace Eyedrivomatic.Configuration.ViewModels
{
    [Export]
    public class ConfigurationViewModel : IHeaderInfoProvider<string>
    {
        public string HeaderInfo => Strings.ViewName_Configuration;

        public List<ThemeResourceDictionary> AvailableColors;

        [Import(ConfigurationModule.SaveAllConfigurationCommandName)]
        public CompositeCommand SaveCommand { get; set; }
    }
}

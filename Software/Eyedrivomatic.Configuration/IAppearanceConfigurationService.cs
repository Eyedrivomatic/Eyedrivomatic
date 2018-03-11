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
using System.ComponentModel;
using System.Globalization;
using Eyedrivomatic.Infrastructure;

namespace Eyedrivomatic.Configuration
{
    public interface IAppearanceConfigurationService : INotifyPropertyChanged
    {
        bool HideMouseCursor { get; set; }

        ThemeColorsResourceDictionary ThemeColors { get; set; }
        ThemeImagesResourceDictionary ThemeImages { get; set; }
        ThemeStylesResourceDictionary ThemeStyles { get; set; }

        IList<ThemeColorsResourceDictionary> AvailableThemeColors { get; }
        IList<ThemeImagesResourceDictionary> AvailableThemeImages { get; }
        IList<ThemeStylesResourceDictionary> AvailableThemeStyles { get; }

        CultureInfo CurrentCulture { get; set; }
        IList<CultureInfo> AvailableCultures { get; }

        void Save();
        bool HasChanges { get; }
    }
}

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


using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using NullGuard;

namespace Eyedrivomatic.Common.UI
{
    [Export]
    public class ThemeSelector : DependencyObject
    {
        private readonly Application _application;

        public ThemeSelector()
        {
            _application = Application.Current;
        }

        public ThemeSelector(Application application)
        {
            _application = application;
        }

        public void ApplyTheme([AllowNull] ThemeColorsResourceDictionary theme)
        {
            var dictionaries = _application.Resources.MergedDictionaries;

            var prevThemes = dictionaries.OfType<ThemeColorsResourceDictionary>().ToList();
            if (theme != null) dictionaries.Insert(0, theme);
            foreach (var prevTheme in prevThemes) dictionaries.Remove(prevTheme);
        }

        public void ApplyTheme([AllowNull] ThemeImagesResourceDictionary theme)
        {
            var dictionaries = _application.Resources.MergedDictionaries;

            var prevThemes = dictionaries.OfType<ThemeImagesResourceDictionary>().ToList();
            if (theme != null) dictionaries.Insert(0, theme);
            foreach (var prevTheme in prevThemes) dictionaries.Remove(prevTheme);
        }

        public void ApplyTheme([AllowNull] ThemeStylesResourceDictionary theme)
        {
            var dictionaries = _application.Resources.MergedDictionaries;

            var prevThemes = dictionaries.OfType<ThemeStylesResourceDictionary>().ToList();
            if (theme != null) dictionaries.Insert(0, theme);
            foreach (var prevTheme in prevThemes) dictionaries.Remove(prevTheme);
        }

    }
}

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


using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using Prism.Mvvm;

namespace Eyedrivomatic.Configuration
{
    public static class ThemeConfiguraionProvider
    {
        [Export]
        internal static ThemeConfiguration DefaultConfiguration => ThemeConfiguration.Default;
    }

    [Export(typeof(IThemeConfigurationService)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class ThemeConfigurationService : BindableBase, IThemeConfigurationService
    {
        private readonly ThemeConfiguration _configuration;
        private readonly ThemeSelector _themeSelector;
        private bool _hasChanges;

        [ImportingConstructor]
        internal ThemeConfigurationService(ThemeConfiguration configuration, IEnumerable<ThemeResourceDictionary> themes, ThemeSelector themeSelector)
        {
            _configuration = configuration;
            _themeSelector = themeSelector;
            _configuration.PropertyChanged += Configuration_PropertyChanged;
            Themes = themes.ToList();

            if (_configuration.SettingsVersion < 1)
            {
                _configuration.Upgrade();
                _configuration.SettingsVersion = 1;
            }
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Theme))
            {
                _hasChanges = true;
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(e.PropertyName);

                var theme = Themes.FirstOrDefault(t => t.ThemeName == Theme) ?? Themes.FirstOrDefault();
                _themeSelector.ApplyTheme(theme);
            }
        }

        public string Theme
        {
            get => _configuration.Theme;
            set => _configuration.Theme = value;
        }
        
        public bool HasChanges => _hasChanges;
        public IList<ThemeResourceDictionary> Themes { get; }

        public void Save()
        {
            if (!_hasChanges) return;

            Log.Info(this, "Saving Changes");

            _configuration.Save();
            _hasChanges = false;
        }
    }
}

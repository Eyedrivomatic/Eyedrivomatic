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
    public static class AppearanceConfigurationProvider
    {
        [Export]
        internal static AppearanceConfiguration DefaultConfiguration => AppearanceConfiguration.Default;
    }

    [Export(typeof(IAppearanceConfigurationService)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class AppearanceConfigurationService : BindableBase, IAppearanceConfigurationService
    {
        private readonly AppearanceConfiguration _configuration;
        private readonly ThemesProvider _themesProvider;
        private readonly ThemeSelector _themeSelector;
        private bool _hasChanges;

        [ImportingConstructor]
        internal AppearanceConfigurationService(AppearanceConfiguration configuration, ThemesProvider themesProvider, ThemeSelector themeSelector)
        {
            _configuration = configuration;
            _themesProvider = themesProvider;
            _themeSelector = themeSelector;
            _configuration.PropertyChanged += Configuration_PropertyChanged;

            if (_configuration.SettingsVersion < 1)
            {
                _configuration.Upgrade();
                _configuration.SettingsVersion = 1;
            }

            ApplyThemeColors();
            ApplyThemeImages();
            ApplyThemeStyles();
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _hasChanges = true;

            if (e.PropertyName == nameof(_configuration.HideMouseCursor))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(HideMouseCursor));
            }

            if (e.PropertyName == nameof(_configuration.CameraOverlayTransparency))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(CameraOverlayTransparency));
            }

            if (e.PropertyName == nameof(_configuration.ThemeColors))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(ThemeColors));

                ApplyThemeColors();
            }

            if (e.PropertyName == nameof(_configuration.ThemeImages))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(ThemeImages));
                ApplyThemeImages();
            }

            if (e.PropertyName == nameof(_configuration.ThemeStyles))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(ThemeStyles));
                ApplyThemeStyles();
            }
        }

        public bool HideMouseCursor
        {
            get => _configuration.HideMouseCursor;
            set => _configuration.HideMouseCursor = value;
        }

        public ThemeColorsResourceDictionary ThemeColors
        {
            get => AvailableThemeColors.SingleOrDefault(t => string.Equals(t.ThemeName, _configuration.ThemeColors)) ?? AvailableThemeColors.First();
            set => _configuration.ThemeColors = value.ThemeName;
        }

        public ThemeImagesResourceDictionary ThemeImages
        {
            get => AvailableThemeImages.SingleOrDefault(t => string.Equals(t.ThemeName, _configuration.ThemeImages)) ?? AvailableThemeImages.First();
            set => _configuration.ThemeImages = value.ThemeName;
        }

        public ThemeStylesResourceDictionary ThemeStyles
        {
            get => AvailableThemeStyles.SingleOrDefault(t => string.Equals(t.ThemeName, _configuration.ThemeStyles)) ?? AvailableThemeStyles.First();
            set => _configuration.ThemeStyles = value.ThemeName;
        }

        public IList<ThemeColorsResourceDictionary> AvailableThemeColors => _themesProvider.Colors;
        public IList<ThemeImagesResourceDictionary> AvailableThemeImages => _themesProvider.Images;
        public IList<ThemeStylesResourceDictionary> AvailableThemeStyles => _themesProvider.Styles;

        public int CameraOverlayTransparency
        {
            get => _configuration.CameraOverlayTransparency;
            set => _configuration.CameraOverlayTransparency = value;
        }

        public bool HasChanges => _hasChanges;

        public void Save()
        {
            if (!_hasChanges) return;

            Log.Info(this, "Saving Changes");

            _configuration.Save();
            _hasChanges = false;
        }


        private void ApplyThemeColors()
        {
            if (ThemeColors == null) return;
            _themeSelector.ApplyTheme(ThemeColors);
        }

        private void ApplyThemeImages()
        {
            if (ThemeImages == null) return;
            _themeSelector.ApplyTheme(ThemeImages);
        }

        private void ApplyThemeStyles()
        {
            if (ThemeStyles == null) return;
            _themeSelector.ApplyTheme(ThemeStyles);
        }
    }
}

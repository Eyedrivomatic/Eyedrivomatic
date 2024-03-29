﻿//	Copyright (c) 2018 Eyedrivomatic Authors
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
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using Gu.Localization;
using Prism.Mvvm;

namespace Eyedrivomatic.Configuration
{
    public static class AppearanceConfigurationProvider
    {
        [Export]
        internal static AppearanceConfiguration DefaultConfiguration => AppearanceConfiguration.Default;
    }

    [Export(typeof(IAppearanceConfigurationService)), PartCreationPolicy(CreationPolicy.Shared)]
    public class AppearanceConfigurationService : BindableBase, IAppearanceConfigurationService
    {
        private readonly AppearanceConfiguration _configuration;
        private readonly ThemesProvider _themesProvider;
        private readonly ThemeSelector _themeSelector;

        [ImportingConstructor]
        internal AppearanceConfigurationService(AppearanceConfiguration configuration, ThemesProvider themesProvider, ThemeSelector themeSelector)
        {
            _configuration = configuration;
            _themesProvider = themesProvider;
            _themeSelector = themeSelector;
            _configuration.PropertyChanged += Configuration_PropertyChanged;
            _configuration.SettingsLoaded += (sender, args) => HasChanges = false;
            _configuration.Upgrade();
            _configuration.WriteToLog();

            InitializeCulture(configuration);
            Translator.CurrentCultureChanged += TranslatorOnCurrentCultureChanged;

            HasChanges = false;

            ApplyThemeColors();
            ApplyThemeImages();
            ApplyThemeStyles();
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

        public CultureInfo CurrentCulture
        {
            get => Translator.CurrentCulture;
            set => Translator.Culture = value;
        }

        public IList<CultureInfo> AvailableCultures => Translator.Cultures.ToList();

        private bool _hasChanges;
        public bool HasChanges
        {
            get => _hasChanges;
            private set => SetProperty(ref _hasChanges, value);
        }

        public void Save()
        {
            if (!HasChanges) return;

            Log.Info(this, "Saving Changes");

            _configuration.Save();
            HasChanges = false;
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

        private void InitializeCulture(AppearanceConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.CurrentCulture)) return;

            try
            {
                var culture = CultureInfo.GetCultureInfoByIetfLanguageTag(configuration.CurrentCulture);
                if (Translator.ContainsCulture(culture))
                {
                    Translator.Culture = culture;
                }
                else
                {
                    Log.Warn(this, $"Culture [{configuration.CurrentCulture}] not available.");
                    configuration.CurrentCulture = CurrentCulture.IetfLanguageTag;
                }
            }
            catch (CultureNotFoundException)
            {
                Log.Warn(this, $"Invalid culture [{configuration.CurrentCulture}].");
                configuration.CurrentCulture = CurrentCulture.IetfLanguageTag;
            }
        }

        private void TranslatorOnCurrentCultureChanged(object o, CultureChangedEventArgs cultureChangedEventArgs)
        {
            _configuration.CurrentCulture = CurrentCulture.IetfLanguageTag;

            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(CurrentCulture));
        }

        private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HasChanges = true;

            if (e.PropertyName == nameof(_configuration.HideMouseCursor))
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(HideMouseCursor));
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
    }
}

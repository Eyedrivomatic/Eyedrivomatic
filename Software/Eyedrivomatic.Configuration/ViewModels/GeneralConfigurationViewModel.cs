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
using System.Globalization;
using System.Windows.Input;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using Prism.Commands;
using Prism.Mvvm;

namespace Eyedrivomatic.Configuration.ViewModels
{
    [Export]
    public class GeneralConfigurationViewModel : BindableBase, IHeaderInfoProvider<string>, IDisposable
    {
        private readonly IAppearanceConfigurationService _appearanceConfigurationService;
        private readonly IDisposable _saveCommandRegistration;
        public string HeaderInfo => Strings.ViewName_GeneralConfiguration;


        [ImportingConstructor]
        public GeneralConfigurationViewModel(
            IAppearanceConfigurationService appearanceConfigurationService, 
            [Import(ConfigurationModule.SaveAllConfigurationCommandName)] CompositeCommand saveAllCommand)
        {
            _appearanceConfigurationService = appearanceConfigurationService;
            _appearanceConfigurationService.PropertyChanged += AppearanceConfigurationPropertyChanged;

            _saveCommandRegistration = saveAllCommand.DisposableRegisterCommand(SaveCommand);
        }


        private static readonly Dictionary<string, string[]> ConfigurationPropertyDependencies = new Dictionary<string, string[]>
        {
            { nameof(IAppearanceConfigurationService.HasChanges), new [] {nameof(HasChanges)} },
            { nameof(IAppearanceConfigurationService.HideMouseCursor), new [] {nameof(HideMouseCursor) } },
            { nameof(IAppearanceConfigurationService.CurrentCulture), new [] {nameof(CurrentCulture) } },
            { nameof(IAppearanceConfigurationService.ThemeColors), new [] {nameof(ThemeColors) } },
            { nameof(IAppearanceConfigurationService.ThemeImages), new [] {nameof(ThemeImages) } },
            { nameof(IAppearanceConfigurationService.ThemeStyles), new [] {nameof(ThemeStyles) } },
        };

        private void AppearanceConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!ConfigurationPropertyDependencies.ContainsKey(e.PropertyName)) return;
            foreach (var dep in ConfigurationPropertyDependencies[e.PropertyName])
            {
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(dep);
            }
        }

        public bool HideMouseCursor
        {
            get => _appearanceConfigurationService.HideMouseCursor;
            set => _appearanceConfigurationService.HideMouseCursor = value;
        }

        public ThemeColorsResourceDictionary ThemeColors
        {
            get => _appearanceConfigurationService.ThemeColors;
            set => _appearanceConfigurationService.ThemeColors = value;
        }

        public ThemeImagesResourceDictionary ThemeImages
        {
            get => _appearanceConfigurationService.ThemeImages;
            set => _appearanceConfigurationService.ThemeImages = value;
        }

        public ThemeStylesResourceDictionary ThemeStyles
        {
            get => _appearanceConfigurationService.ThemeStyles;
            set => _appearanceConfigurationService.ThemeStyles = value;
        }

        public IList<ThemeColorsResourceDictionary> AvailableThemeColors => _appearanceConfigurationService.AvailableThemeColors;
        public IList<ThemeImagesResourceDictionary> AvailableThemeImages => _appearanceConfigurationService.AvailableThemeImages;
        public IList<ThemeStylesResourceDictionary> AvailableThemeStyles => _appearanceConfigurationService.AvailableThemeStyles;

        public CultureInfo CurrentCulture
        {
            get => _appearanceConfigurationService.CurrentCulture;
            set => _appearanceConfigurationService.CurrentCulture = value;
        }

        public IList<CultureInfo> AvailableCultures => _appearanceConfigurationService.AvailableCultures;

        public bool HasChanges => _appearanceConfigurationService.HasChanges;

        public ICommand SaveCommand => new DelegateCommand(() => _appearanceConfigurationService.Save())
            .ObservesCanExecute(() => HasChanges);

        public void Dispose()
        {
            _saveCommandRegistration?.Dispose();
        }
    }
}

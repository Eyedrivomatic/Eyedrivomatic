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

        private void AppearanceConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
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

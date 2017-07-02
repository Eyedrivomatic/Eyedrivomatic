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
using System.ComponentModel.Composition;
using System.Windows.Input;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using Prism.Commands;
using Prism.Mvvm;

namespace Eyedrivomatic.Configuration.ViewModels
{
    [Export]
    public class GeneralConfigurationViewModel : BindableBase, IHeaderInfoProvider<string>
    {
        private readonly IThemeConfigurationService _themeConfigurationService;

        public string HeaderInfo => Strings.ViewName_GeneralConfiguration;

        [ImportingConstructor]
        public GeneralConfigurationViewModel(IThemeConfigurationService themeConfigurationService)
        {
            _themeConfigurationService = themeConfigurationService;
            _themeConfigurationService.PropertyChanged += ThemeConfigurationPropertyChanged;
        }

        private void ThemeConfigurationPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty);
        }

        public string Theme
        {
            get => _themeConfigurationService.Theme;
            set => _themeConfigurationService.Theme = value;
        }

        public IEnumerable<ThemeResourceDictionary> Themes => _themeConfigurationService.Themes;

        public ICommand SaveCommand => new DelegateCommand(() => _themeConfigurationService.Save(), () => _themeConfigurationService.HasChanges);
    }
}

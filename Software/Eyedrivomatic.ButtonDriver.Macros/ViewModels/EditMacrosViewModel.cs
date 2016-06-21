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
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

using Prism.Mvvm;
using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Resources;
using System.Diagnostics.Contracts;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    [Export]
    public class EditMacrosViewModel : BindableBase
    {
        private IMacroService _macroService;

        public string DisplayName => Strings.ViewName_Macros;

        public ObservableCollection<IMacro> Macros => _macroService.Macros;
        public bool HasChanges => _macroService.HasChanges;

        public ICommand AddMacroCommand => new DelegateCommand(AddMacro, () => true);
        public ICommand DeleteMacroCommand => new DelegateCommand<IMacro>(DeleteMacro, macro => Macros.Contains(macro));

        public ICommand ResetMacrosCommand => new DelegateCommand(ResetMacros, () => _macroService.HasChanges);
        public ICommand SaveMacrosCommand => new DelegateCommand(SaveMacros, () => _macroService.HasChanges);

        [ImportingConstructor]
        public EditMacrosViewModel(IMacroService macroService)
        {
            Contract.Requires<ArgumentNullException>(macroService != null, nameof(macroService));

            _macroService = macroService;
            _macroService.PropertyChanged += MacroService_PropertyChanged; ;

            ResetMacros();
        }

        private void MacroService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_macroService.HasChanges))
                OnPropertyChanged(nameof(HasChanges));
        }

        private void AddMacro()
        {
            var macro = new UserMacro();
            Macros.Add(macro);
        }

        private void DeleteMacro(IMacro macro)
        {
            Macros.Remove(macro);
        }

        private void SaveMacros()
        {
            _macroService.Save();
        }

        private void ResetMacros()
        {
            _macroService.Reset();
        }
    }
}

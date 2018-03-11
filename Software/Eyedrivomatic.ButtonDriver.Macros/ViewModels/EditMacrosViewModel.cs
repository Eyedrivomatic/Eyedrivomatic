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


using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

using Prism.Mvvm;
using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    [Export]
    public class EditMacrosViewModel : BindableBase
    {
        private readonly IMacroService _macroService;

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
            _macroService = macroService;
            _macroService.PropertyChanged += MacroService_PropertyChanged; ;

            ResetMacros();
        }

        private void MacroService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_macroService.HasChanges))
                RaisePropertyChanged(nameof(HasChanges));
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

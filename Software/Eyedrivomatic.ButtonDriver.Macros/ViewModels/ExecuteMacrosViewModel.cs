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
using Prism.Regions;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    [Export]
    public class ExecuteMacrosViewModel : BindableBase, INavigationAware, IHeaderInfoProvider<string>
    {
        private readonly IMacroSerializationService _macroSerializationService;

        [ImportingConstructor]
        public ExecuteMacrosViewModel(
            [Import("ExecuteMacroCommand")]ICommand executeMacroCommand, 
            IMacroSerializationService macroSerializationService
            )
        {
            ExecuteMacroCommand = executeMacroCommand;
            _macroSerializationService = macroSerializationService;
            Macros = new ObservableCollection<IMacro>(_macroSerializationService.LoadMacros());
        }

        public ObservableCollection<IMacro> Macros { get; }

        public ICommand ExecuteMacroCommand { get; }

        public string HeaderInfo { get; } = Strings.ViewName_Macros;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Macros.Clear();
            Macros.AddRange(_macroSerializationService.LoadMacros());
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

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
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System.Collections.ObjectModel;
using System.Windows.Input;

using Prism.Mvvm;
using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Macros.Models;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    public class EditMacrosViewModel : BindableBase
    {
        public string DisplayName => Eyedrivomatic.Resources.Strings.ViewName_Macros;

        public ObservableCollection<IMacro> Macros { get; } = new ObservableCollection<IMacro>();

        public ICommand AddMacroCommand => new DelegateCommand(AddMacro, () => true);
        public ICommand DeleteMacroCommand => new DelegateCommand<IMacro>(macro => Macros.Remove(macro), macro => Macros.Contains(macro));

        public ICommand SaveMacrosCommand => new DelegateCommand(SaveMacros, () => true);

        private void AddMacro()
        {
            var macro = new UserMacro();
            Macros.Add(macro);
        }

        private void SaveMacros()
        {
        }

        private void ResetMacros()
        {
        }
    }
}

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

using System;
using Prism.Mvvm;

using System.Diagnostics.Contracts;

using Eyedrivomatic.ButtonDriver.Macros.Models;


namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    public abstract class EditMacroTaskViewModel : BindableBase
    {
        protected EditMacroTaskViewModel(MacroTask task)
        {
            Contract.Requires<ArgumentNullException>(task != null, nameof(task));

            Task = task;
        }

        public MacroTask Task { get; }

        public string DisplayName
        {
            get { return Task.DisplayName; }
            set
            {
                if (Task.DisplayName == value) return;
                Task.DisplayName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Info));
            }
        }

        public abstract string Description { get; }

        public string Info => Task.ToString();
    }
}

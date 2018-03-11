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


using Prism.Mvvm;

using Eyedrivomatic.ButtonDriver.Macros.Models;


namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    public abstract class EditMacroTaskViewModel : BindableBase
    {
        protected EditMacroTaskViewModel(MacroTask task)
        {
            Task = task;
        }

        public MacroTask Task { get; }

        public string DisplayName
        {
            get => Task.DisplayName;
            set
            {
                if (Task.DisplayName == value) return;
                Task.DisplayName = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Info));
            }
        }

        public abstract string Description { get; }

        public string Info => Task.ToString();
    }
}

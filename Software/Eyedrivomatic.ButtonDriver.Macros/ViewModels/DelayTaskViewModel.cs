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


using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Resources;
using System.ComponentModel.Composition;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    [Export]
    public class DelayTaskViewModel : EditMacroTaskViewModel
    {
        private DelayTask DelayTask => (DelayTask)Task;

        public DelayTaskViewModel(DelayTask task)
            : base(task)
        { }

        public override string Description => string.Format(Strings.DelayTask_DefaultNameFormat, DelayMs/1000d);

        public double DelayMs
        {
            get => DelayTask.DelayMs;
            set
            {
                if (DelayTask.DelayMs == value) return;
                DelayTask.DelayMs = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Info));
                RaisePropertyChanged(nameof(Description));
            }
        }

    }
}

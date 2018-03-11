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
using System.ComponentModel.Composition;
using System.Threading.Tasks;

using Prism.Mvvm;

using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Macros.ViewModels
{
    [Export]
    public class CreateNewTaskViewModel : BindableBase
    {
        private readonly TaskCompletionSource<MacroTask> _createTask = new TaskCompletionSource<MacroTask>();

        public string DisplayName => Strings.NewTaskViewModelName;

        //public IEnumerable<ICommand> CreateTaskMethods => new
        //{
        //    new DelegateCommand(()=>CycleRelayTask.CreateNew, () => _driver != null );
        //}

        public Task<MacroTask> CreateTask(IMacro macro)
        {
            throw new NotImplementedException();
        }
    }
}

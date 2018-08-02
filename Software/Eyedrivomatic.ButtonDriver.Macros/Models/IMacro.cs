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
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eyedrivomatic.ButtonDriver.Device.Services;

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    [XmlInclude(typeof(UserMacro))]
    public interface IMacro : IDataErrorInfo
    {
        /// <summary>
        /// An icon to display on the button. May be null.
        /// </summary>
        string IconPath { get; set; }
        
        /// <summary>
        /// The name of the macro as it should be seen by the user. May be null or empty.
        /// </summary>
        string DisplayName { get; set; }

        /// <summary>
        /// True if the macro is currently executing.
        /// </summary>
        bool IsExecuting { get; }

        /// <summary>
        /// The tasks performed by the macro.
        /// </summary>
        ObservableCollection<MacroTask> Tasks { get; }

        /// <summary>
        /// Execute the macro node task
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync(IButtonDriver driver);

        /// <summary>
        /// Returns true if the task can be executed.
        /// </summary>
        /// <param name="driver">The IButtonDriver currently in use.</param>
        /// <returns>True if this macro can be executed.</returns>
        bool CanExecute(IButtonDriver driver);
    }
}

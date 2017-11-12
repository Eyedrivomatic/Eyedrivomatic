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


using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eyedrivomatic.ButtonDriver.Hardware.Services;

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

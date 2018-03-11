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

namespace Eyedrivomatic.ButtonDriver.Macros.Models
{
    public interface IMacroService : INotifyPropertyChanged
    {
        /// <summary>
        /// The list of currently loaded macros.
        /// </summary>
        ObservableCollection<IMacro> Macros { get; }

        /// <summary>
        /// Returns true if the properties of any Macros or their tasks have 
        /// changed or if any new macros have been added.
        /// </summary>
        bool HasChanges { get; set; }

        void Save();
        void Reset();
    }
}
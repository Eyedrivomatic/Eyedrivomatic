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
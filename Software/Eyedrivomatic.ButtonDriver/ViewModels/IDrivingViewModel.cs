using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros.Models;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public interface IDrivingViewModel : INotifyPropertyChanged
    {
        string HeaderInfo { get; }

        ProfileSpeed CurrentSpeed { get; }
        IEnumerable<ProfileSpeed> Speeds { get; }

        ObservableCollection<IMacro> Macros { get; }
        bool DiagonalSpeedReduction { get; set; }
        bool SafetyBypass { get; set; }
        bool IsOnline { get; }

        double CameraOverlayOpacity { get; }
        Brush CameraOverlayButtonBackgroundBrush { get; }

        ICommand ContinueCommand { get; }
        ICommand MoveCommand { get; }
        ICommand NudgeCommand { get; }
        ICommand StopCommand { get; }
        ICommand ExecuteMacroCommand { get; }
        ICommand DiagonalSpeedReductionToggleCommand { get; }
        ICommand SetSpeedCommand { get; }
    }

}
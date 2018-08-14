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


using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros.Models;

namespace Eyedrivomatic.ButtonDriver.UI.ViewModels
{
    public interface IDrivingViewModel : INotifyPropertyChanged
    {
        string HeaderInfo { get; }

        ProfileSpeed CurrentSpeed { get; }
        IEnumerable<ProfileSpeed> Speeds { get; }

        ObservableCollection<IMacro> Macros { get; }
        bool SafetyBypass { get; set; }
        bool IsOnline { get; }

        bool ShowForwardView { get; }
        double CameraOverlayOpacity { get; }

        ICommand ContinueCommand { get; }
        ICommand MoveCommand { get; }
        ICommand NudgeCommand { get; }
        ICommand StopCommand { get; }
        ICommand ExecuteMacroCommand { get; }
        ICommand SetSpeedCommand { get; }
    }

}
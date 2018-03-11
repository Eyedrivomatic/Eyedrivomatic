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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Annotations;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Prism.Commands;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    internal class DesignDrivingViewModel : IDrivingViewModel
    {
        private readonly ICommand _designCommand;

        public DesignDrivingViewModel()
        {
            _designCommand = new DelegateCommand(() => { }, () => IsOnline);
            CurrentSpeed = Speeds.FirstOrDefault();
        }

        public string HeaderInfo => "Drive";

        public bool IsOnline => true;
        public bool ShowForwardView => true;

        public ProfileSpeed CurrentSpeed { get; }
        public bool SafetyBypass { get; set; }

        public double CameraOverlayOpacity => 0.6;

        public ObservableCollection<IMacro> Macros { get; } = new ObservableCollection<IMacro>
        {
            new UserMacro {DisplayName = "Power", IconPath = "Images/Off.png"},
            new UserMacro {DisplayName = "Tilt", IconPath = "Images/Tilt.png"}
        };

        public double Duration { get; set; }

        public ICommand ExecuteMacroCommand => _designCommand;
        public ICommand ContinueCommand => _designCommand;
        public ICommand MoveCommand => _designCommand;
        public ICommand NudgeCommand => _designCommand;
        public ICommand StopCommand => _designCommand;
        public ICommand SetSpeedCommand => _designCommand;

        public IEnumerable<ProfileSpeed> Speeds { get; } = new List<ProfileSpeed>
        {
            new ProfileSpeed{Name = "Slow"},
            new ProfileSpeed{Name = "Walk"},
            new ProfileSpeed{Name = "Fast"},
        };

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
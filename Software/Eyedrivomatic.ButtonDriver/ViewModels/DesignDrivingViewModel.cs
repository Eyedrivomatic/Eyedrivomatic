using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Prism.Commands;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    class DesignDrivingViewModel : IDrivingViewModel
    {
        private ICommand _designCommand;

        public DesignDrivingViewModel()
        {
            _designCommand = new DelegateCommand(() => { }, () => IsOnline);
            CurrentSpeed = Speeds.FirstOrDefault();
        }

        public string HeaderInfo => "Drive";

        public bool IsOnline => true;

        public ProfileSpeed CurrentSpeed { get; }
        public bool DiagonalSpeedReduction { get; set; }
        public bool SafetyBypass { get; set; }

        public ObservableCollection<IMacro> Macros { get; } = new ObservableCollection<IMacro>
        {
            new UserMacro {DisplayName = "Power", IconPath = "Images/Off.png"},
            new UserMacro {DisplayName = "Tilt", IconPath = "Images/Tilt.png"}
        };

        public double XDuration { get; set; }= 1000;
        public double YDuration { get; set; }= 2000;

        public ICommand ExecuteMacroCommand => _designCommand;
        public ICommand ContinueCommand => _designCommand;
        public ICommand MoveCommand => _designCommand;
        public ICommand NudgeCommand => _designCommand;
        public ICommand StopCommand => _designCommand;
        public ICommand DiagonalSpeedReductionToggleCommand => _designCommand;
        public ICommand SetSpeedCommand => _designCommand;

        public IEnumerable<ProfileSpeed> Speeds { get; } = new List<ProfileSpeed>
        {
            new ProfileSpeed{Name = "Slow"},
            new ProfileSpeed{Name = "Walk"},
            new ProfileSpeed{Name = "Fast"},
        };
    }
}
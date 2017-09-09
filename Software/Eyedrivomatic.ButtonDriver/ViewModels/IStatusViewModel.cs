using System.Windows;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Services;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public interface IStatusViewModel
    {
        ConnectionState ConnectionState { get; }
        Point JoystickPosition { get; }
        Direction CurrentDirection { get; }
        string Profile { get; }
        ReadyState ReadyState { get; }
        bool SafetyBypassStatus { get; }
        string Speed { get; }

        bool Switch1 { get; }
        bool Switch2 { get; }
        bool Switch3 { get; }
    }

    class DesignStatusViewModel : IStatusViewModel
    {
        public ConnectionState ConnectionState => ConnectionState.Connecting;
        public Point JoystickPosition => new Point(7, 12);
        public Direction CurrentDirection => Direction.BackwardRight;
        public string Profile => "Indoors";
        public ReadyState ReadyState => ReadyState.Continue;
        public bool SafetyBypassStatus => false;
        public string Speed => "Walk";
        public bool Switch1 => false;
        public bool Switch2 => true;
        public bool Switch3 => false;
    }
}
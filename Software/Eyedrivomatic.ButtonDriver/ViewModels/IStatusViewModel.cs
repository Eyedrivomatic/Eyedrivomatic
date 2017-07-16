using System.Windows;
using Eyedrivomatic.ButtonDriver.Hardware.Communications;
using Eyedrivomatic.ButtonDriver.Hardware.Services;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public interface IStatusViewModel
    {
        ConnectionState ConnectionState { get; }
        bool DiagonalSpeedReduction { get; }
        Point JoystickPosition { get; }
        Direction LastDirection { get; }
        string Profile { get; }
        ReadyState ReadyState { get; }
        bool SafetyBypassStatus { get; }
        string Speed { get; }
    }

    class DesignStatusViewModel : IStatusViewModel
    {
        public ConnectionState ConnectionState => ConnectionState.Connecting;
        public bool DiagonalSpeedReduction => false;
        public Point JoystickPosition => new Point(7, 12);
        public Direction LastDirection => Direction.BackwardRight;
        public string Profile => "Indoors";
        public ReadyState ReadyState => ReadyState.Continue;
        public bool SafetyBypassStatus => false;
        public string Speed => "Walk";
    }
}
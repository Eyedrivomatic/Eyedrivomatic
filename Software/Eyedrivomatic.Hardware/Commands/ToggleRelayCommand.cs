using System;

namespace Eyedrivomatic.Hardware.Commands
{
    internal class ToggleRelayCommand : DeviceCommandBase
    {
        internal ToggleRelayCommand(uint relay, TimeSpan duration)
        {
            Relay = relay;
            Duration = duration;
        }

        public uint Relay { get; }
        public TimeSpan Duration { get; }

        public override string Name => $"Toggle Relay {Relay}";

        public override string ToString()
        {
            return $"SWITCH {Relay} {Duration.TotalMilliseconds} ON";
        }
    }
}
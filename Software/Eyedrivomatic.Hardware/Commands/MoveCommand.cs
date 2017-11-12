using System;

namespace Eyedrivomatic.Hardware.Commands
{
    internal class MoveCommand : DeviceCommandBase
    {
        internal MoveCommand(int xPositionRelative, int yPositionRelative, TimeSpan duration)
        {
            XPositionRelative = xPositionRelative;
            YPositionRelative = yPositionRelative;
            Duration = duration;
        }

        public int XPositionRelative { get; }
        public int YPositionRelative { get; }
        public TimeSpan Duration { get; }

        public override string Name => "Move";

        public override int Retries => Math.Min(base.Retries, 2); //Move only gets 2 or less tries

        public override string ToString()
        {
            return $"MOVE {Duration.TotalMilliseconds} {XPositionRelative} {YPositionRelative}";
        }
    }
}
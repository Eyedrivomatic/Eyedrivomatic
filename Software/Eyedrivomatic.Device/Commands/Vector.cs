using System;

namespace Eyedrivomatic.Device.Commands
{
    public struct Vector
    {
        public decimal Direction;
        public decimal Speed;

        public Vector(decimal direction, decimal speed)
        {
            Direction = direction;
            Speed = speed;
        }

        public static implicit operator Point(Vector vector)
        {
            return new Point(
                (decimal)Math.Cos((double)vector.Direction) * vector.Speed,
                (decimal)Math.Sin((double)vector.Direction) * vector.Speed);
        }

        public static Vector Parse(string str)
        {
            var parts = str.Split(',');
            if (parts.Length != 2) throw new ArgumentException();
            if (!decimal.TryParse(parts[0], out decimal direction)) throw new ArgumentException();
            if (!decimal.TryParse(parts[1], out decimal speed)) throw new ArgumentException();
            return new Vector(direction, speed);
        }
    }
}
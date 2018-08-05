using System;

namespace Eyedrivomatic.Device.Commands
{
    public struct Point
    {
        public decimal X;
        public decimal Y;

        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector(Point point)
        {
            return new Vector(
                (decimal)Math.Atan2((double)point.Y, (double)point.X),
                (decimal)Math.Sqrt(Math.Pow((double)point.X, 2) + Math.Pow((double)point.Y, 2)));
        }

        public override string ToString()
        {
            return $"{X:F1},{X:F1}";
        }

        public static Point Parse(string str)
        {
            var parts = str.Split(',');
            if (parts.Length != 2) throw new ArgumentException();
            if (!decimal.TryParse(parts[0], out decimal x)) throw new ArgumentException();
            if (!decimal.TryParse(parts[1], out decimal y)) throw new ArgumentException();
            return new Point(x, y);
        }
    }
}
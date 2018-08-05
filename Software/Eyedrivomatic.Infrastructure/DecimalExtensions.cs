using System;

namespace Eyedrivomatic.Infrastructure
{
    public static class DecimalExtensions
    {
        public static decimal Abs(this decimal value)
        {
            return Math.Abs(value);
        }
    }
}

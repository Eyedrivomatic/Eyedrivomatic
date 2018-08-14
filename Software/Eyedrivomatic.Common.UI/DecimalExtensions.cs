using System;

namespace Eyedrivomatic.Common.UI
{
    public static class DecimalExtensions
    {
        public static decimal Abs(this decimal value)
        {
            return Math.Abs(value);
        }
    }
}

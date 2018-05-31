using System.Windows;

namespace Eyedrivomatic.Eyegaze
{
    public interface IPointFilter
    {
        Point FilterPoint(Point measured);
    }
}

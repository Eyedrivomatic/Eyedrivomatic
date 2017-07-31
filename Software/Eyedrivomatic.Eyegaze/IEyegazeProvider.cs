using System;
using System.Windows;

namespace Eyedrivomatic.Eyegaze
{
    public interface IEyegazeProvider
    {
        IDisposable RegisterElement(UIElement element, IEyegazeClient client);
    }

    public interface IEyegazeClient
    {
        void ManualActivation();

        void GazeEnter(Point point);

        void GazeMove(Point point);

        void GazeLeave();
    }
}
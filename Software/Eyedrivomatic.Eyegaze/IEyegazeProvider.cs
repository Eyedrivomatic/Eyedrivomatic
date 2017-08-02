using System;
using System.Windows;

namespace Eyedrivomatic.Eyegaze
{
    public interface IEyegazeProvider : IDisposable
    {
        IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client);
    }

    public interface IEyegazeClient
    {
        void ManualActivation();

        void GazeEnter();

        void GazeContinue();

        void GazeLeave();
    }
}
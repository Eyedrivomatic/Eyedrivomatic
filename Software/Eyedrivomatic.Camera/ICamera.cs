using System;
using System.Drawing;

namespace Eyedrivomatic.Camera
{
    public interface ICamera : IDisposable
    {
        double OverlayOpacity { get; }
        bool IsCapturing { get; }

        void StartCapture();
        void StopCapture();

        event EventHandler<Bitmap> FrameCaptured;
        event EventHandler IsCapturingChanged;
    }
}
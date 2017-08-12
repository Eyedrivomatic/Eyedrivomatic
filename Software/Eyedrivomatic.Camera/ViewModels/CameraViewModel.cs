using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Accord.Video;
using NullGuard;
using Prism.Mvvm;
using Point = System.Drawing.Point;

namespace Eyedrivomatic.Camera.ViewModels
{
    [Export]
    public class CameraViewModel : BindableBase, IDisposable
    {
        private readonly ICamera _camera;
        private readonly ICameraConfigurationService _cameraConfiguration;
        private Dispatcher _dispatcher;
        private WriteableBitmap _frameSource;

        [ImportingConstructor]
        public CameraViewModel(ICamera camera, ICameraConfigurationService cameraConfiguration)
        {
            _camera = camera;
            _cameraConfiguration = cameraConfiguration;
            _camera.FrameCaptured += CameraOnFrameCaptured;
        }

        public void StartCapture()
        {
            if (_dispatcher == null) _dispatcher = Dispatcher.CurrentDispatcher;
            _camera.StartCapture();
        }

        public void StopCapture()
        {
            _camera.StopCapture();
        }

        [AllowNull]
        public BitmapSource FrameSource
        {
            get => _frameSource;
            private set => SetProperty(ref _frameSource, value as WriteableBitmap);
        }

        private void CameraOnFrameCaptured(object sender, Bitmap bitmap)
        {
            if (bitmap == null) return;

            _dispatcher.Invoke(() =>
            {
                var destFrame = PrepareFrame(bitmap);
                if (destFrame == null) return;

                var rect = new Int32Rect(0, 0, bitmap.Width, bitmap.Height);

                var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size),
                    ImageLockMode.ReadOnly, bitmap.PixelFormat);

                try
                {
                    destFrame.WritePixels(rect, data.Scan0, data.Stride * data.Height, data.Stride);
                }
                finally
                {
                    bitmap.UnlockBits(data);
                }
            });
        }

        private WriteableBitmap PrepareFrame(Image nativeFrame)
        {
            var frameSource = _frameSource;

            if (nativeFrame.Width == 0 || nativeFrame.Height == 0) return null;
            if (frameSource == null || frameSource.PixelHeight != nativeFrame.Height || frameSource.PixelWidth != nativeFrame.Width)
            {
                frameSource = new WriteableBitmap(nativeFrame.Width, nativeFrame.Height,
                    nativeFrame.HorizontalResolution, nativeFrame.HorizontalResolution, PixelFormats.Bgr24, null);
                FrameSource = frameSource;
            }

            return frameSource;
        }

        public void Dispose()
        {
            _camera.FrameCaptured -= CameraOnFrameCaptured;
            _camera.Dispose();
        }
    }
}

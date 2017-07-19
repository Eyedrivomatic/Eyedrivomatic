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
using Accord.Video.DirectShow;
using Eyedrivomatic.Infrastructure;
using NullGuard;
using Prism.Mvvm;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace Eyedrivomatic.Camera.ViewModels
{
    [Export]
    public class CameraViewModel : BindableBase, IDisposable
    {
        private readonly ICameraConfigurationService _cameraConfiguration;
        private IVideoSource _videoSource;
        private WriteableBitmap _frameSource;
        private Dispatcher _dispatcher;

        [ImportingConstructor]
        public CameraViewModel(ICameraConfigurationService cameraConfiguration)
        {
            _cameraConfiguration = cameraConfiguration;
            _cameraConfiguration.PropertyChanged += CameraConfigurationOnPropertyChanged;
        }

        private void CameraConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(ICameraConfigurationService.CameraEnabled) ||
                propertyChangedEventArgs.PropertyName == nameof(ICameraConfigurationService.Camera))
            {
                StopCapture();
                if (_cameraConfiguration.CameraEnabled)
                {
                    StartCapture();
                }
            }
        }

        public void StartCapture()
        {
            if (_dispatcher == null) _dispatcher = Dispatcher.CurrentDispatcher;

            try
            {
                if (!_cameraConfiguration.CameraEnabled) return;

                if (_videoSource == null || !_videoSource.IsRunning)
                {
                    if (_videoSource != null) _videoSource.NewFrame -= VideoSourceOnNewFrame;

                    if (_cameraConfiguration.Camera == null)
                    {
                        Log.Warn(this, "No camera available");
                        return;
                    }

                    var source = new VideoCaptureDevice(_cameraConfiguration.Camera.MonikerString, PixelFormat.Format24bppRgb);
                    _videoSource = source;
                    _videoSource.NewFrame += VideoSourceOnNewFrame;
                    _videoSource.Start();
                }

            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to start video capture - {ex}");
                if (_videoSource != null) _videoSource.NewFrame -= VideoSourceOnNewFrame;
                _videoSource = null;
            }
        }

        public void StopCapture()
        {
            if (_videoSource == null) return;
            _videoSource.NewFrame -= VideoSourceOnNewFrame;
            _videoSource.SignalToStop();
            _videoSource = null;
        }

        [AllowNull]
        public BitmapSource FrameSource
        {
            get => _frameSource;
            private set => SetProperty(ref _frameSource, value as WriteableBitmap);
        }

        private void VideoSourceOnNewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var nativeFrame = eventArgs.Frame;
            if (nativeFrame == null) return;

            _dispatcher.Invoke(() =>
            {
                var destFrame = PrepareFrame(nativeFrame);
                if (destFrame == null) return;

                var rect = new Int32Rect(0, 0, nativeFrame.Width, nativeFrame.Height);

                var data = nativeFrame.LockBits(new Rectangle(Point.Empty, nativeFrame.Size),
                    ImageLockMode.ReadOnly, nativeFrame.PixelFormat);

                try
                {
                    destFrame.WritePixels(rect, data.Scan0, data.Stride * data.Height, data.Stride);
                }
                finally
                {
                    nativeFrame.UnlockBits(data);
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
            StopCapture();
        }
    }
}

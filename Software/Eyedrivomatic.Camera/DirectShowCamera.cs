using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using Accord.Video;
using Accord.Video.DirectShow;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Camera
{
    [Export(typeof(ICamera))]
    public class DirectShowCamera : ICamera
    {
        private readonly ICameraConfigurationService _cameraConfiguration;
        private IVideoSource _videoSource;

        [ImportingConstructor]
        public DirectShowCamera(ICameraConfigurationService cameraConfiguration)
        {
            _cameraConfiguration = cameraConfiguration;
            _cameraConfiguration.PropertyChanged += CameraConfigurationOnPropertyChanged;
        }

        public double OverlayOpacity => _cameraConfiguration.OverlayOpacity;

        public bool IsCapturing => _videoSource?.IsRunning ?? false;

        public void StartCapture()
        {
            try
            {
                if (!_cameraConfiguration.CameraEnabled) return;
                if (_videoSource?.IsRunning ?? false) return; //already running.

                if (_videoSource != null)
                {
                    _videoSource.NewFrame -= VideoSourceOnNewFrame;
                    _videoSource.PlayingFinished -= VideoSourceOnPlayingFinished;
                }

                if (_cameraConfiguration.Camera == null)
                {
                    Log.Warn(this, "No camera available");
                    return;
                }

                var source = new VideoCaptureDevice(_cameraConfiguration.Camera.MonikerString, PixelFormat.Format24bppRgb);
                _videoSource = source;
                _videoSource.NewFrame += VideoSourceOnNewFrame;
                _videoSource.PlayingFinished += VideoSourceOnPlayingFinished;
                _videoSource.Start();
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to start video capture - {ex}");
                if (_videoSource != null) _videoSource.NewFrame -= VideoSourceOnNewFrame;
                _videoSource = null;
            }
            finally
            {
                IsCapturingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void VideoSourceOnPlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            if (reason == ReasonToFinishPlaying.StoppedByUser || reason == ReasonToFinishPlaying.EndOfStreamReached)
            {
                Log.Info(this, $"Camera capture source stopped - [{reason}]");
            }
            else
            {
                Log.Warn(this, $"Camera capture source unexpectedly stopped - [{reason}]");
            }

            _videoSource = null;
            IsCapturingChanged?.Invoke(this, EventArgs.Empty);
        }

        public void StopCapture()
        {
            try
            {
                if (_videoSource == null) return;
                _videoSource.NewFrame -= VideoSourceOnNewFrame;
                _videoSource.PlayingFinished -= VideoSourceOnPlayingFinished;
                _videoSource.SignalToStop();
                _videoSource = null;
            }
            finally
            {
                IsCapturingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler<Bitmap> FrameCaptured;
        public event EventHandler IsCapturingChanged;
        public event EventHandler OverlayOpacityChanged;

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
            else if (propertyChangedEventArgs.PropertyName == nameof(ICameraConfigurationService.OverlayOpacity))
            {
                OverlayOpacityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private int _failedFrameCount;
        private void VideoSourceOnNewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            try
            {
                FrameCaptured?.Invoke(this, eventArgs.Frame);
                _failedFrameCount = 0;
            }
            catch (Exception e)
            {
                if (++_failedFrameCount == 10)
                {
                    Log.Warn(this, $"Failed to process 10 consecutive frames - [{e}]");
                }
            }
        }

        public void Dispose()
        {
            StopCapture();
        }
    }
}
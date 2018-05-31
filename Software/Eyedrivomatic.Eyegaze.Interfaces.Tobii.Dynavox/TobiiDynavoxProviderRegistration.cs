//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  

using System;
using System.Reactive;
using System.Windows;
using System.Windows.Input;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    internal class TobiiDynavoxProviderRegistration : IDisposable
    {
        private readonly FrameworkElement _element;
        private readonly IEyegazeClient _client;
        private readonly IDisposable _dataSourceRegistration;
        private readonly Action<TobiiDynavoxProviderRegistration> _completeCallback;

        private bool _hasGaze;

        public TobiiDynavoxProviderRegistration(FrameworkElement element, IEyegazeClient client, IObservable<Timestamped<Point?>> dataSource, 
            Action<TobiiDynavoxProviderRegistration> completeCallback)
        {
            _element = element;
            _client = client;
            _completeCallback = completeCallback;

            _dataSourceRegistration = dataSource.Subscribe(OnNext, OnError, OnCompleted);

            _element.MouseDown += ElementOnMouseDown;
        }

        private void ElementOnMouseDown(object sender, MouseButtonEventArgs e)
        {
            _client.ManualActivation();
            e.Handled = true;
        }

        public void Dispose()
        {
            _dataSourceRegistration.Dispose();
            _element.MouseDown -= ElementOnMouseDown;
            _completeCallback(this);
        }

        private void OnNext(Timestamped<Point?> gazePoint)
        {
            try
            {
                var point = gazePoint.Value;
                var hitTest = point != null && GazeHitTest(point.Value);

                if (!hitTest)
                {
                    if (!_hasGaze) return;

                    _client.GazeLeave();
                    _hasGaze = false;
                    Log.Debug(this, $"Gaze lost for element [{_element.LoggingToString()}].");
                    return;
                }

                if (!_hasGaze)
                {
                    Log.Debug(this, $"Gaze over element [{_element.LoggingToString()}].");

                    _hasGaze = true;
                    _client.GazeEnter();
                    return;
                }

                _client.GazeContinue();
            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to process point on [{_element.LoggingToString()}] - [{ex}].");
            }
        }

        private void OnError(Exception error)
        {
            Log.Error(this, $"Gaze data stream error for element [{_element.LoggingToString()}] - [{error}].");
            if (!_hasGaze) return;
            _hasGaze = false;
            _client.GazeLeave();
        }

        private void OnCompleted()
        {
            if (!_hasGaze) return;
            _hasGaze = false;
            _client.GazeLeave();
            Log.Debug(this, $"Gaze stream completed for element [{_element.LoggingToString()}].");
        }

        private bool GazeHitTest(Point point)
        {
            try
            {
                if (!_element.IsVisible || !_element.IsEnabled) return false;
                var hitTest = _element.GazeHitTest(_element.PointFromScreen(point));
                return ReferenceEquals(_element, hitTest?.VisualHit);
            }
            catch (Exception ex)
            {
                Log.Debug(this, $"Gaze hit test error for element [{_element.LoggingToString()}]. Visible:[{_element.IsVisible}] - [{ex.Message}]");
                return false;
            }
        }
    }
}

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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Logging;
using Tobii.Gaze.Core;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    public class DataStreamFilter : IDisposable
    {
        private readonly IObservable<Point?> _dataStream;
        private readonly IEyeTracker _host;
        private readonly List<IDisposable> _registrations = new List<IDisposable>();
        private readonly IDisposable _publishRegistration;

        private static readonly Dictionary<TrackingStatus, Func<GazeData, Point?>> DataFilter = new Dictionary<TrackingStatus, Func<GazeData, Point?>>
        {
            { TrackingStatus.NoEyesTracked, data => null },
            { TrackingStatus.OneEyeTrackedUnknownWhich, data => null },
            { TrackingStatus.BothEyesTracked, data => new Point((data.Left.GazePointOnDisplayNormalized.X+data.Right.GazePointOnDisplayNormalized.X)/2, (data.Left.GazePointOnDisplayNormalized.Y+data.Right.GazePointOnDisplayNormalized.Y)/2) },
            { TrackingStatus.OneEyeTrackedProbablyLeft, data => new Point(data.Left.GazePointOnDisplayNormalized.X, data.Left.GazePointOnDisplayNormalized.Y) },
            { TrackingStatus.OneEyeTrackedProbablyRight, data => new Point(data.Right.GazePointOnDisplayNormalized.X, data.Right.GazePointOnDisplayNormalized.Y ) },
            { TrackingStatus.OnlyLeftEyeTracked, data => new Point(data.Left.GazePointOnDisplayNormalized.X, data.Right.GazePointOnDisplayNormalized.Y ) },
            { TrackingStatus.OnlyRightEyeTracked, data => new Point(data.Right.GazePointOnDisplayNormalized.X, data.Right.GazePointOnDisplayNormalized.Y ) }
        };


        public DataStreamFilter(IEyeTracker host)
        {
            _host = host;
            var datastream = Observable
                .FromEventPattern<EventHandler<GazeDataEventArgs>, GazeDataEventArgs>(o => host.GazeData += o, o => host.GazeData -= o)
                .SubscribeOnDispatcher()
                .Select(e => DataFilter[e.EventArgs.GazeData.TrackingStatus](e.EventArgs.GazeData))
                .Select(ScreenPointFromNormal)
                .ObserveOnDispatcher()
                .Publish();

            _dataStream = datastream;
            _publishRegistration = datastream.Connect();

            Log.Debug(this, $"DataStream  filter created. Screen width [{SystemParameters.PrimaryScreenWidth}] height:[{SystemParameters.PrimaryScreenHeight}]");
        }

        private static Point? ScreenPointFromNormal(Point? normalizedPoint)
        {
            if (normalizedPoint == null) return null;

            var screenPoint = new Point(
                SystemParameters.PrimaryScreenWidth * normalizedPoint.Value.X,
                SystemParameters.PrimaryScreenHeight * normalizedPoint.Value.Y);
            //Log.Debug(nameof(DataFilter), $"Gaze Point - Normalized:[{normalizedPoint}], Screen:[{screenPoint}]");
            return screenPoint;
        }

        public IDisposable AddRegistration(FrameworkElement element, IEyegazeClient client)
        {
            var lossOfGazeSent = false;

            if (!_registrations.Any())
            {
                _host.StartTrackingAsync(code =>
                {
                    if (code == ErrorCode.Success) Log.Debug(this, "Tracking started.");
                    else Log.Error(this, $"Failed to start tracking [{code}]");
                });
            }

            var stream = _dataStream
                    .Select(point => !point.HasValue || !ReferenceEquals(element, element.GazeHitTest(element.PointFromScreen(point.Value), 20)?.VisualHit) ? null : point)
                    .Where(point => point.HasValue || !lossOfGazeSent) //don't hound our elements. Just send a null normalizedPoint once to indicate gaze lost.
                    .Do(point => lossOfGazeSent = !point.HasValue);


            var registration = new TobiiDynavoxProviderRegistration(element, client, stream, r =>
            {
                _registrations.Remove(r);
                if (!_registrations.Any())
                {
                    _host.StopTrackingAsync(code =>
                    {
                        if (code == ErrorCode.Success) Log.Debug(this, "Tracking stopped.");
                        else Log.Error(this, $"Failed to stop tracking [{code}]");
                    });
                }
            });
            _registrations.Add(registration);
            return registration;
        }

        public void Dispose()
        {
            _publishRegistration?.Dispose();
        }
    }

}
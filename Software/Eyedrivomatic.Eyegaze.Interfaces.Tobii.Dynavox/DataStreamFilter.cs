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
using System.Reactive.Linq;
using System.Windows;
using Eyedrivomatic.Eyegaze.DwellClick;
using Eyedrivomatic.Eyegaze.Interfaces.Dynavox.Interop;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    public partial class TobiiDynavoxEyegazeProvider
    {
        public class DataStreamFilter
        {
            private readonly IObservable<Point?> _dataStream;

            private static readonly Dictionary<GazeData.TrackingStatus, Func<GazeData, Point?>> DataFilter = new Dictionary<GazeData.TrackingStatus, Func<GazeData, Point?>>
            {
                { GazeData.TrackingStatus.NoEyesTracked, data => null },
                { GazeData.TrackingStatus.OneEyeTrackedUnknownWhich, data => null },
                { GazeData.TrackingStatus.BothEyesTracked, data => new Point((data.GazePointLeft.X+data.GazePointRight.X)/2d, (data.GazePointLeft.Y+data.GazePointRight.Y)/2d) },
                { GazeData.TrackingStatus.OneEyeTrackedProbablyLeft, data => new Point(data.GazePointLeft.X, data.GazePointLeft.Y) },
                { GazeData.TrackingStatus.OneEyeTrackedProbablyRight, data => new Point(data.GazePointRight.X, data.GazePointRight.Y ) },
                { GazeData.TrackingStatus.OnlyLeftEyeTracked, data => new Point(data.GazePointLeft.X, data.GazePointRight.Y ) },
                { GazeData.TrackingStatus.OnlyRightEyeTracked, data => new Point(data.GazePointRight.X, data.GazePointRight.Y ) }
            };


            public DataStreamFilter(IDynavoxHost host)
            {
                _dataStream = host.DataStream
                    .SubscribeOnDispatcher()
                    .Select(data => DataFilter[data.Status](data));
            }

            public IDisposable AddRegistration(FrameworkElement element, IEyegazeClient client)
            {
                var lossOfGazeSent = false;

                var stream = _dataStream
                        .Select(point => !point.HasValue || !ReferenceEquals(element, element.GazeHitTest(point.Value, 20)?.VisualHit) ? null : point)
                        .Where(point => point.HasValue || !lossOfGazeSent) //don't hound our elements. Just send a null point once to indicate gaze lost.
                        .Do(point => lossOfGazeSent = !point.HasValue);
                return new TobiiDynavoxProviderRegistration(element, client, stream);
            }
        }
    }


}
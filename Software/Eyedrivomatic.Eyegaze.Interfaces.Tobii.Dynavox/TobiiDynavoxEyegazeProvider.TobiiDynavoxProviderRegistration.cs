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
using System.Windows.Input;
using System.Windows.Media;
using Eyedrivomatic.Eyegaze.Interfaces.Dynavox.Interop;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii.Dynavox
{
    public partial class TobiiDynavoxEyegazeProvider
    {
        private class TobiiDynavoxProviderRegistration : IDisposable
        {
            private readonly FrameworkElement _element;
            private readonly IEyegazeClient _client;
            private readonly IDisposable _dataSourceRegistration;

            public TobiiDynavoxProviderRegistration(FrameworkElement element, IEyegazeClient client, IObservable<GazeData> dataSource)
            {
                _element = element;
                _client = client;

                _dataSourceRegistration = dataSource
                    .Select(data => DataFilter[data.Status](data))
                    .Where(point => point != null && VisualTreeHelper.HitTest(element, point.Value).VisualHit == element).Subscribe(OnNext, OnCompleted, OnError);

                _element.MouseDown += ElementOnMouseDown;
            }

            private bool _hasGaze;

            private void ElementOnMouseDown(object sender, MouseButtonEventArgs e)
            {
                _client.ManualActivation();
                e.Handled = true;
            }

            public void Dispose()
            {
                _dataSourceRegistration.Dispose();
                _element.MouseDown -= ElementOnMouseDown;
            }

            private void OnNext(Point? point)
            {
                if (point == null)
                {
                    if (_hasGaze) _client.GazeLeave();
                    _hasGaze = false;
                    return;
                }

                if (!_hasGaze)
                {
                    _hasGaze = true;
                    _client.GazeEnter();
                }
                _client.GazeContinue();
            }

            private void OnError(Exception error)
            {
                Log.Error(this, $"Gaze data source error [{error}].");
                if (!_hasGaze) return;
                _hasGaze = false;
                _client.GazeLeave();
            }

            private void OnCompleted()
            {
                if (!_hasGaze) return;
                _hasGaze = false;
                _client.GazeLeave();
            }
        }
    }
}
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Eyedrivomatic.Eyegaze.DwellClick;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using Tobii.Interaction.Wpf;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii
{
    public partial class TobiiEyegazeProvider
    {
        private class TobiiProviderRegistration : IDisposable
        {
            private readonly WpfInteractor _interactor;
            private readonly IEyegazeClient _client;
            private readonly Action<TobiiProviderRegistration> _completedCallback;
            private readonly Dispatcher _dispatcher;

            public TobiiProviderRegistration(WpfInteractor interactor, IEyegazeClient client, Action<TobiiProviderRegistration> completedCallback)
            {
                _interactor = interactor;
                _interactor.WithGazeAware().HasGaze(HasGaze).LostGaze(LostGaze).Mode = GazeAwareMode.Normal;
                _interactor.GetGazePointDataStream().GazePoint(GazePoint);

                _interactor.Element.MouseDown += ElementOnMouseDown;

                _interactor.SetIsEnabled(_interactor.Element.IsVisible);
                _interactor.Element.IsVisibleChanged += ElementOnIsVisibleChanged;

                _client = client;
                _completedCallback = completedCallback;
                _dispatcher = Dispatcher.CurrentDispatcher;
            }

            private void ElementOnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
            {
                _interactor.SetIsEnabled(_interactor.Element.IsVisible);
            }

            private void GazePoint(double x, double y, double timestamp)
            {
                if (!_hasGaze) return;

                _dispatcher.Invoke(() =>
                {
                    if (!_hasGaze) return;
                    var element = _interactor.Element;
                    if (!element.IsVisible) return;

                    var point = element.PointFromScreen(new Point(x, y));
                    var hitTest = element.GazeHitTest(point);

                    if (hitTest != null && !ReferenceEquals(element, hitTest.VisualHit))
                    {
                        //visual child has gaze.
                        //Console.WriteLine($"Child has gaze - {element.LoggingToString()} {x}, {y}");
                        _hasGaze = false;
                        _client.GazeLeave();
                        return;
                    }
                    _client.GazeContinue();
                });
            }

            private bool _hasGaze;

            private void LostGaze()
            {
                if (!_hasGaze) return;
                _hasGaze = false;
                _dispatcher.Invoke(_client.GazeLeave);
            }

            private void HasGaze()
            {
                if (_hasGaze) return;
                _hasGaze = true;
                _dispatcher.Invoke(_client.GazeEnter);
            }

            private void ElementOnMouseDown(object sender, MouseButtonEventArgs e)
            {
                _client.ManualActivation();
                e.Handled = true;
            }

            public void Dispose()
            {
                _interactor.SetIsEnabled(false);
                _interactor.Element.IsVisibleChanged -= ElementOnIsVisibleChanged;
                _interactor.Element.MouseDown -= ElementOnMouseDown;
                _completedCallback(this);
            }
        }
    }
}
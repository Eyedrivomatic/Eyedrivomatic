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
            private readonly Dispatcher _dispatcher;

            public TobiiProviderRegistration(WpfInteractor interactor, IEyegazeClient client)
            {
                _interactor = interactor;
                _interactor.WithGazeAware().HasGaze(HasGaze).LostGaze(LostGaze).Mode = GazeAwareMode.Normal;
                _interactor.GetGazePointDataStream().GazePoint(GazePoint);
                _interactor.Element.MouseDown += ElementOnMouseDown;

                _client = client;
                _dispatcher = Dispatcher.CurrentDispatcher;
            }

            private void GazePoint(double x, double y, double timestamp)
            {
                _dispatcher.InvokeAsync(() =>
                {
                    if (!_hasGaze) return;
                    var hitTest = _interactor.Element.GazeHitTest(new Point(x, y));
                    if (hitTest != null && !ReferenceEquals(_interactor.Element, hitTest.VisualHit))
                    {
                        Console.WriteLine($"{_interactor.Element.Name} {x}, {y}");
                        return; //child element.
                    }
                    _client.GazeContinue();
                });
            }

            private bool _hasGaze;

            private void LostGaze()
            {
                _dispatcher.InvokeAsync(() =>
                {
                    if (!_hasGaze) return;
                    _hasGaze = false;
                    _client.GazeLeave();
                });
            }

            private void HasGaze()
            {
                _dispatcher.InvokeAsync(() =>
                {
                    if (_hasGaze) return;
                    _hasGaze = true;
                    _client.GazeEnter();
                });
            }

            private void ElementOnMouseDown(object sender, MouseButtonEventArgs e)
            {
                _client.ManualActivation();
                e.Handled = true;
            }

            public void Dispose()
            {
                _interactor.SetIsEnabled(false);
            }
        }
    }
}
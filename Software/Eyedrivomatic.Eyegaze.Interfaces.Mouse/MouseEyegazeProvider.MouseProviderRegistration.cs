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
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using Eyedrivomatic.Eyegaze.DwellClick;

namespace Eyedrivomatic.Eyegaze.Interfaces.Mouse
{
    public partial class MouseEyegazeProvider
    {
        private class MouseProviderRegistration : IDisposable
        {
            private readonly UIElement _element;
            private readonly IEyegazeClient _client;
            private int _mouseMoves;

            public MouseProviderRegistration(FrameworkElement element, IEyegazeClient client)
            {
                _element = element;
                _client = client;

                element.IsHitTestVisible = true;
                element.MouseEnter += MouseEnterHandler;
                element.MouseMove += MouseMoveHandler;
                element.MouseLeave += MouseLeaveHandler;
                element.MouseDown += MouseDownHandler;
            }

            private void MouseDownHandler(object sender, MouseButtonEventArgs e)
            {
                _client.ManualActivation();
                e.Handled = true;
            }

            private void MouseEnterHandler(object sender, MouseEventArgs e)
            {
                _mouseMoves = 0;
                e.Handled = true;
            }

            private void MouseLeaveHandler(object sender, MouseEventArgs e)
            {
                if (_mouseMoves >= RequiredMouseMoves) _client.GazeLeave();
                _mouseMoves = 0;
            }

            private void MouseMoveHandler(object sender, MouseEventArgs e)
            {
                e.Handled = true;

                var position = e.GetPosition(_element);

                if (!ReferenceEquals(_element, _element.GazeHitTest(position)?.VisualHit))
                {
                    //Child element with gaze interaction is selected.
                    _mouseMoves = 0;
                    if (_mouseMoves > RequiredMouseMoves) _client.GazeLeave();
                    return;
                }

                //Only start the animation when the mouse has moved a specified number of times after MouseEnter or the last click.
                //This prevents unintended double-clicks if the gaze tracking is lost.
                if (_mouseMoves == RequiredMouseMoves)
                {
                    _mouseMoves++;
                    _client.GazeEnter();
                    return;
                }

                if (_mouseMoves > RequiredMouseMoves)
                {
                    _client.GazeContinue();
                    return;
                }

                _mouseMoves++;
            }

            public void Dispose()
            {
                _element.MouseEnter += MouseEnterHandler;
                _element.MouseMove += MouseMoveHandler;
                _element.MouseLeave += MouseLeaveHandler;
                _element.MouseDown += MouseDownHandler;
            }
        }
    }
}

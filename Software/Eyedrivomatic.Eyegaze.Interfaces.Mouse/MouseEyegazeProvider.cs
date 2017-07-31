using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace Eyedrivomatic.Eyegaze.Interfaces.Mouse
{
    [ExportEyegazeProvider("Mouse"), PartCreationPolicy(CreationPolicy.Shared)]
    public class MouseEyegazeProvider : IEyegazeProvider
    {
        private class MouseProviderRegistration : IDisposable
        {
            private readonly UIElement _element;
            private readonly IEyegazeClient _client;
            private int _mouseMoves;
            private IDisposable _moveWatchdogRegistration;

            public MouseProviderRegistration(UIElement element, IEyegazeClient client)
            {
                _element = element;
                _client = client;

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
                _moveWatchdogRegistration?.Dispose();

                if (_mouseMoves >= RequiredMouseMoves) _client.GazeLeave();
                _mouseMoves = 0;
            }

            private void MouseMoveHandler(object sender, MouseEventArgs e)
            {
                if (!(sender is UIElement element)) return;

                e.Handled = true;

                _moveWatchdogRegistration?.Dispose();
                _moveWatchdogRegistration = null;

                //Only start the animation when the mouse has moved a specified number of times after MouseEnter or the last click.
                //This prevents unintended double-clicks if the gaze tracking is lost.
                if (_mouseMoves == RequiredMouseMoves)
                {
                    _mouseMoves++;
                    _client.GazeEnter(e.GetPosition(element));
                    StartMovementWatchdog();
                    return;
                }

                if (_mouseMoves > RequiredMouseMoves)
                {
                    _client.GazeMove(e.GetPosition(element));
                    StartMovementWatchdog();
                    return;
                }

                StartMovementWatchdog();
                _mouseMoves++;
            }

            public void Dispose()
            {
                _element.MouseEnter += MouseEnterHandler;
                _element.MouseMove += MouseMoveHandler;
                _element.MouseLeave += MouseLeaveHandler;
                _element.MouseDown += MouseDownHandler;
            }

            private void StartMovementWatchdog()
            {
                _moveWatchdogRegistration?.Dispose();

                var cts = new CancellationTokenSource(MouseMoveWatchdogTime);
                _moveWatchdogRegistration = cts.Token.Register(() =>
                {
                    _mouseMoves = 0;
                    _client.GazeLeave();
                }, true);
            }
        }

        //[Import(nameof(RequiredMouseMoves))]
        public static int RequiredMouseMoves = 2; //the number of mouse moves that need to happen before a click can begin.

        //[Import(nameof(MouseMoveWatchdogTime))]
        public static TimeSpan MouseMoveWatchdogTime = TimeSpan.FromMilliseconds(400);

        public IDisposable RegisterElement(UIElement element, IEyegazeClient client)
        {
            return new MouseProviderRegistration(element, client);
        }
    }
}

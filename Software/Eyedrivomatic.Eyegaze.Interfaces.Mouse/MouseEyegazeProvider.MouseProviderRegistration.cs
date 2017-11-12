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

                if (IsOverClickableVisibleChild(position))
                {
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

            private bool IsOverClickableVisibleChild(Point point)
            {
                var result = VisualTreeHelper.HitTest(_element, point);

                var visual = result?.VisualHit;
                while (!(visual?.Equals(_element) ?? true))
                {
                    var behaviors = Interaction.GetBehaviors(visual);
                    if (behaviors.OfType<DwellClickBehavior>().Any()) return true;
                    visual = VisualTreeHelper.GetParent(visual);
                }

                return false;
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

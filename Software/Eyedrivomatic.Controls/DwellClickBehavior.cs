// Copyright (c) 2016 Eyedrivomatic Authors
//
// This file is part of the 'Eyedrivomatic' PC application.
//
//    This program is intended for use as part of the 'Eyedrivomatic System' for 
//    controlling an electric wheelchair using soley the user's eyes. 
//
//    Eyedrivomatic is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    Eyedrivomaticis distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with Eyedrivomatic.  If not, see <http://www.gnu.org/licenses/>.


using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

using Prism.Logging;
using System.Threading.Tasks;

namespace Eyedrivomatic.Controls
{
    public class DwellClickBehavior : Behavior<Button>
    {
        private Storyboard _dwellStoryboard;
        private IDisposable _dwellCancellation;
        public DwellClickAdorner Adorner { get; set; }

        public static ILoggerFacade Logger { get; set; }

        public DwellClickBehavior() : base()
        {
        }

        public static bool GetEnabled(DependencyObject obj)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));
            return (bool)obj.GetValue(EnabledProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));
            obj.SetValue(EnabledProperty, value);
        }

        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(DwellClickBehavior), new FrameworkPropertyMetadata(false, OnEnabledChanged));

        private static void OnEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(d);
            var dwellClickBehavior = behaviors.OfType<DwellClickBehavior>().SingleOrDefault();

            if ((bool)e.NewValue)
            {
                if (dwellClickBehavior == null)
                {
                    dwellClickBehavior = new DwellClickBehavior();
                    behaviors.Add(dwellClickBehavior);
                }
            }
            else
            {
                if (dwellClickBehavior != null) behaviors.Remove(dwellClickBehavior);
            }
        }



        public static Style GetAdornerStyle(DependencyObject obj)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));
            return (Style)obj.GetValue(AdornerStyleProperty);
        }

        public static void SetAdornerStyle(DependencyObject obj, Style value)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));
            obj.SetValue(AdornerStyleProperty, value);
        }

        public static readonly DependencyProperty AdornerStyleProperty =
            DependencyProperty.RegisterAttached("AdornerStyle", typeof(Style), typeof(DwellClickBehavior), new FrameworkPropertyMetadata(null, OnAdornerStyleChanged));

        private static void OnAdornerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(d);
            var dwellClickBehavior = behaviors.OfType<DwellClickBehavior>().SingleOrDefault();
            if (dwellClickBehavior == null) return;

            var style = (Style)e.NewValue;
            if (dwellClickBehavior.Adorner != null) dwellClickBehavior.Adorner.Style = style;
        }





        /// <summary>
        /// After this amount of time passes expressed in milliseconds, a mouse click will be simulated.
        /// </summary>
        public Duration DwellTime
        {
            get { return (Duration)GetValue(DwellTimeProperty); }
            set { SetValue(DwellTimeProperty, value); }
        }
        public static readonly DependencyProperty DwellTimeProperty =
            DependencyProperty.Register(nameof(DwellTime), typeof(Duration), typeof(DwellClickBehavior), new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(1000))));

        public Duration DwellCancelTimeout
        {
            get { return (Duration)GetValue(DwellCancelTimeoutProperty); }
            set { SetValue(DwellCancelTimeoutProperty, value); }
        }
        public static readonly DependencyProperty DwellCancelTimeoutProperty =
            DependencyProperty.Register(nameof(DwellCancelTimeout), typeof(Duration), typeof(DwellClickBehavior), new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(500))));


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Logger?.Log("MouseEnter", Category.Debug, Priority.None);

            e.Handled = true;

            _dwellCancellation?.Dispose();
            _dwellCancellation = null;

            if (_dwellStoryboard == null)
            {
                CreateAdorner();
                CreateAnimation();

                StartAnimation();

                Adorner.Visibility = Visibility.Visible;
            }
            else
            {
                ResumeAnimation();
            }
        }

        private void CreateAnimation()
        {
            Logger?.Log("Creating dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard = new Storyboard();

            var dwellAnimation = new DoubleAnimation(0.0, 1.0, DwellTime);
            dwellAnimation.Completed += (s, a) =>
            {
                Logger?.Log("Performing dwell click!", Category.Debug, Priority.None);

                AssociatedObject.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                if (AssociatedObject.Command?.CanExecute(AssociatedObject.CommandParameter) ?? false) AssociatedObject.Command.Execute(AssociatedObject.CommandParameter);

                RemoveAdorner();
                _dwellStoryboard = null;
            };

            Storyboard.SetTarget(dwellAnimation, Adorner);
            Storyboard.SetTargetProperty(dwellAnimation, new PropertyPath(DwellClickAdorner.DwellProgressProperty));

            _dwellStoryboard.Children.Add(dwellAnimation);
        }

        private void StartAnimation()
        {
            Logger?.Log("Starting dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard.Begin();
        }

        private void PauseAnimation()
        {
            Logger?.Log("Pausing dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard.Pause();
        }

        private void ResumeAnimation()
        {
            Logger?.Log("Resuming the dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard.Resume();
        }

        private void StopAnimation()
        {
            _dwellStoryboard?.Stop();
            _dwellStoryboard = null;

            Logger?.Log("Stopped the dwell click animation.", Category.Debug, Priority.None);
        }

        private void CreateAdorner()
        {
            Logger?.Log("Creating dwell click adorner.", Category.Debug, Priority.None);

            var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
            if (adornerLayer == null) return;

            if (Adorner == null)
            {
                Adorner = new DwellClickAdorner(AssociatedObject)
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                var style = GetAdornerStyle(AssociatedObject);
                if (style != null) Adorner.Style = style;
            }

            adornerLayer.Add(Adorner);
            adornerLayer.Update(AssociatedObject);
        }

        private void RemoveAdorner()
        {
            if (Adorner != null)
            {
                Dispatcher.Invoke(() =>
                {
                    Logger?.Log("Removing dwell click adorner.", Category.Debug, Priority.None);

                    Adorner.Visibility = Visibility.Collapsed;

                    var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                    if (adornerLayer == null) return;
                    adornerLayer.Remove(Adorner);
                });
            }
        }

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Logger?.Log("MouseLeave", Category.Debug, Priority.None);

            e.Handled = true;

            AssociatedObject.ReleaseMouseCapture();

            if (_dwellStoryboard != null)
            {
                PauseAnimation();
                StartCancelTimer();
            }
        }

        private void StartCancelTimer()
        {
            //Set a timer that resets the dwell to 0.
            var cancelTimeout = DwellCancelTimeout.HasTimeSpan ? DwellCancelTimeout.TimeSpan : TimeSpan.FromMilliseconds(250);
            var cancelation = new CancellationTokenSource(cancelTimeout);
            _dwellCancellation = cancelation.Token.Register(() =>
            {
                StopAnimation();
                RemoveAdorner();
            });
        }
    }
}

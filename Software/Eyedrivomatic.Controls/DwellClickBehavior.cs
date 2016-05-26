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
//    Eyedrivomatic is distributed in the hope that it will be useful,
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
using System.Windows.Documents;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

using Prism.Logging;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace Eyedrivomatic.Controls
{
    public class DwellClickBehavior : Behavior<UIElement>
    {
        #region DefaultConfiguration
        private static IDwellClickConfigurationService _defaultConfiguration = null; //off.
        public static IDwellClickConfigurationService DefaultConfiguration
        {
            get { return _defaultConfiguration; }
            set
            {
                if (Object.ReferenceEquals(_defaultConfiguration, value)) return;

                _defaultConfiguration = value;
                DefaultConfigurationChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler DefaultConfigurationChanged;

        #endregion DefaultConfiguration

        #region Pause (static)

        private static bool _pause;
        public static bool Pause
        {
            get { return _pause; }
            set
            {
                if (Object.ReferenceEquals(_pause, value)) return;

                _pause = value;
                PauseChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler PauseChanged;

        #endregion Pause (static)

        #region Attached Properties
        #region Configuration
        public static readonly DependencyProperty ConfigurationProperty =
            DependencyProperty.RegisterAttached("Configuration", typeof(IDwellClickConfigurationService), typeof(DwellClickBehavior), new PropertyMetadata(OnConfigurationChanged));

        public static IDwellClickConfigurationService GetConfiguration(DependencyObject obj)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));
            return (IDwellClickConfigurationService)obj.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(DependencyObject obj, IDwellClickConfigurationService value)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));
            obj.SetValue(ConfigurationProperty, value);
        }

        private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //verify that the dwell click behavior has been created.
            GetDwellClickBehavior(d);
        }

        private static DwellClickBehavior GetDwellClickBehavior(DependencyObject obj)
        {
            var behaviors = Interaction.GetBehaviors(obj);
            var dwellClickBehavior = behaviors.OfType<DwellClickBehavior>().SingleOrDefault();

            if (dwellClickBehavior == null)
            {
                dwellClickBehavior = new DwellClickBehavior();
                behaviors.Add(dwellClickBehavior);
            }

            return dwellClickBehavior;
        }
        #endregion Configuration

        #region AdornerStyle
        public static readonly DependencyProperty AdornerStyleProperty =
            DependencyProperty.RegisterAttached("AdornerStyle", typeof(Style), typeof(DwellClickBehavior), new PropertyMetadata(OnAdornerStyleChanged));

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

        private static void OnAdornerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //verify that the dwell click behavior has been created.
            var dwellClickBehavior = GetDwellClickBehavior(d);

            var style = (Style)e.NewValue;
            if (dwellClickBehavior.Adorner != null) dwellClickBehavior.Adorner.Style = style;
        }
        #endregion AdornerStyle

        #region IgnorePause
        public static readonly DependencyProperty IgnorePauseProperty =
            DependencyProperty.RegisterAttached("IgnorePause", typeof(bool), typeof(DwellClickBehavior), new PropertyMetadata(false));

        public static bool GetIgnorePause(DependencyObject obj)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));

            return (bool)obj.GetValue(IgnorePauseProperty);
        }

        public static void SetIgnorePause(DependencyObject obj, bool  value)
        {
            Contract.Requires<ArgumentNullException>(obj != null, nameof(obj));

            obj.SetValue(IgnorePauseProperty, value);
        }
        #endregion IgnorePause
        #endregion Attached Properties

        public static ILoggerFacade Logger { get; set; }


        private Storyboard _dwellStoryboard;
        private IDisposable _dwellCancelRegistration;
        private CancellationTokenSource _repeatCancelSource = null;

        public DwellClickAdorner Adorner { get; set; }

        public DwellClickBehavior() : base()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            AssociatedObject.MouseDown += AssociatedObject_MouseDown;
        }

        private void AssociatedObject_MouseDown(object sender, RoutedEventArgs e)
        {
            StopAnimation();
            RemoveAdorner();
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var configruation = GetConfiguration(AssociatedObject);

            if (configruation == null || !configruation.EnableDwellClick || Paused) return;

            _dwellCancelRegistration?.Dispose();
            _dwellCancelRegistration = null;

            if (_dwellStoryboard == null)
            {
                CreateAdorner();
                CreateAnimation(
                    TimeSpan.FromMilliseconds(configruation.DwellTimeMilliseconds),
                    TimeSpan.FromMilliseconds(configruation.RepeatDelayMilliseconds));

                StartAnimation();
            }
            else
            {
                ResumeAnimation();
            }

            if(Adorner != null) Adorner.Visibility = Visibility.Visible;
        }

        private void CreateAnimation(TimeSpan dwellTime, TimeSpan repeatDelay)
        {
            Logger?.Log("Creating dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard = new Storyboard();

            var dwellAnimation = new DoubleAnimation(0.0, 1.0, dwellTime);
            dwellAnimation.Completed += async (s, a) =>
            {
                Logger?.Log("Performing dwell click!", Category.Debug, Priority.None);

                var peer = UIElementAutomationPeer.FromElement(AssociatedObject);
                if (peer == null) peer = UIElementAutomationPeer.CreatePeerForElement(AssociatedObject);
                var clicked = InvokeElement(peer) || ToggleElement(peer) || SelectElement(peer) || SelectTabElement();

                if (!clicked) Logger?.Log("Failed to perform dwell click.", Category.Warn, Priority.None);

                _repeatCancelSource?.Cancel();

                try
                {
                    _repeatCancelSource = new CancellationTokenSource();
                    await Task.Delay(repeatDelay, _repeatCancelSource.Token);

                    StartAnimation();
                }
                catch (OperationCanceledException)
                {
                    //click repeat cancelled.
                    RemoveAdorner();
                }
            };

            Storyboard.SetTarget(dwellAnimation, Adorner);
            Storyboard.SetTargetProperty(dwellAnimation, new PropertyPath(DwellClickAdorner.DwellProgressProperty));

            _dwellStoryboard.Children.Add(dwellAnimation);
        }

        private bool InvokeElement(AutomationPeer peer)
        {
            var invokePattern = peer?.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            if (invokePattern == null) return false;
            invokePattern.Invoke();
            return true;
        }

        private bool ToggleElement(AutomationPeer peer)
        {
            var togglePattern = peer?.GetPattern(PatternInterface.Toggle) as IToggleProvider;
            if (togglePattern == null) return false;
            togglePattern.Toggle();
            return true;
        }

        private bool SelectElement(AutomationPeer peer)
        {
            var selectionPattern = peer?.GetPattern(PatternInterface.SelectionItem) as ISelectionItemProvider;
            if (selectionPattern == null) return false;
            selectionPattern.Select();
            return true;
        }

        private bool SelectTabElement()
        {
            //The AutomationPeer returned by UIElementAutomationPeer.CreatePeerForElement to a TabItem is stupid and cannot select the tab.
            //The "correct" approach is apparently to create a custom tab item adn a custom automation provider. This however works just as well.
            // It's just not as elegant as it requires down-casting... yuck!
            var tabItem = AssociatedObject as TabItem;
            if (tabItem == null) return false;
            return (tabItem.IsSelected = true);
        }

        private void StartAnimation()
        {
            Logger?.Log("Starting dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard?.Begin();
        }

        private void PauseAnimation()
        {
            Logger?.Log("Pausing dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard?.Pause();
        }

        private void ResumeAnimation()
        {
            Logger?.Log("Resuming the dwell click animation.", Category.Debug, Priority.None);
            _dwellStoryboard?.Resume();
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

        private void HideAdorner()
        {
            if (Adorner != null)
            {
                Adorner.Visibility = Visibility.Hidden;
            }
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
            if (_dwellStoryboard != null)
            {
                var configruation = GetConfiguration(AssociatedObject);

                PauseAnimation();
                HideAdorner();
                StartCancelTimer(TimeSpan.FromMilliseconds(configruation.DwellTimeoutMilliseconds));
            }
        }

        private void StartCancelTimer(TimeSpan cancelTimeout)
        {
            //Set a timer that resets the dwell to 0.
            var cancelation = new CancellationTokenSource(cancelTimeout);
            _dwellCancelRegistration = cancelation.Token.Register(() =>
            {
                StopAnimation();
                RemoveAdorner();
            }, true);
        }

        private bool Paused => Pause && !GetIgnorePause(AssociatedObject);
    }
}

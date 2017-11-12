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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interactivity;
using Eyedrivomatic.Logging;
using NullGuard;

namespace Eyedrivomatic.Eyegaze.DwellClick
{
    public static class DwellClickBehaviorFactory
    {
        public static Func<DwellClickBehavior> Create { get; set; }
    }

    [Export(typeof(DwellClickBehavior)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class DwellClickBehavior : Behavior<FrameworkElement>, IEyegazeClient
    {
        #region DefaultConfiguration
        private static IDwellClickConfigurationService _defaultConfiguration; //off.

        [AllowNull]
        public static IDwellClickConfigurationService DefaultConfiguration
        { 
            get => _defaultConfiguration;
            set
            {
                if (ReferenceEquals(_defaultConfiguration, value)) return;

                _defaultConfiguration = value;
                DefaultConfigurationChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler DefaultConfigurationChanged;

        #endregion DefaultConfiguration

        #region Pause (static)

        private static bool _pause = true;
        /// <summary>
        /// Pause is used to suspend DwellClick on all controls for which IgnorePause is set.
        /// The idea is that IgnorePause would be set on the Pause button, but nowhere else.
        /// </summary>
        public static bool Pause
        {
            get => _pause;
            set
            {
                if (_pause == value) return;

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
            return (IDwellClickConfigurationService)obj.GetValue(ConfigurationProperty);
        }

        public static void SetConfiguration(DependencyObject obj, IDwellClickConfigurationService value)
        {
            obj.SetValue(ConfigurationProperty, value);
        }

        private static void OnConfigurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //verify that the dwell click behavior has been created.
            var behaviors = Interaction.GetBehaviors(d);
            var dwellClickBehavior = behaviors.OfType<DwellClickBehavior>().SingleOrDefault();

            if (dwellClickBehavior == null)
            {
                dwellClickBehavior = DwellClickBehaviorFactory.Create();
                behaviors.Add(dwellClickBehavior);
            }
        }
        #endregion Configuration

        #region Role
        public static readonly DependencyProperty RoleProperty = 
            DependencyProperty.RegisterAttached("Role", typeof(DwellClickActivationRole), typeof(DwellClickBehavior), new PropertyMetadata(DwellClickActivationRole.Standard));

        public static void SetRole(DependencyObject obj, DwellClickActivationRole value)
        {
            obj.SetValue(RoleProperty, value);
        }

        public static DwellClickActivationRole GetRole(DependencyObject obj)
        {
            return (DwellClickActivationRole)obj.GetValue(RoleProperty);
        }
        #endregion

        #region AdornerStyle
        public static readonly DependencyProperty AdornerStyleProperty =
            DependencyProperty.RegisterAttached("AdornerStyle", typeof(Style), typeof(DwellClickBehavior), new PropertyMetadata(null));

        [return: AllowNull]
        public static Style GetAdornerStyle(DependencyObject obj)
        {
            return obj?.GetValue(AdornerStyleProperty) as Style;
        }

        public static void SetAdornerStyle(DependencyObject obj, Style value)
        {
            obj.SetValue(AdornerStyleProperty, value);
        }

        #endregion AdornerStyle

        #region IgnorePause
        public static readonly DependencyProperty IgnorePauseProperty =
            DependencyProperty.RegisterAttached("IgnorePause", typeof(bool), typeof(DwellClickBehavior), new PropertyMetadata(false));

        public static bool GetIgnorePause(DependencyObject obj)
        {
            return (bool)obj.GetValue(IgnorePauseProperty);
        }

        public static void SetIgnorePause(DependencyObject obj, bool value)
        {
            obj.SetValue(IgnorePauseProperty, value);
        }
        #endregion IgnorePause
        #endregion Attached Properties

        //If the mouse leaves the control, the animation is paused and a cancellation timer starts. 
        //If the mouse re-enters the control before the timer triggers, then the animation continues. 
        //This is to prevent a very short "eye-twitch" from stopping the dwell click.
        private IDisposable _dwellCancelRegistration;
        private CancellationTokenSource _repeatCancelSource;
        private IDisposable _providerRegistration;

        private readonly IDwellClickAnimator _animator;
        private readonly DwellClickAdornerFactory _adornerFactory;
        private readonly IEyegazeProviderFactory _providerFactory;
        private DwellClickAdorner _adorner;
        private IDisposable _moveWatchdogRegistration;
        private IDwellClickConfigurationService _configuration;

        [ImportingConstructor]
        public DwellClickBehavior(
            IEyegazeProviderFactory providerFactory,
            IDwellClickAnimator animator,
            DwellClickAdornerFactory adornerFactory)
        {
            _providerFactory = providerFactory;
            _animator = animator;
            _adornerFactory = adornerFactory;
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            _configuration = GetConfiguration(AssociatedObject);
            _configuration.PropertyChanged += ConfigOnPropertyChanged;

            AttachProvider(_configuration.Provider);
        }

        private void AttachProvider(string providerName)
        {
            try
            {
                _providerRegistration?.Dispose();
                var provider = _providerFactory.Create(providerName);
                _providerRegistration = provider?.RegisterElement(AssociatedObject, this);

            }
            catch (Exception ex)
            {
                Log.Error(this, $"Failed to attach eyegaze provider - [{ex}]");
            }
        }

        private void ConfigOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == nameof(IDwellClickConfigurationService.Provider))
            {
                AttachProvider(_configuration.Provider);
                //Let any errors fall through to event invocation to prevent further attempts to update the provider. 
            }
        }


        private static TimeSpan GetDwellTime(IDwellClickConfigurationService configuration, DwellClickActivationRole role)
        {
            var accessors = new Dictionary<DwellClickActivationRole, Func<IDwellClickConfigurationService, int>>
            {
                { DwellClickActivationRole.Standard, config => config.StandardDwellTimeMilliseconds },
                { DwellClickActivationRole.DirectionButtons, config => config.DirectionButtonDwellTimeMilliseconds},
                { DwellClickActivationRole.StartButton, config => config.StartButtonDwellTimeMilliseconds },
                { DwellClickActivationRole.StopButton, config => config.StopButtonDwellTimeMilliseconds}
            };

            var ms = accessors.ContainsKey(role) ? accessors[role](configuration) : configuration.StandardDwellTimeMilliseconds;
            return TimeSpan.FromMilliseconds(ms);
        }

        private void StartDwellClick(TimeSpan dwellTime)
        {
            _moveWatchdogRegistration?.Dispose();

            if (_adorner == null)
            {
                Log.Info(this, $"Starting dwell click on [{GetAssociatedObjectLoggingName()}]");
                CreateAdorner();
                if (_adorner == null)
                {
                    Log.Warn(this, $"Failed to create adorner on [{GetAssociatedObjectLoggingName()}]");
                    return;
                }
                _animator.StartAnimation(_adorner, dwellTime, DoClick);
            }
            else
            {
                Log.Info(this, $"Resuming dwell click on [{GetAssociatedObjectLoggingName()}]");
                //re-entry before the cancel timer has fired.
                ShowAdorner();
                _animator.ResumeAnimation();
            }
        }

        private void CancelDwellClick()
        {
            _animator?.StopAnimation();
            RemoveAdorner();
            //Log.Debug(this, $"Canceled dwell click on [{AssociatedObject}]");
        }

        private void DoClick()
        {
            RemoveAdorner();

            if (!AssociatedObject.IsEnabled)
            {
                Log.Warn(this, $"Dwell Click ignored because element [{GetAssociatedObjectLoggingName()}] is not enabled.");
                return;
            }

            Log.Info(this, $"Performing dwell click on [{GetAssociatedObjectLoggingName()}]");

            if (!DwellClicker.Click(AssociatedObject)) Log.Warn(this, $"Failed to perform dwell click on [{GetAssociatedObjectLoggingName()}].");

            _moveWatchdogRegistration?.Dispose();
            RepeatDwellAnimationAfter(TimeSpan.FromMilliseconds(_configuration.RepeatDelayMilliseconds));
        }

        private async void RepeatDwellAnimationAfter(TimeSpan repeatDelay)
        {
            _repeatCancelSource?.Cancel();
            _repeatCancelSource = new CancellationTokenSource();

            try
            {
                RemoveAdorner();
                StartMovementWatchdog(repeatDelay);
                await Task.Delay(repeatDelay, _repeatCancelSource.Token);

                if (!AssociatedObject.IsEnabled) return;
                if (!_configuration.EnableDwellClick || Paused) return;

                Log.Info(this, "Repeat-click start.");
                StartDwellClick(GetDwellTime(_configuration, GetRole(AssociatedObject)));
            }
            catch (OperationCanceledException)
            {
                //click repeat cancelled.
                Log.Debug(this, $"ClickRepeat canceled on [{GetAssociatedObjectLoggingName()}].");
            }
        }

        private void CreateAdorner()
        {
            _adorner = _adornerFactory.Create(AssociatedObject);
            if (_adorner == null) return;
            var style = GetAdornerStyle(AssociatedObject);
            if (style != null) _adorner.Style = style;
            _adorner.Visibility = Visibility.Visible;
        }

        private void ShowAdorner()
        {
            if (_adorner != null)
            {
                _adorner.Visibility = Visibility.Visible;
            }
        }

        private void HideAdorner()
        {
            if (_adorner != null)
            {
                _adorner.Visibility = Visibility.Hidden;
            }
        }

        private void RemoveAdorner()
        {
            var tmp = _adorner;
            _adorner = null;

            if (tmp != null)
            {
                tmp.Visibility = Visibility.Collapsed;

                var adornerLayer = AdornerLayer.GetAdornerLayer(AssociatedObject);
                if (adornerLayer == null) return;
                adornerLayer.Remove(tmp);
            }
        }

        private void PauseAndCancelDwellAfter(TimeSpan cancelTimeout)
        {
            _dwellCancelRegistration?.Dispose();
            //Set a timer that resets the dwell to 0.
            var cancelation = new CancellationTokenSource(cancelTimeout);
            _dwellCancelRegistration = cancelation.Token.Register(CancelDwellClick, true);
        }

        private bool Paused => Pause && !GetIgnorePause(AssociatedObject);

 
        public void ManualActivation()
        {
            _animator?.StopAnimation();
            RemoveAdorner();
        }

        public void GazeEnter()
        {
            _dwellCancelRegistration?.Dispose(); //If the cancellation timer is running, stop it.
            _dwellCancelRegistration = null;
            _moveWatchdogRegistration?.Dispose();

            if (!AssociatedObject.IsEnabled) return;
            if (!_configuration.EnableDwellClick || Paused) return;

            var role = GetRole(AssociatedObject);
            var dwellTime = GetDwellTime(_configuration, role);
            StartDwellClick(dwellTime);
            StartMovementWatchdog(dwellTime);
        }

        public void GazeContinue()
        {
            if (!_configuration.EnableDwellClick || Paused) return;

            _moveWatchdogRegistration?.Dispose();
            _moveWatchdogRegistration = null;

            var role = GetRole(AssociatedObject);
            var dwellTime = GetDwellTime(_configuration, role);
            StartMovementWatchdog(dwellTime);
        }

        public void GazeLeave()
        {
            _repeatCancelSource?.Cancel();
            _repeatCancelSource = null;

            _moveWatchdogRegistration?.Dispose();
            _moveWatchdogRegistration = null;

            _animator.PauseAnimation();
            HideAdorner();

            PauseAndCancelDwellAfter(TimeSpan.FromMilliseconds(_configuration.DwellTimeoutMilliseconds));
        }

        private void StartMovementWatchdog(TimeSpan dwellTime)
        {
            _moveWatchdogRegistration?.Dispose();
            var watchdogTime = Math.Max(dwellTime.TotalMilliseconds / 3d, 200);
            var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(watchdogTime));
            _moveWatchdogRegistration = cts.Token.Register(GazeLeave, true);
        }

        private string GetAssociatedObjectLoggingName()
        {
            if (AssociatedObject == null) return "NULL";

            if (!string.IsNullOrEmpty(AssociatedObject.Name)) return AssociatedObject.Name;
            return AssociatedObject.ToString();
        }
    }
}

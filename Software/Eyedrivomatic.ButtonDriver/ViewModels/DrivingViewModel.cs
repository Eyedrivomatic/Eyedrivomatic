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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Prism.Commands;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.Infrastructure;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Regions;
using Eyedrivomatic.Camera;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export(typeof(IDrivingViewModel))]
    public class DrivingViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>, IDrivingViewModel, INavigationAware
    {
        private readonly IEnumerable<Profile> _profiles;
        private readonly ICamera _camera;

        [ImportingConstructor]
        public DrivingViewModel(IHardwareService hardwareService,
            [Import("ExecuteMacroCommand")]ICommand executeMacroCommand,
            IMacroSerializationService macroSerializationService,
            IEnumerable<Profile> profiles,
            ICamera camera)
            : base (hardwareService)
        {
            _profiles = profiles;
            _camera = camera;
            _camera.IsCapturingChanged += CameraOnIsCapturingChanged;
            _camera.OverlayOpacityChanged += CameraOnOverlayOpacityChanged;
            ExecuteMacroCommand = executeMacroCommand;
            Macros = new ObservableCollection<IMacro>(macroSerializationService.LoadMacros());
        }

        public string HeaderInfo { get; } = Strings.DriveProfile_Default;

        public bool IsOnline => Driver?.HardwareReady ?? false;

        public bool ShowForwardView => _camera.IsCapturing;

        public bool SafetyBypass
        {
            get => IsOnline && Driver.Profile.SafetyBypass;
            set
            {
                Driver.Profile.SafetyBypass = value;
                LogSettingChange(Driver.Profile.SafetyBypass);
                RaisePropertyChanged();
            }
        }

        public IEnumerable<ProfileSpeed> Speeds => HardwareService.CurrentDriver?.Profile?.Speeds ?? Enumerable.Empty<ProfileSpeed>();

        [AllowNull]
        public ProfileSpeed CurrentSpeed
        {
            get => IsOnline ? Driver.Profile.CurrentSpeed : null;
            set
            {
                Driver.Profile.CurrentSpeed = value;
                LogSettingChange(Driver.Profile.CurrentSpeed.Name);
                RaisePropertyChanged();
            }
        }

        public double XDuration
        {
            get => IsOnline ? Driver.Profile.XDuration.TotalMilliseconds : 0;
            set
            {
                Driver.Profile.XDuration = TimeSpan.FromMilliseconds(value);
                LogSettingChange(Driver.Profile.XDuration);
                RaisePropertyChanged();
            }
        }

        public double YDuration
        {
            get => IsOnline ? Driver.Profile.YDuration.TotalMilliseconds : 0;
            set
            {
                Driver.Profile.YDuration = TimeSpan.FromMilliseconds(value);
                LogSettingChange(Driver.Profile.YDuration);
                RaisePropertyChanged();
            }
        }

        public double CameraOverlayOpacity => ShowForwardView ? _camera.OverlayOpacity : 1d;

        public ICommand ContinueCommand => new DelegateCommand(
            () => Driver.Continue(), 
            () => Driver.ReadyState == ReadyState.Continue);

        public ICommand StopCommand => new DelegateCommand(
            () => Driver.Stop(), 
            () => IsOnline);

        public ICommand NudgeCommand => new DelegateCommand<XDirection?>(
            direction => { if (direction != null) Driver.Nudge(direction.Value); }, 
            direction => direction.HasValue && IsOnline && Driver.LastDirection == Direction.Forward && Driver.CurrentDirection != Direction.None);

        public ICommand MoveCommand => new DelegateCommand<Direction?>(
            direction => { if (direction != null) Driver.Move(direction.Value); }, 
            direction => direction.HasValue && IsOnline && Driver.CanMove(direction.Value));

        public ICommand ExecuteMacroCommand { get; }

        public ICommand SetSpeedCommand => new DelegateCommand<ProfileSpeed>(
            speed =>
            {
                Driver.Profile.CurrentSpeed = speed;
                // ReSharper disable once ExplicitCallerInfoArgument
                LogSettingChange(Driver.Profile.CurrentSpeed.Name, nameof(Driver.Profile.CurrentSpeed));
                // ReSharper disable once ExplicitCallerInfoArgument
                RaisePropertyChanged(nameof(CurrentSpeed));
            },
            speed => IsOnline && speed != null);

        public ObservableCollection<IMacro> Macros { get; }


        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            var profileName = parameters["profile"].ToString();
            var profile = _profiles.FirstOrDefault(p => p.Name == profileName);

            if (profile == null)
            {
                Log.Error(this, $"Profile [{profileName}] not found!");
                profile = _profiles.First();
            }

            Log.Info(this, $"Setting driver profile to [{profile.Name}]");
            Driver.Profile = profile;
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged("");
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            var profileName = parameters["profile"].ToString();
            var profile = _profiles.FirstOrDefault(p => p.Name == profileName);

            return profile != null;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        { 
        }

        protected override void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverStateChanged(sender, e);
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(string.Empty); //Just refresh everything.
        }

        private void CameraOnOverlayOpacityChanged(object sender, EventArgs eventArgs)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(CameraOverlayOpacity));
        }

        private void CameraOnIsCapturingChanged(object sender, EventArgs eventArgs)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged(nameof(ShowForwardView));
        }
    }
}

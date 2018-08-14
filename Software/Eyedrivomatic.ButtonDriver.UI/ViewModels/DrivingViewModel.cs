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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver.Configuration;
using Eyedrivomatic.ButtonDriver.Macros.Models;
using Eyedrivomatic.ButtonDriver.Services;
using Eyedrivomatic.Camera;
using Eyedrivomatic.Common;
using Eyedrivomatic.Common.UI;
using Eyedrivomatic.Logging;
using Eyedrivomatic.Resources;
using NullGuard;
using Prism.Commands;
using Prism.Regions;

namespace Eyedrivomatic.ButtonDriver.UI.ViewModels
{
    [Export(typeof(IDrivingViewModel))]
    public class DrivingViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>, IDrivingViewModel, INavigationAware
    {
        private readonly IEnumerable<Profile> _profiles;
        private readonly ICamera _camera;
        private double _duration;

        [ImportingConstructor]
        public DrivingViewModel(IButtonDriverService driverService,
            [Import("ExecuteMacroCommand")]ICommand executeMacroCommand,
            IMacroSerializationService macroSerializationService,
            IEnumerable<Profile> profiles,
            ICamera camera)
            : base (driverService)
        {
            _profiles = profiles;
            _camera = camera;
            _camera.IsCapturingChanged += CameraOnIsCapturingChanged;
            _camera.OverlayOpacityChanged += CameraOnOverlayOpacityChanged;
            ExecuteMacroCommand = executeMacroCommand;
            Macros = new ObservableCollection<IMacro>(macroSerializationService.LoadMacros());
        }

        public string HeaderInfo { get; } = Strings.DriveProfile_Default;

        public bool IsOnline => Driver?.DeviceReady ?? false;

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

        public IEnumerable<ProfileSpeed> Speeds => InitializationService.LoadedButtonDriver?.Profile?.Speeds ?? Enumerable.Empty<ProfileSpeed>();

        [AllowNull]
        public ProfileSpeed CurrentSpeed
        {
            get => IsOnline ? Driver.Profile.CurrentSpeed : null;
            set
            {
                Driver.Profile.CurrentSpeed = value;
                LogSettingChange(Driver.Profile.CurrentSpeed?.Name);
                RaisePropertyChanged();
            }
        }

        public double Duration
        {
            get => IsOnline ? _duration : 0;
            set => SetProperty(ref _duration, value);
        }

        public double CameraOverlayOpacity => ShowForwardView ? _camera.OverlayOpacity : 1d;

        public ICommand ContinueCommand => new DelegateCommand(
            () => Driver.Continue(), 
            () => Driver.ReadyState == ReadyState.Continue);

        public ICommand StopCommand => new DelegateCommand(
            () => Driver.Stop(), 
            () => IsOnline);

        public ICommand NudgeCommand => new DelegateCommand<XDirection?>(
            direction => { if (direction != null) Driver.Nudge(direction.Value, TimeSpan.FromMilliseconds(_duration)); }, 
            direction => direction.HasValue && IsOnline && Driver.LastDirection == Direction.Forward && Driver.CurrentDirection != Direction.None && _duration > 0)
            .ObservesProperty(() => Duration);

        public ICommand MoveCommand => new DelegateCommand<Direction?>(
            direction => { if (direction != null) Driver.Move(direction.Value, TimeSpan.FromMilliseconds(Duration)); }, 
            direction => direction.HasValue && IsOnline && Driver.CanMove(direction.Value) && _duration > 0)
            .ObservesProperty(() => Duration);

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

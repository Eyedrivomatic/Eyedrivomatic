﻿//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Eyedrivomatic.ButtonDriver.Services;
using Eyedrivomatic.Common;
using Eyedrivomatic.Device.Commands;
using Eyedrivomatic.Device.Communications;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.UI.ViewModels
{
    [Export]
    public class StatusViewModel : ButtonDriverViewModelBase, IStatusViewModel
    {
        [ImportingConstructor]
        public StatusViewModel(IButtonDriver driver)
            : base(driver)
        {}

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverStateChanged(sender, e);
            if (e.PropertyName == nameof(Driver.DeviceStatus))
            {
                RaisePropertyChanged(nameof(JoystickVector));
                RaisePropertyChanged(nameof(Switch1));
                RaisePropertyChanged(nameof(Switch2));
                RaisePropertyChanged(nameof(Switch3));
                RaisePropertyChanged(nameof(Switch4));
            }

            if (e.PropertyName == nameof(Driver.Profile))
            {
                RaisePropertyChanged(nameof(Profile));
                RaisePropertyChanged(nameof(Speed));
                RaisePropertyChanged(nameof(SafetyBypassStatus));
            }

            if (e.PropertyName == nameof(Driver.ConnectionState))
            {
                RaisePropertyChanged(nameof(ConnectionState));
            }
        }

        public ConnectionState ConnectionState => Driver.ConnectionState;
        public bool SafetyBypassStatus => Driver.Profile?.SafetyBypass ?? false;

        public Direction CurrentDirection => Driver?.CurrentDirection ?? Direction.None;
        public Vector JoystickVector => Driver?.DeviceStatus.Vector ?? new Vector(0,0);
        public bool Switch1 => Driver?.DeviceStatus.Switches[1] ?? false;
        public bool Switch2 => Driver?.DeviceStatus.Switches[2] ?? false;
        public bool Switch3 => Driver?.DeviceStatus.Switches[3] ?? false;
        public bool Switch4 => Driver?.DeviceStatus.Switches[4] ?? false;

        public string Profile => Driver.Profile.Name;

        public string Speed => Driver.Profile.CurrentSpeed?.Name ?? Strings.StatusView_Speed_None;
        public ReadyState ReadyState => Driver.ReadyState;
    }
}
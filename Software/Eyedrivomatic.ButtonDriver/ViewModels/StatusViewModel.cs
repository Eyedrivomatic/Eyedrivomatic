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


using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Eyedrivomatic.ButtonDriver.Hardware.Services;
using Eyedrivomatic.Hardware.Communications;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class StatusViewModel : ButtonDriverViewModelBase, IStatusViewModel
    {
        [ImportingConstructor]
        public StatusViewModel(IHardwareService hardwareService)
            : base(hardwareService)
        {

        }

        [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
        protected override void OnDriverStateChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnDriverStateChanged(sender, e);
            if (e.PropertyName == nameof(Driver.DeviceStatus))
            {
                RaisePropertyChanged(nameof(JoystickPosition));
                RaisePropertyChanged(nameof(Switch1));
                RaisePropertyChanged(nameof(Switch2));
                RaisePropertyChanged(nameof(Switch3));
            }

            if (e.PropertyName == nameof(Driver.Profile))
            {
                RaisePropertyChanged(nameof(Profile));
                RaisePropertyChanged(nameof(Speed));
                RaisePropertyChanged(nameof(SafetyBypassStatus));
            }

            if (e.PropertyName == nameof(Driver.Connection))
            {
                RaisePropertyChanged(nameof(ConnectionState));
            }
        }

        public ConnectionState ConnectionState => Driver?.Connection?.State ?? ConnectionState.Disconnected;
        public bool SafetyBypassStatus => Driver?.Profile?.SafetyBypass ?? false;

        public Direction CurrentDirection => Driver?.CurrentDirection ?? Direction.None;
        public Point JoystickPosition => Driver == null ? new Point() : new Point(Driver.DeviceStatus.XPosition, Driver.DeviceStatus.YPosition);
        public bool Switch1 => Driver?.DeviceStatus.Switch1 ?? false;
        public bool Switch2 => Driver?.DeviceStatus.Switch2 ?? false;
        public bool Switch3 => Driver?.DeviceStatus.Switch3 ?? false;

        public string Profile => Driver?.Profile.Name;

        public string Speed => Driver?.Profile.CurrentSpeed?.Name ?? Strings.StatusView_Speed_None;
        public ReadyState ReadyState => Driver?.ReadyState ?? ReadyState.None;
    }
}

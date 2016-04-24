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


using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

using Prism.Commands;

using Eyedrivomatic.ButtonDriver.Hardware;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    [Export]
    public class ArduinoConfigViewModel : DeviceConfigViewModel
    {
        [ImportingConstructor]
        public ArduinoConfigViewModel(IHardwareService hardwareServce)
            : base(hardwareServce)
        {
        }

        private string _deviceConnectionString;
        public override string SelectedDevice
        {
            get
            {
                return _deviceConnectionString;
            }
            set
            {
                if (_deviceConnectionString == value) return;

                _deviceConnectionString = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public override ICommand ApplyChanges => new DelegateCommand(
            () => HardwareService.CurrentDriver?.ConnectAsync(_deviceConnectionString),
            () => string.CompareOrdinal(_deviceConnectionString, HardwareService.CurrentDriver?.ConnectionString) != 0);

        public override ICommand Connect => new DelegateCommand(() => { HardwareService.CurrentDriver?.ConnectAsync(_deviceConnectionString); }, () => HardwareService.CurrentDriver?.IsConnected ?? false);
        public override ICommand Disconnect => new DelegateCommand(() => { HardwareService.CurrentDriver?.Disconnect(); }, () => HardwareService.CurrentDriver?.IsConnected ?? false);

        private void DriverPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(string.Empty);
        }
    }
}

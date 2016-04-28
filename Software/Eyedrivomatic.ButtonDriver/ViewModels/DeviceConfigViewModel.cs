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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Commands;
using Prism.Events;

using Eyedrivomatic.ButtonDriver.Hardware;
using Eyedrivomatic.Controls;
using Eyedrivomatic.Infrastructure.Events;

namespace Eyedrivomatic.ButtonDriver.ViewModels
{
    public abstract class DeviceConfigViewModel : ButtonDriverViewModelBase, IHeaderInfoProvider<string>
    {
        protected IEventAggregator EventAggregator { get; }

        public DeviceConfigViewModel(IHardwareService hardwareService, IEventAggregator eventAggregator)
            : base(hardwareService)
        {
            Contract.Requires<ArgumentNullException>(hardwareService != null, nameof(hardwareService));
            Contract.Requires<ArgumentNullException>(eventAggregator != null, nameof(eventAggregator));

            EventAggregator = eventAggregator;
            SaveCommand = new DelegateCommand(SaveChanges, CanSaveChanges);
            RefreshAvailableDeviceListCommand = new DelegateCommand(RefreshAvailableDeviceList, CanRefreshAvailableDeviceList);
            AutoDetectDeviceCommand = DelegateCommand.FromAsyncHandler(AutoDetectDeviceAsync, CanAutoDetectDevice);
            ConnectCommand = DelegateCommand.FromAsyncHandler(ConnectAsync, CanConnect);
            DisconnectCommand = new DelegateCommand(Disconnect, CanDisconnect);
        }

        public string HeaderInfo => Strings.ViewName_Setup;

        public ICommand RefreshAvailableDeviceListCommand { get; }

        public DelegateCommand AutoDetectDeviceCommand { get; }
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }

        public DelegateCommand SaveCommand { get; }

        public virtual bool Connecting => HardwareService.CurrentDriver?.IsConnecting ?? false;
        public virtual bool Connected => HardwareService.CurrentDriver?.IsConnected ?? false;
        public virtual bool Ready => HardwareService.CurrentDriver?.HardwareReady ?? false;

        public virtual IList<string> AvailableDevices => HardwareService.CurrentDriver?.GetAvailableDevices();
        public abstract string SelectedDevice { get; set; }

        private bool _autoConnect;
        public virtual bool AutoConnect
        {
            get { return _autoConnect; }
            set { SetProperty(ref _autoConnect, value); }
        }

        public virtual void RefreshAvailableDeviceList()
        {
            HardwareService.CurrentDriver?.GetAvailableDevices();
            OnPropertyChanged(nameof(AvailableDevices));

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }

        public virtual bool CanRefreshAvailableDeviceList()
        {
            return true;
        }

        protected virtual void SaveChanges()
        {
            EventAggregator.GetEvent<SaveAutoConnectEvent>().Publish(_autoConnect);
        }

        protected virtual bool CanSaveChanges() { return false; }

        protected abstract Task AutoDetectDeviceAsync();
        protected virtual bool CanAutoDetectDevice() { return !Connected && !Connecting; }

        protected abstract Task ConnectAsync();
        protected virtual bool CanConnect()
        {
            return !Connected && !Connecting;
        }

        protected abstract void Disconnect();
        protected virtual bool CanDisconnect()
        {
            return Connected;
        }

        protected override void OnDriverStatusChanged(object sender, EventArgs e)
        {
            base.OnDriverStatusChanged(sender, e);

            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            AutoDetectDeviceCommand.RaiseCanExecuteChanged();
        }
    }
}

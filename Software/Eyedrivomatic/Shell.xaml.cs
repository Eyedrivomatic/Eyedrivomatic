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
using System.ComponentModel.Composition;
using System.Windows.Input;
using Eyedrivomatic.ButtonDriver;
using Eyedrivomatic.Device;
using Eyedrivomatic.Infrastructure;
using Prism.Commands;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic
{
    [Export]
    public sealed partial class Shell : IDisposable
    {
        [Import]
        private IMouseVisibility MouseVisibilty { get; set; }

        public Shell()
        {
            InitializeComponent();
            DriveProfileSelection.Items.Clear();
            MainContent.Content = null;

            ConfigurationNavigation.Items.Clear();
            ConfigurationContent.Content = null;
        }

        [Export]
        public InteractionRequest<IConfirmation> ConfirmationRequest { get; } = new InteractionRequest<IConfirmation>();

        [Export]
        public InteractionRequest<INotification> NotificationRequest { get; } = new InteractionRequest<INotification>();

        [Export]
        public InteractionRequest<INotificationWithCustomButton> CustomNotificationRequest { get; } = new InteractionRequest<INotificationWithCustomButton>();

        [Export]
        public InteractionRequest<IConfirmationWithCustomButtons> CustomConfirmationRequest { get; } = new InteractionRequest<IConfirmationWithCustomButtons>();

        [Export("FirmwareUpdateProgress")]
        public InteractionRequest<IFirmwareUpdateProgressNotification> FirmwareUpdateProgress => ConnectionDecorator.FirmwareUpdateNotification;

        [Export(nameof(ShowDisclaimerCommand))]
        public ICommand ShowDisclaimerCommand => new DelegateCommand(() => CustomNotificationRequest.Raise(new DisclaimerNotification()));

        [Import]
        public IEventAggregator EventAggregator
        {
            set => ConnectionDecorator.SetConnectionEventSource(value.GetEvent<DeviceConnectionEvent>());
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                MouseVisibilty.OverrideMouseVisibility(!MouseVisibilty.IsMouseHidden);
            }
        }

        public void Dispose()
        {
        }
    }
}

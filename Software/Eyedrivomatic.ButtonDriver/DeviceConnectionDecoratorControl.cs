using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Eyedrivomatic.Device;
using Eyedrivomatic.Infrastructure.Annotations;
using NullGuard;
using Prism.Events;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic.ButtonDriver
{
    public class DeviceConnectionDecoratorControl : ContentControl, INotifyPropertyChanged, IDisposable
    {
        private SubscriptionToken _connectionEventToken;

        private static readonly DependencyPropertyKey FirmwareUpdateNotificationKey = DependencyProperty.RegisterReadOnly(
            nameof(FirmwareUpdateNotification), typeof(InteractionRequest<IFirmwareUpdateProgressNotification>), typeof(DeviceConnectionDecoratorControl), 
            new PropertyMetadata(new InteractionRequest<IFirmwareUpdateProgressNotification>()));
        public static readonly DependencyProperty FirmwareUpdateNotificationProperty = FirmwareUpdateNotificationKey.DependencyProperty;
        public InteractionRequest<IFirmwareUpdateProgressNotification> FirmwareUpdateNotification => (InteractionRequest<IFirmwareUpdateProgressNotification>) GetValue(FirmwareUpdateNotificationProperty);

        static DeviceConnectionDecoratorControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DeviceConnectionDecoratorControl), new FrameworkPropertyMetadata(typeof(DeviceConnectionDecoratorControl)));
        }

        public DeviceConnectionDecoratorControl()
        {
            FirmwareUpdateNotification.Raised += FirmwareUpdateProgressOnRaised;
        }

        public void SetConnectionEventSource(DeviceConnectionEvent connectionEvent)
        {
            _connectionEventToken?.Dispose();
            _connectionEventToken = connectionEvent.Subscribe(state => VisualStateManager.GoToState(this, state.ToString(), true));
        }

        private void FirmwareUpdateProgressOnRaised(object sender, InteractionRequestedEventArgs args)
        {
            FirmwareUpdateProgress = args.Context as IFirmwareUpdateProgressNotification;
            if (FirmwareUpdateProgress == null) return;

            void FirmwareProgressOnDone(object o, EventArgs eventArgs)
            {
                FirmwareUpdateProgress.OnDone -= FirmwareProgressOnDone;
                args.Callback();
            }

            FirmwareUpdateProgress.OnDone += FirmwareProgressOnDone;
            VisualStateManager.GoToState(this, "FirmwareDownload", true);
        }

        public static readonly DependencyProperty FirmwareUpdateProgressProperty = DependencyProperty.Register(
            nameof(FirmwareUpdateProgress), typeof(IFirmwareUpdateProgressNotification), typeof(DeviceConnectionDecoratorControl), new PropertyMetadata(default(IFirmwareUpdateProgressNotification)));

        [AllowNull]
        public IFirmwareUpdateProgressNotification FirmwareUpdateProgress
        {
            get => (IFirmwareUpdateProgressNotification) GetValue(FirmwareUpdateProgressProperty);
            set => SetValue(FirmwareUpdateProgressProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _connectionEventToken?.Dispose();
        }
    }
}

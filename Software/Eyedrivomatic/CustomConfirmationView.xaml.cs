using System;
using System.Windows;
using Eyedrivomatic.Infrastructure;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic
{
    public partial class CustomConfirmationView : IInteractionRequestAware
    {
        public CustomConfirmationView()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Notification is Confirmation confirmation) confirmation.Confirmed = true;
            FinishInteraction?.Invoke();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (Notification is Confirmation confirmation) confirmation.Confirmed = false;
            FinishInteraction?.Invoke();
        }

        public INotification Notification { get; set; }
        public Action FinishInteraction { get; set; }
    }
}

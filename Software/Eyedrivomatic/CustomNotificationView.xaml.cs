using System;
using System.Windows;
using Prism.Interactivity.InteractionRequest;

namespace Eyedrivomatic
{
    public partial class CustomNotificationView : IInteractionRequestAware
    {
        public CustomNotificationView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishInteraction?.Invoke();
        }

        public INotification Notification { get; set; }
        public Action FinishInteraction { get; set; }
    }
}

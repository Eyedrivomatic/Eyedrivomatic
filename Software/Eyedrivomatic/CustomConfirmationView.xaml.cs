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
using System.Windows;
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

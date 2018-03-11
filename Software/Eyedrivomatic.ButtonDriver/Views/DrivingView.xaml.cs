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


using Eyedrivomatic.ButtonDriver.ViewModels;
using System.ComponentModel.Composition;

namespace Eyedrivomatic.ButtonDriver.Views
{
    [Export]
    public partial class DrivingView
    {
        public DrivingView()
        {
            InitializeComponent();
        }

        [Import]
        public IDrivingViewModel ViewModel
        {
            get => (IDrivingViewModel)DataContext;
            set => DataContext = value;
        }
    }
}

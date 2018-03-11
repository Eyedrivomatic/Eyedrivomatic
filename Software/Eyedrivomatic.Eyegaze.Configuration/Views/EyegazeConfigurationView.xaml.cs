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


using System.ComponentModel.Composition;
using Eyedrivomatic.Eyegaze.Configuration.ViewModels;

namespace Eyedrivomatic.Eyegaze.Configuration.Views
{
    [Export]
    public partial class EyegazeConfigurationView
    {
        public EyegazeConfigurationView()
        {
            InitializeComponent();
        }

        [Import]
        public EyegazeConfigruationViewModel ViewModel
        {
            get => (EyegazeConfigruationViewModel)DataContext;
            set => DataContext = value;
        }
    }
}

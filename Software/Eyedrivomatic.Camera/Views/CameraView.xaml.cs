﻿//	Copyright (c) 2018 Eyedrivomatic Authors
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
using System.Windows;
using Eyedrivomatic.Camera.ViewModels;

namespace Eyedrivomatic.Camera.Views
{
    [Export]
    public partial class CameraView
    {
        public CameraView()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            StartOrStopCapture();
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            StartOrStopCapture();
        }

        [Import]
        public CameraViewModel ViewModel
        {
            get => (CameraViewModel)DataContext;
            set => DataContext = value;
        }

        private void StartOrStopCapture()
        {
            if (IsVisible && IsLoaded)
            {
                ViewModel.StartCapture();
            }
            else
            {
                ViewModel.StopCapture();
            }
        }
    }
}

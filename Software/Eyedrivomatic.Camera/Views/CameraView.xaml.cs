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

using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Tobii.Interaction;
using Tobii.Interaction.Framework;
using Tobii.Interaction.Wpf;

namespace Eyedrivomatic.Eyegaze.Interfaces.Tobii
{
    [ExportEyegazeProvider("Tobii"), PartCreationPolicy(CreationPolicy.Shared)]
    public class TobiiEyegazeProvider : IEyegazeProvider
    {
        private class TobiiProviderRegistration : IDisposable
        {
            private readonly WpfInteractor _interactor;
            private readonly IEyegazeClient _client;
            //private readonly FixationDataStream _stream;
            private readonly Dispatcher _dispatcher;

            public TobiiProviderRegistration(WpfInteractor interactor, IEyegazeClient client)
            {
                _interactor = interactor;
                _interactor.WithGazeAware().HasGaze(HasGaze).LostGaze(LostGaze).Mode = GazeAwareMode.Normal;
                
                _interactor.GetGazePointDataStream().GazePoint(GazePoint);
                
                interactor.Element.MouseDown += ElementOnMouseDown;

                //_stream.Begin(TobiiEnterHandler)
                //       .Data(TobiiMoveHandler)
                //       .End(TobiiLeaveHandler);

                _client = client;
                _dispatcher = Dispatcher.CurrentDispatcher;
            }

            private void GazePoint(double x, double y, double timestamp)
            {
                _dispatcher.InvokeAsync(() =>
                {
                    if (!_hasGaze) return;
                    _client.GazeContinue();
                });
            }

            private bool _hasGaze;
            private void LostGaze()
            {
                _dispatcher.InvokeAsync(() =>
                {
                    if (!_hasGaze) return;
                    _hasGaze = false;
                    _client.GazeLeave();
                });
            }

            private void HasGaze()
            {
                _dispatcher.InvokeAsync(() =>
                {
                    if (_hasGaze) return;
                    _hasGaze = true;
                    _client.GazeEnter();
                });
            }


            private void ElementOnMouseDown(object sender, MouseButtonEventArgs e)
            {
                    _client.ManualActivation();
                    e.Handled = true;
            }

            private void TobiiEnterHandler(double x, double y, double timestamp)
            {
                _interactor.InvokeOnElementAsync(() => _client.GazeEnter());
            }

            private void TobiiMoveHandler(double x, double y, double timestamp)
            {
                _interactor.InvokeOnElementAsync(() => _client.GazeContinue());
            }

            private void TobiiLeaveHandler(double x, double y, double timestamp)
            {
                _interactor.InvokeOnElementAsync(() => _client.GazeLeave());
            }

            public void Dispose()
            {
                _interactor.SetIsEnabled(false);
            }
        }

        private readonly Host _host;
        private readonly WpfInteractorAgent _interactorAgent;

        public TobiiEyegazeProvider()
        {
            _host = new Host();
            _interactorAgent = _host.InitializeWpfAgent();
        }

        public IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client)
        {
            var interactor = _interactorAgent.AddInteractorFor(element);

            return new TobiiProviderRegistration(interactor, client);
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}

using System;
using System.ComponentModel.Composition;
using System.Windows;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Eyegaze.Interfaces.Mouse
{
    [ExportEyegazeProvider("Mouse"), PartCreationPolicy(CreationPolicy.Shared)]
    public partial class MouseEyegazeProvider : IEyegazeProvider
    {
        //[Import(nameof(RequiredMouseMoves))]
        public static int RequiredMouseMoves = 2; //the number of mouse moves that need to happen before a click can begin.

        //[Import(nameof(MouseMoveWatchdogTime))]
        public static TimeSpan MouseMoveWatchdogTime = TimeSpan.FromMilliseconds(400);

        public bool Initialize()
        {
            return true;
        }

        public IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client)
        {
            return new MouseProviderRegistration(element, client);
        }

        public void Dispose()
        {
            Log.Info(this, "Disposing Mouse Eyegaze provider.");
        }
    }
}

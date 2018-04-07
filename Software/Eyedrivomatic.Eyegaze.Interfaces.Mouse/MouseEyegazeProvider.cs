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
using System.ComponentModel.Composition;
using System.Threading.Tasks;
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

        public Task<bool> InitializeAsync()
        {
            return Task.FromResult(true);
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

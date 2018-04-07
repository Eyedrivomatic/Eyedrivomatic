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
using System.Threading.Tasks;
using System.Windows;

namespace Eyedrivomatic.Eyegaze
{
    public interface IEyegazeProvider : IDisposable
    {
        Task<bool> InitializeAsync();
        IDisposable RegisterElement(FrameworkElement element, IEyegazeClient client);
    }

    public interface IEyegazeClient
    {
        void ManualActivation();

        void GazeEnter();

        void GazeContinue();

        void GazeLeave();
    }
}
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

namespace Eyedrivomatic.Hardware.Commands
{
    public interface IDeviceCommand
    {
        /// <summary>
        /// A name that is used to describe the command.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The number of times the command should be sent before failing.
        /// Includes the first attempt.
        /// Use 0 to retry indefinately.
        /// </summary>
        int MaxAttempts { get; }

        /// <summary>
        /// The timeout period for the command.
        /// This time should be greater than the connections send timeout and should account for other potential commands in the queue.
        /// </summary>
        TimeSpan DefaultTimeout { get; }

        /// <summary>
        /// Returns the command text as it is sent to the device.
        /// </summary>
        string ToString();

    }
}
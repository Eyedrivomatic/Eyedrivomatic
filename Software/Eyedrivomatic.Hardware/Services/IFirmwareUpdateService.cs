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
using System.Collections.Generic;
using System.Threading.Tasks;
using Eyedrivomatic.Hardware.Communications;
using NullGuard;

namespace Eyedrivomatic.Hardware.Services
{
    public interface IFirmwareUpdateService
    {
        IEnumerable<Version> GetAvailableFirmware();

        [return:AllowNull]
        Version GetLatestVersion();
        Task<bool> UpdateFirmwareAsync(IDeviceConnection connection, Version version, bool required, IProgress<double> progress = null);
    }
}
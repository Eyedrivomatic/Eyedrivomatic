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


namespace Eyedrivomatic.Eyegaze.Configuration.DwellClick
{
    internal sealed partial class DwellClickConfiguration
    {
        public override void Upgrade()
        {
            if (SettingsVersion > 0) return; //Already upgraded.

            base.Upgrade();

            if (SettingsVersion == 0) //Eyedrivomatic 1.0
            {
                if (GetPreviousVersion("DwellTimeMilliseconds") is int dwellTime)
                {
                    StandardDwellTimeMilliseconds = dwellTime;
                    DirectionButtonDwellTimeMilliseconds = dwellTime;
                    StartButtonDwellTimeMilliseconds = dwellTime;
                    StopButtonDwellTimeMilliseconds = dwellTime;
                }
            }

            SettingsVersion = 1;
        }
    }
}

using System.Linq;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    internal sealed partial class ButtonDriverConfiguration
    {
        public override void Upgrade()
        {
            if (SettingsVersion > 0) return; //Already upgraded.

            base.Upgrade();
            
            if (SettingsVersion == 0) //Eyedrivomatic 1.0
            {
                //Populate the driving profiles if none are defined.
                if (!(DrivingProfiles?.Any() ?? false))
                {
                    var defaultProfile = new Profile();
                    defaultProfile.AddDefaultSpeeds();
                    DrivingProfiles = new ProfileCollection { defaultProfile };
                }
            }

            SettingsVersion = 1;
        }
    }
    
}
using System.Configuration;
using System.Linq;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    internal sealed partial class ButtonDriverConfiguration
    {
        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            //Populate the driving profiles if none are defined.
            if (!(DrivingProfiles?.Any() ?? false))
            {
                var defaultProfile = new Profile();
                defaultProfile.AddDefaultSpeeds();
                DrivingProfiles = new ProfileCollection { defaultProfile  };
                base.OnSettingsLoaded(sender, e);
            }
        }
    }
}
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using Eyedrivomatic.Resources;

namespace Eyedrivomatic.ButtonDriver.Configuration
{
    internal sealed partial class ButtonDriverConfiguration
    {
        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            //Populate the driving profiles if none are defined.
            if (!(DrivingProfiles?.Any() ?? false))
            {
                var defaultProfile = new Profile() {Name = "Driving"};
                defaultProfile.Speeds.AddRange(
                        new []
                        {
                            //Previous version = {Name="Slow",                           X=22, YForward=9, YBackward=9, XDiag=14, YForwardDiag=6, YBackwardDiag=6, XDiagReduced=4, YForwardDiagReduced=6, Nudge=6},
                            new ProfileSpeed {Name=Strings.DrivingView_SpeedSlow, X=22, YForward=9, YBackward=9, XDiag=14, YForwardDiag=6, YBackwardDiag=6, XDiagReduced=4, YForwardDiagReduced=6, Nudge=6},

                            //Previous version = {Name="Walk",                           X=22, YForward=13, YBackward=13, XDiag=15, YForwardDiag=10, YBackwardDiag=10, XDiagReduced=5, YForwardDiagReduced=10, Nudge=6},
                            new ProfileSpeed {Name=Strings.DrivingView_SpeedWalk, X=22, YForward=13, YBackward=13, XDiag=15, YForwardDiag=10, YBackwardDiag=10, XDiagReduced=5, YForwardDiagReduced=10, Nudge=6},

                            //Previous version = {Name="Fast",                           X=22, YForward=17, YBackward=17, XDiag=17, YForwardDiag=14, YBackwardDiag=14, XDiagReduced=7, YForwardDiagReduced=14, Nudge=6},
                            new ProfileSpeed {Name=Strings.DrivingView_SpeedFast, X=22, YForward=17, YBackward=17, XDiag=17, YForwardDiag=14, YBackwardDiag=14, XDiagReduced=7, YForwardDiagReduced=14, Nudge=6},

                            //Previous version = {Name="Manic",                          X=22, YForward=21, YBackward=21, XDiag=22, YForwardDiag=18, YBackwardDiag=18, XDiagReduced=12, YForwardDiagReduced=18, Nudge=6},
                            new ProfileSpeed {Name=Strings.DrivingView_SpeedManic, X=22, YForward=21, YBackward=21, XDiag=22, YForwardDiag=18, YBackwardDiag=18, XDiagReduced=12, YForwardDiagReduced=18, Nudge=6},
                        });

                DrivingProfiles = new ProfileCollection { defaultProfile  };
                base.OnSettingsLoaded(sender, e);
            }
        }
    }
}
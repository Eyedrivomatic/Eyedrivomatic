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

namespace Eyedrivomatic.Configuration
{
    internal sealed partial class AppearanceConfiguration
    {
        public override void Upgrade()
        {
            if (SettingsVersion > 0) return; //Already upgraded.

            base.Upgrade();

            SettingsVersion = 1;
        }
    }
}
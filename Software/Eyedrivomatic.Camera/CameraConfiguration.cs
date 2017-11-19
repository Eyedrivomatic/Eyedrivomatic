namespace Eyedrivomatic.Camera
{
    internal sealed partial class CameraConfiguration
    {
        public override void Upgrade()
        {
            if (SettingsVersion > 0) return; //Already upgraded.
    
            base.Upgrade();

            SettingsVersion = 1;
        }
    }
}
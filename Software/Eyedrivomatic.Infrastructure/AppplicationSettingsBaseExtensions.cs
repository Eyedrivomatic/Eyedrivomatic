using System.Configuration;
using System.Linq;
using Eyedrivomatic.Logging;

namespace Eyedrivomatic.Infrastructure
{
    public static class AppplicationSettingsBaseExtensions
    {
        public static void WriteToLog(this ApplicationSettingsBase settings)
        {
            Log.Debug(settings, $"Loaded {settings.SettingsKey} configuration.");
            foreach (var prop in settings.PropertyValues.Cast<SettingsPropertyValue>())
            {
                Log.Debug(settings, $"{prop.Name}: {prop.PropertyValue} {(prop.UsingDefaultValue ? " *" : "")}");
            }
        }

    }
}

using System.Windows;

namespace Eyedrivomatic.Infrastructure
{
    public static class FrameworkElementExtensions
    {
        public static string LoggingToString(this FrameworkElement element)
        {
            if (element == null) return "NULL";

            if (!string.IsNullOrEmpty(element.Name)) return element.Name;
            return element.ToString();
        }
    }
}

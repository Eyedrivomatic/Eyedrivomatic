using System.Windows;

namespace Eyedrivomatic.Common.UI
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

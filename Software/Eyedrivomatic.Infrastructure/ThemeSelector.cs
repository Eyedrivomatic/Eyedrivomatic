using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using NullGuard;

namespace Eyedrivomatic.Infrastructure
{
    [Export]
    public class ThemeSelector : DependencyObject
    {
        private readonly Application _application;

        public ThemeSelector()
        {
            _application = Application.Current;
        }

        public ThemeSelector(Application application)
        {
            _application = application;
        }

        public void ApplyTheme([AllowNull] ThemeResourceDictionary theme)
        {
            var dictionaries = _application.Resources.MergedDictionaries;
            var prevThemes = dictionaries.OfType<ThemeResourceDictionary>().ToList();
            if (theme != null) dictionaries.Insert(0, theme);
            foreach (var prevTheme in prevThemes) dictionaries.Remove(prevTheme);
        }
    }
}

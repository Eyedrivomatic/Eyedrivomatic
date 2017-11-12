using System.ComponentModel;
using System.Globalization;
using Gu.Localization;

namespace Eyedrivomatic.Infrastructure
{
    /// <summary>
    /// A wrapper around the translation to implement ToString to return the translated text.
    /// </summary>
    public class TranslationWrapper : ITranslation
    {
        private readonly ITranslation _translationImplementation;

        public TranslationWrapper(ITranslation translationImplementation)
        {
            _translationImplementation = translationImplementation;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _translationImplementation.PropertyChanged += value;
            remove => _translationImplementation.PropertyChanged -= value;
        }

        public string Translate(CultureInfo culture, ErrorHandling errorHandlingStrategy = ErrorHandling.Inherit)
        {
            return _translationImplementation.Translate(culture, errorHandlingStrategy);
        }

        public string Translated => _translationImplementation.Translated;

        public string Key => _translationImplementation.Key;

        public ErrorHandling ErrorHandling => _translationImplementation.ErrorHandling;

        public override string ToString()
        {
            return Translated;
        }
    }
}
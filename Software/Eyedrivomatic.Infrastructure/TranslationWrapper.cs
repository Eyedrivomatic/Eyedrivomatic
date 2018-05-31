//	Copyright (c) 2018 Eyedrivomatic Authors
//	
//	This file is part of the 'Eyedrivomatic' PC application.
//	
//	This program is intended for use as part of the 'Eyedrivomatic System' for 
//	controlling an electric wheelchair using soley the user's eyes. 
//	
//	Eyedrivomaticis distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  


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
            _translationImplementation.PropertyChanged += (sender, args) => PropertyChanged?.Invoke(this, args);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Translate(CultureInfo culture, ErrorHandling errorHandlingStrategy = ErrorHandling.Inherit)
        {
            return _translationImplementation.Translate(culture, errorHandlingStrategy);
        }

        public string Translated => _translationImplementation.Translated;

        public string Key => _translationImplementation.Key;

        public ErrorHandling ErrorHandling => _translationImplementation.ErrorHandling;

        public override string ToString()
        {
            try
            {
                return Translated;
            }
            catch (System.InvalidOperationException)
            {
                return Key;
            }
        }
    }
}
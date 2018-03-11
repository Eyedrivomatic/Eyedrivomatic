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


using System;
using System.Resources;
using Eyedrivomatic.Logging;
using Gu.Localization;
using Gu.Localization.Properties;

namespace Eyedrivomatic.Infrastructure
{
    public static class Translate
    {
        public static ResourceManager ResourceManager { get; set; } = Resources.ResourceManager;

        /// <summary>Call like this: Translate.Key(nameof(Strings.Saved_file__0_)).</summary>
        /// <param name="key">A key in Properties.Resources</param>
        /// <param name="errorHandling">How to handle translation errors like missing key or culture.</param>
        /// <returns>A translation for the key.</returns>
        public static string Key(string key, ErrorHandling errorHandling = ErrorHandling.ReturnErrorInfoPreserveNeutral)
        {
            return TranslationFor(key, errorHandling).Translated;
        }

        /// <summary>Call like this: Translate.Key(nameof(Strings.Saved_file__0_)).</summary>
        /// <param name="key">A key in Properties.Resources</param>
        /// <param name="defaultValue">The value to use if the translation does not exist</param>
        /// <returns>A translation for the key.</returns>
        public static string Key(string key, string defaultValue)
        {
            return TranslationFor(key, defaultValue).Translated;
        }

        /// <summary>Call like this: Translate.Key(nameof(Strings.Saved_file__0_)).</summary>
        /// <param name="key">A key in Properties.Resources</param>
        /// <param name="errorHandling">How to handle translation errors like missing key or culture.</param>
        /// <returns>A translation for the key.</returns>
        public static ITranslation TranslationFor(string key, ErrorHandling errorHandling = ErrorHandling.ReturnErrorInfoPreserveNeutral)
        {
            return new TranslationWrapper(Translation.GetOrCreate(ResourceManager, key, errorHandling));
        }

        /// <summary>Call like this: Translate.Key(nameof(Strings.Saved_file__0_)).</summary>
        /// <param name="key">A key in Properties.Resources</param>
        /// <param name="defaultValue">The value to use if the translation does not exist</param>
        /// <returns>A translation for the key or the defaultValue if none exists.</returns>
        public static ITranslation TranslationFor(string key, string defaultValue)
        {
            try
            {
                var translation = Translation.GetOrCreate(ResourceManager, key, ErrorHandling.ReturnErrorInfoPreserveNeutral);
                if (translation is StaticTranslation) return new TranslationWrapper(new StaticTranslation(defaultValue));//error info found.
                return new TranslationWrapper(translation);
            }
            catch (Exception e)
            {
                Log.Warn(nameof(Translate), $"Failed to find translation for [{key}]. [{e}]");
                return new StaticTranslation(defaultValue);
            }
        }
    }
}
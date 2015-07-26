using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebsiteRipper.Core
{
    static class Tools
    {
        static IEnumerable<string> GetAllLanguages(CultureInfo language)
        {
            while (!string.IsNullOrEmpty(language.Name))
            {
                yield return language.Name;
                language = language.Parent;
            }
        }

        public static string GetPreferredLanguages(CultureInfo language)
        {
            return GetPreferredLanguages(GetAllLanguages(language));
        }

        public static string GetPreferredLanguages(IEnumerable<CultureInfo> languages)
        {
            return GetPreferredLanguages(languages.SelectMany(GetAllLanguages));
        }

        static string GetPreferredLanguages(IEnumerable<string> languages)
        {
            var allLanguages = languages.ToList();
            var qualityDecrement = 1.0 / (allLanguages.Count + 1);
            var preferredLanguages = string.Join(",", allLanguages.Select((language, number) =>
            {
                var quality = 1.0 - number * qualityDecrement;
                return string.Format(number == 0 ? "{0}" : "{0};q={1}", language, Math.Round(quality, 3).ToString(CultureInfo.InvariantCulture));
            }));
            return preferredLanguages;
        }
    }
}

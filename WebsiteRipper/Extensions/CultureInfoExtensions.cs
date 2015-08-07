using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebsiteRipper.Extensions
{
    static class CultureInfoExtensions
    {
        static IEnumerable<string> GetAllLanguages(this CultureInfo language)
        {
            while (!string.IsNullOrEmpty(language.Name))
            {
                yield return language.Name;
                language = language.Parent;
            }
        }

        public static string GetPreferredLanguages(this CultureInfo language)
        {
            return language.GetAllLanguages().GetPreferredLanguages();
        }

        public static string GetPreferredLanguages(this IEnumerable<CultureInfo> languages)
        {
            return languages.SelectMany(GetAllLanguages).GetPreferredLanguages();
        }

        static string GetPreferredLanguages(this IEnumerable<string> languages)
        {
            const string preferredLanguagesFormat = "{0};q={1}";
            var allLanguages = languages.ToList();
            var qualityDecrement = 1.0 / (allLanguages.Count + 1);
            var preferredLanguages = string.Join(",", allLanguages.Select((language, number) =>
            {
                var quality = 1.0 - number * qualityDecrement;
                return number == 0 ? language : string.Format(preferredLanguagesFormat, language, Math.Round(quality, 3).ToString(CultureInfo.InvariantCulture));
            }));
            return preferredLanguages;
        }
    }
}

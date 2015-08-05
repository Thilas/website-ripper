using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebsiteRipper.Core
{
    static class Tools
    {
        public static int CombineHashCodes(int hashCode1, int hashCode2)
        {
            return (hashCode1 << 5) + hashCode1 ^ hashCode2;
        }

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

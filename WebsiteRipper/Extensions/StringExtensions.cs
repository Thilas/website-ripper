using System;

namespace WebsiteRipper.Extensions
{
    static class StringExtensions
    {
        const string Ellipsis = "…";

        public static string MiddleTruncate(this string value, int totalWidth, string ellipsis = Ellipsis)
        {
            if (value == null) throw new ArgumentNullException("value");
            ellipsis = ellipsis ?? string.Empty;
            // Check than totalWidth is greater or equal ellipsis length plus one leading character and one trailing character
            if (totalWidth < ellipsis.Length + 2) throw new ArgumentOutOfRangeException("totalWidth");
            if (value.Length <= totalWidth) return value;
            var count = value.Length - totalWidth + ellipsis.Length;
            var start = (value.Length - count) / 2;
            value = value.Remove(start, count);
            value = value.Insert(start, ellipsis);
            return value;
        }
    }
}

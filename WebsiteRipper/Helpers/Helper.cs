using System;
using System.Linq;

namespace WebsiteRipper.Helpers
{
    static class Helper
    {
        public static int CombineHashCodes(int hashCode1, int hashCode2)
        {
            return (hashCode1 << 5) + hashCode1 ^ hashCode2;
        }

        readonly static char[] _spaceCharacters = { ' ', '\t', '\n', '\f', '\r' };

        public static string[] SplitSpaceSeparatedTokens(string list)
        {
            return list.Split(_spaceCharacters, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] SplitCommaSeparatedTokens(string list)
        {
            return list.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(token => token.Trim(_spaceCharacters))
                .Where(token => !string.IsNullOrEmpty(token))
                .ToArray();
        }
    }
}

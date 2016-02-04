using System;

namespace Msiler.Helpers
{
    public static class StringHelpers
    {
        public static string ReplaceNewLineCharacters(string str) {
            return str.Replace("\n", @"\n").Replace("\r", @"\r");
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source.IndexOf(toCheck, comp) >= 0;
        }
    }
}

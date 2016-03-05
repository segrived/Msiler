using System;
using System.Collections.Generic;
using System.Text;

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

        public static string[] Lines(this string source) {
            return source.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        public static long? ParseNumber(string s) {
            try {
                int numberBase = 10;
                if (s.StartsWith("0x", StringComparison.Ordinal)) {
                    s = s.Replace("0x", "");
                    numberBase = 16;
                }
                long number = Convert.ToInt64(s, numberBase);
                return number;
            } catch (Exception) {
                return null;
            }
        }

        public static string JoinStrings(this IEnumerable<string> strColl) {
            var sb = new StringBuilder();
            foreach (string s in strColl) {
                sb.AppendLine(s);
            }
            return sb.ToString();
        }
    }
}

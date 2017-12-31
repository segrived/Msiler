using System;

namespace Msiler.Helpers
{
    public static class StringHelpers
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
            => source.IndexOf(toCheck, comp) >= 0;

        public static string[] Lines(this string source)
            => source.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        public static long? ParseNumber(string s)
        {
            try
            {
                return !s.StartsWith("0x", StringComparison.Ordinal) 
                    ? Convert.ToInt64(s, 10) 
                    : Convert.ToInt64(s.Replace("0x", ""), 16);
            } 
            catch (Exception)
            {
                return null;
            }
        }
    }
}

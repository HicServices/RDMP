using System.Globalization;
using System.Text.RegularExpressions;

namespace ReusableLibraryCode.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string haystack,string needle, CompareOptions compareOptions)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, CompareOptions.IgnoreCase) >= 0;
        }

        public static string Replace(this string haystack, string needle, string replacement, RegexOptions options)
        {
            string result = Regex.Replace(
                haystack,
                Regex.Escape(needle),
                replacement.Replace("$", "$$"),
                options
            );
            return result;
        }
    }
}
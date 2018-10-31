using System;
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

        /// <summary>
        /// Returns true if s is null, whitespace or the text 'null' (ignoring trimm and case)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsBasicallyNull(this string s)
        {
            return string.IsNullOrWhiteSpace(s) || s.Trim().Equals("NULL", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
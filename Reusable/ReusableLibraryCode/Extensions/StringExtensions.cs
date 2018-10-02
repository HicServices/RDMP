using System.Globalization;

namespace ReusableLibraryCode.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string haystack,string needle, CompareOptions compareOptions)
        {
            return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, CompareOptions.IgnoreCase) >= 0;
        }
    }
}
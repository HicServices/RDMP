// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Rdmp.Core.CommandLine.Interactive
{
    static class AutoComplete
    {
        public static List<string> GetAutoCompletePossibilities(string input, List<string> strings, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(input) || strings == null || strings.Count == 0)
                return new List<string>(0);

            return strings.Where(s => s.StartsWith(input, ignoreCase, CultureInfo.InvariantCulture)).ToList();
        }

        public static string GetComplimentaryAutoComplete(string input, List<string> strings, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var possibilities = GetAutoCompletePossibilities(input, strings, ignoreCase);
            if (possibilities.Count == 0)
                return input;
            var leadingString = GetEqualLeadingString(possibilities, ignoreCase);
            if (leadingString.Length == input.Length)
                return input;
            return leadingString;
        }

        public static string GetEqualLeadingString(List<string> strings, bool ignoreCase = true)
        {
            if (strings == null || strings.Count == 0)
                return string.Empty;
            if (strings.Count == 1)
                return strings[0];

            var baseString = strings[0];
            var index = 0;
            var result = string.Empty;
            while (index < baseString.Length)
            {
                var currentChar = baseString[index];
                for (var i = 1; i < strings.Count; i++)
                {
                    var otherChar = strings[i][index];
                    if (ignoreCase)
                    {
                        if (char.ToUpperInvariant(otherChar) != char.ToUpperInvariant(currentChar))
                            return result;
                    }
                    else if(otherChar != currentChar)
                        return result;
                }
                result += currentChar;
                index++;
            }
            return result;
        }
    }
}

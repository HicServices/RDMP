// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive
{
    class AutoComplete
    {
        private readonly string[] autocompletes;

        public AutoComplete(IEnumerable<string> autocompletes)
        {
            this.autocompletes = autocompletes?.ToArray() ?? new string[0];
        }

        public char[] Separators { get;set;} = new []{ ','};

        public string[] GetSuggestions(string text, int index)
        {
            //they haven't typed anything yet
            if(string.IsNullOrWhiteSpace(text))
                return autocompletes;

            return autocompletes.Where(a=>a.StartsWith(text)).ToArray();
        }
    }
}

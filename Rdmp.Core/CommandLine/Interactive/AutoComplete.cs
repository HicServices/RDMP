// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 1591

namespace Rdmp.Core.CommandLine.Interactive;

internal class AutoComplete
{
    private readonly string[] _autocompletes;

    public AutoComplete(IEnumerable<string> autocompletes)
    {
        _autocompletes = autocompletes?.ToArray() ?? Array.Empty<string>();
    }

    public char[] Separators { get;set;} = new []{ ','};

    public string[] GetSuggestions(string text, int index)
    {
        //they haven't typed anything yet
        if(string.IsNullOrWhiteSpace(text))
            return _autocompletes;

        return _autocompletes.Where(a=>a.StartsWith(text)).ToArray();
    }
}
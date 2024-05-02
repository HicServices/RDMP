// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

public abstract class PickObjectBase
{
    public abstract string Format { get; }
    public abstract string Help { get; }
    public abstract IEnumerable<string> Examples { get; }

    protected Regex Regex { get; }
    protected readonly IBasicActivateItems Activator;

    public virtual bool IsMatch(string arg, int idx)
    {
        return Regex.IsMatch(arg);
    }

    public abstract CommandLineObjectPickerArgumentValue Parse(string arg, int idx);


    /// <summary>
    ///     Runs the <see cref="Regex" /> on the provided <paramref name="arg" /> throwing an
    ///     <see cref="InvalidOperationException" />
    ///     if the match is a failure.
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="idx"></param>
    /// <returns></returns>
    protected Match MatchOrThrow(string arg, int idx)
    {
        var match = Regex.Match(arg);

        return !match.Success
            ? throw new InvalidOperationException("Regex did not match, no value could be parsed")
            : match;
    }

    public PickObjectBase(IBasicActivateItems activator, Regex regex)
    {
        Regex = regex;
        Activator = activator;
    }

    protected static Type ParseDatabaseEntityType(string objectType, string arg, int idx)
    {
        var t = (GetTypeFromShortCodeIfAny(objectType) ?? MEF.GetType(objectType)) ??
                throw new CommandLineObjectPickerParseException("Could not recognize Type name", idx, arg);
        return !typeof(DatabaseEntity).IsAssignableFrom(t)
            ? throw new CommandLineObjectPickerParseException("Type specified must be a DatabaseEntity", idx, arg)
            : t;
    }

    /// <summary>
    ///     Returns true if <paramref name="possibleTypeName" /> is a Type name or shortcode for an
    ///     <see cref="IMapsDirectlyToDatabaseTable" />
    ///     object.  The <see cref="Type" /> is also out via <paramref name="t" /> (or null)
    /// </summary>
    /// <param name="possibleTypeName"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    protected static bool IsDatabaseObjectType(string possibleTypeName, out Type t)
    {
        try
        {
            t = GetTypeFromShortCodeIfAny(possibleTypeName) ?? MEF.GetType(possibleTypeName);
        }
        catch (Exception)
        {
            t = null;
            return false;
        }

        return t != null
               && typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(t);
    }

    private static Type GetTypeFromShortCodeIfAny(string possibleShortCode)
    {
        return SearchablesMatchScorer.ShortCodes.TryGetValue(possibleShortCode, out var code) ? code : null;
    }

    protected IMapsDirectlyToDatabaseTable GetObjectByID(Type type, int id)
    {
        var repo = Activator.GetRepositoryFor(type);
        return repo.GetObjectByID(type, id);
    }


    protected IEnumerable<IMapsDirectlyToDatabaseTable> GetAllObjects(Type type)
    {
        var repo = Activator.GetRepositoryFor(type);
        return repo.GetAllObjects(type);
    }

    private readonly Dictionary<string, Regex> patternDictionary = new();

    /// <summary>
    ///     Returns true if the <paramref name="pattern" /> (which is a simple non regex e.g. "Bio*") matches the ToString of
    ///     <paramref name="o" />
    /// </summary>
    /// <param name="o"></param>
    /// <param name="pattern"></param>
    /// <returns></returns>
    protected bool FilterByPattern(object o, string pattern)
    {
        //build regex for the pattern which must be a complete match with anything (.*) matching the users wildcard
        if (!patternDictionary.ContainsKey(pattern))
            patternDictionary.Add(pattern,
                new Regex($"^{Regex.Escape(pattern).Replace(@"\*", ".*")}$", RegexOptions.IgnoreCase));

        return patternDictionary[pattern].IsMatch(o.ToString());
    }

    /// <summary>
    ///     Takes a key value pair in a string e.g. "Schema:dbo" and returns the substring "dbo".  Trims leading and trailing
    ///     ':'.  Returns null if <paramref name="keyValueString" /> is null
    /// </summary>
    /// <param name="key"></param>
    /// <param name="keyValueString"></param>
    /// <returns></returns>
    protected static string Trim(string key, string keyValueString)
    {
        if (string.IsNullOrWhiteSpace(keyValueString))
            return null;

        return !keyValueString.StartsWith(key, StringComparison.CurrentCultureIgnoreCase)
            ? throw new ArgumentException($"Provided value '{keyValueString}' did not start with expected key '{key}'")
            : keyValueString[key.Length..].Trim(':');
    }

    public virtual IEnumerable<string> GetAutoCompleteIfAny()
    {
        return null;
    }
}
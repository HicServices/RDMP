// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

/// <summary>
///     Determines if a command line argument provided was a reference to one or more <see cref="DatabaseEntity" />
///     matching based on name (e.g. "Catalogue:my*cata")
/// </summary>
public partial class PickObjectByName : PickObjectBase
{
    public override string Format => "{Type}:{NamePattern}[,{NamePattern2},{NamePattern3}...]";

    public override string Help =>
        @"Type: must be an RDMP object type e.g. Catalogue, Project etc.
NamePattern: must be a string that matches 1 (or more if selecting multiple objects) object based on its name (ToString).  Can include the wild card '*'.  Cannot include the ':' character.
NamePattern2+: (optional) only allowed if you are being prompted for multiple objects, allows you to specify multiple objects of the same Type using comma separator";

    public override IEnumerable<string> Examples => new[]
    {
        "Catalogue:mycata*",
        "Catalogue:mycata1,mycata2"
    };

    public PickObjectByName(IBasicActivateItems activator) :
        base(activator,
            HasId())
    {
    }

    public override bool IsMatch(string arg, int idx)
    {
        var baseMatch = base.IsMatch(arg, idx);

        if (!string.IsNullOrWhiteSpace(arg))
            if (IsDatabaseObjectType(arg, out _))
                return true;

        //only considered  match if the first letter is an Rdmp Type e.g. "Catalogue:fish" but not "C:\fish"
        return baseMatch && IsDatabaseObjectType(Regex.Match(arg).Groups[1].Value, out _);
    }

    public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
    {
        if (IsDatabaseObjectType(arg, out var t))
            return new CommandLineObjectPickerArgumentValue(arg, idx, GetAllObjects(t).ToArray());

        var objByToString = MatchOrThrow(arg, idx);

        var objectType = objByToString.Groups[1].Value;
        var objectToString = objByToString.Groups[2].Value;

        var dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);

        var objs = objectToString.Split(',').SelectMany(str => GetObjectByToString(dbObjectType, str)).Distinct();
        return new CommandLineObjectPickerArgumentValue(arg, idx, objs.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
    }

    private IEnumerable<object> GetObjectByToString(Type dbObjectType, string str)
    {
        return GetAllObjects(dbObjectType).Where(o => FilterByPattern(o, str));
    }

    [GeneratedRegex("^([A-Za-z]+):([^:]+)$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex HasId();
}
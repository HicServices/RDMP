// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

/// <summary>
///     Determines if a command line argument provided was a reference to one or more <see cref="DatabaseEntity" />
///     matching based on ID (e.g. "Catalogue:23")
/// </summary>
public partial class PickObjectByID : PickObjectBase
{
    /*
        Console.WriteLine("Format \"\" e.g. \"Catalogue:*mysql*\" or \"Catalogue:12,23,34\"");

        */
    public override string Format => "{Type}:{ID}[,{ID2},{ID3}...]";

    public override string Help =>
        @"Type: must be an RDMP object type e.g. Catalogue, Project etc.
ID: must reference an object that exists
ID2+: (optional) only allowed if you are being prompted for multiple objects, allows you to specify multiple objects of the same Type using comma separator";

    public override IEnumerable<string> Examples => new[]
    {
        "Catalogue:1",
        "Catalogue:1,2,3"
    };

    public override bool IsMatch(string arg, int idx)
    {
        var baseMatch = base.IsMatch(arg, idx);

        //only considered  match if the first letter is an Rdmp Type e.g. "Catalogue:12" but not "C:\fish"
        return baseMatch && IsDatabaseObjectType(Regex.Match(arg).Groups[1].Value, out _);
    }

    public PickObjectByID(IBasicActivateItems activator)
        : base(activator,
            HasId())
    {
    }

    public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
    {
        var objByID = MatchOrThrow(arg, idx);

        var objectType = objByID.Groups[1].Value;
        var objectId = objByID.Groups[2].Value;

        var dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);

        var objs = objectId.Split(',').Select(id => GetObjectByID(dbObjectType, int.Parse(id))).Distinct();

        return new CommandLineObjectPickerArgumentValue(arg, idx, objs.ToArray());
    }

    [GeneratedRegex("^([A-Za-z]+):([0-9,]+)$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex HasId();
}
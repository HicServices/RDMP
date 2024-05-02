// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandLine.Interactive.Picking;

/// <summary>
///     Determines if a command line argument provided was a reference to one or more <see cref="DatabaseEntity" />
///     matching based on property (e.g. "Catalogue?Folder:*edris*")
/// </summary>
public class PickObjectByQuery : PickObjectBase
{
    public override string Format => "{Type}?{Property}:{PropertyValue}";

    public override string Help =>
        @"Type: must be an RDMP object type e.g. Catalogue, Project etc.
Property: must be a property of the Type class.
NamePattern: must be a value that could appear for the given Property.  Comparison will be via ToString on property value.";

    public override IEnumerable<string> Examples => new[]
    {
        "CatalogueItem?Catalogue_ID:55",
        "Catalogue?Folder:*edris*"
    };

    public PickObjectByQuery(IBasicActivateItems activator) :
        base(activator,
            new Regex(@"^(\w+)\?(\w+):([^:]+)$", RegexOptions.IgnoreCase))
    {
    }

    public override bool IsMatch(string arg, int idx)
    {
        var baseMatch = base.IsMatch(arg, idx);

        //only considered  match if the first letter is an Rdmp Type e.g. "Catalogue:fish" but not "C:\fish"
        return baseMatch && IsDatabaseObjectType(Regex.Match(arg).Groups[1].Value, out _);
    }

    public override CommandLineObjectPickerArgumentValue Parse(string arg, int idx)
    {
        if (IsDatabaseObjectType(arg, out var t))
            return new CommandLineObjectPickerArgumentValue(arg, idx, GetAllObjects(t).ToArray());

        var objByToString = MatchOrThrow(arg, idx);

        var objectType = objByToString.Groups[1].Value;
        var propertyName = objByToString.Groups[2].Value;
        var objectToString = objByToString.Groups[3].Value;

        var dbObjectType = ParseDatabaseEntityType(objectType, arg, idx);

        var property = dbObjectType.GetProperty(propertyName) ??
                       throw new Exception(
                           $"Unknown property '{propertyName}'.  Did not exist on Type '{dbObjectType.Name}'");
        var objs = GetObjectByToString(dbObjectType, property, objectToString);
        return new CommandLineObjectPickerArgumentValue(arg, idx, objs.Cast<IMapsDirectlyToDatabaseTable>().ToArray());
    }

    private IEnumerable<object> GetObjectByToString(Type dbObjectType, PropertyInfo property, string str)
    {
        return GetAllObjects(dbObjectType).Where(o =>
        {
            var value = property.GetValue(o)?.ToString() ?? "Null";
            return FilterByPattern(value, str);
        });
    }
}
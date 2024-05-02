// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rdmp.Core.Validation.Dependency;

/// <summary>
///     Regex pattern for finding references in ValidatorXML without having to deserialize it.  This is used to identify
///     rules which reference columns and ensure
///     that they cannot be deleted (See ValidationXMLObscureDependencyFinder)
/// </summary>
public class Suspect
{
    public string Pattern { get; private set; }
    public Type Type { get; private set; }
    public List<PropertyInfo> SuspectProperties = new();
    public List<FieldInfo> SuspectFields = new();

    public Suspect(string pattern, Type type, List<PropertyInfo> suspectProperties, List<FieldInfo> suspectFields)
    {
        Pattern = pattern;
        Type = type;
        SuspectProperties = suspectProperties;
        SuspectFields = suspectFields;
    }
}
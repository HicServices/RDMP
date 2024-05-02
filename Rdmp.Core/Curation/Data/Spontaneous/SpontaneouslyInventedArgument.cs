// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Spontaneous;

/// <summary>
///     Spontaneous (memory only) implementation of IArgument.
/// </summary>
public class SpontaneouslyInventedArgument : SpontaneousObject, IArgument
{
    private readonly object _value;

    public SpontaneouslyInventedArgument(MemoryRepository repo, string name, object value) : base(repo)
    {
        Name = name;
        _value = value;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public string Value => _value.ToString();


    public string Type => _value.GetType().FullName;

    public void SetValue(object o)
    {
        throw new NotSupportedException();
    }

    public object GetValueAsSystemType()
    {
        return _value;
    }

    public Type GetSystemType()
    {
        return _value.GetType();
    }

    public Type GetConcreteSystemType()
    {
        return _value.GetType();
    }

    public void SetType(Type t)
    {
        throw new NotImplementedException();
    }
}
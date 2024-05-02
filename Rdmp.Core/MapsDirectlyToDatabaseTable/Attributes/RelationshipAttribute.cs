// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Runtime.CompilerServices;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;

/// <summary>
///     Used to indicate when an ID column contains the ID of another RDMP object.  Decorate the foreign key object. This
///     can be involve going
///     between databases or even servers e.g. between DataExport and Catalogue libraries or between Catalogue and plugin
///     databases
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class RelationshipAttribute : Attribute
{
    /// <summary>
    ///     The other class whose ID is stored in decorated property
    /// </summary>
    public Type Cref { get; set; }

    /// <summary>
    ///     The decorated property
    /// </summary>
    public string PropertyName { get; set; }

    public RelationshipType Type { get; set; }

    /// <summary>
    ///     Optional function name for finding compatible objects to populate this relationship.  When null all objects are
    ///     considered viable
    ///     Must be a paramaterless method
    /// </summary>
    public string ValueGetter { get; set; }

    /// <summary>
    ///     Declares that the decorated property contains the ID of the specified Type of object
    /// </summary>
    /// <param name="cref"></param>
    /// <param name="type"></param>
    /// <param name="propertyName"></param>
    public RelationshipAttribute(Type cref, RelationshipType type, [CallerMemberName] string propertyName = null)
    {
        Cref = cref;
        Type = type;
        PropertyName = propertyName;
    }

    #region Equality Members

    protected bool Equals(RelationshipAttribute other)
    {
        return base.Equals(other) && Equals(Cref, other.Cref) &&
               string.Equals(PropertyName, other.PropertyName);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((RelationshipAttribute)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), Cref, PropertyName);
    }

    #endregion
}
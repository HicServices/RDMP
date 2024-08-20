// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

#nullable enable
using System;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Equ;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace ResearchDataManagementPlatform.WindowManagement;

/// <summary>
/// Records the fact that the user visited a specific object in a tree collection
/// </summary>
public sealed class CollectionNavigation : IEquatable<CollectionNavigation>, INavigation
{
    private readonly IMapsDirectlyToDatabaseTable _object;

    public bool IsAlive => _object is not IMightNotExist o || o.Exists();

    public CollectionNavigation(IMapsDirectlyToDatabaseTable @object)
    {
        _object = @object;
    }

    public void Activate(ActivateItems activateItems)
    {
        activateItems.RequestItemEmphasis(this, new EmphasiseRequest(_object, 0));
    }

    public void Close()
    {
    }

    public override string? ToString() => _object.ToString();

    public bool Equals(CollectionNavigation? other)
    {
        if (other is null) return false;

        return ReferenceEquals(this, other) || Equals(_object, other._object);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || (obj is CollectionNavigation other && Equals(other));

    public override int GetHashCode() => HashCode.Combine(_object);
}
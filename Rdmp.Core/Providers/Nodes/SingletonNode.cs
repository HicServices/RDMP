// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
/// <see cref="Node"/> of which there can only ever be one in the RDMP object hierarchy e.g. <see cref="AllCohortsNode"/>.  By convention
/// these classes should normally start with the prefix "All"
/// </summary>
public abstract class SingletonNode:Node
{
    protected readonly string Caption;

    protected SingletonNode(string caption)
    {
        Caption = caption;
    }

    public override string ToString()
    {
        return Caption;
    }

    protected bool Equals(SingletonNode other)
    {
        return string.Equals(Caption, other.Caption);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        return ReferenceEquals(this, obj) || obj is SingletonNode other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Caption.GetHashCode();
    }
}
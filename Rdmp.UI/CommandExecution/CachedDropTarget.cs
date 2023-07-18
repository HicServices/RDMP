// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.UI.CommandExecution;

internal class CachedDropTarget
{
    public object Target { get; }
    public InsertOption RelativeLocation { get; }

    public CachedDropTarget(object target, InsertOption relativeLocation)
    {
        Target = target;
        RelativeLocation = relativeLocation;
    }

    protected bool Equals(CachedDropTarget other)
    {
        return Equals(Target, other.Target) && RelativeLocation == other.RelativeLocation;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CachedDropTarget) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Target, RelativeLocation);
    }
}
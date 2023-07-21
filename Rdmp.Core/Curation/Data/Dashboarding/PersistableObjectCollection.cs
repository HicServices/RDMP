// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Dashboarding;

public abstract class PersistableObjectCollection : IPersistableObjectCollection
{
    public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

    public PersistableObjectCollection()
    {
        DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
    }

    public virtual string SaveExtraText() => "";

    public virtual void LoadExtraText(string s)
    {
    }

    protected bool Equals(PersistableObjectCollection other) => DatabaseObjects.SequenceEqual(other.DatabaseObjects) &&
                                                                Equals(SaveExtraText(), other.SaveExtraText());

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((PersistableObjectCollection)obj);
    }

    public override int GetHashCode() => System.HashCode.Combine(DatabaseObjects, SaveExtraText());
}
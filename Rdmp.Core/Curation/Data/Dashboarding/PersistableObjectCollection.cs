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
    public PersistStringHelper Helper { get; private set; }
    public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

    public PersistableObjectCollection()
    {
        DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
        Helper = new PersistStringHelper();
    }

    public virtual string SaveExtraText()
    {
        return "";
    }

    public virtual void LoadExtraText(string s)
    {
            
    }

    protected bool Equals(PersistableObjectCollection other)
    {
        return DatabaseObjects.SequenceEqual(other.DatabaseObjects) && Equals(SaveExtraText(), other.SaveExtraText());
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PersistableObjectCollection) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return
                (397 * (DatabaseObjects != null ?
                        DatabaseObjects.Aggregate(0, (old, curr) =>
                            (old * 397) ^ (curr != null ? curr.GetHashCode() : 0)) :
                        0)
                ) ^
                (SaveExtraText() != null ? SaveExtraText().GetHashCode() : 0);
        } 
    }
}
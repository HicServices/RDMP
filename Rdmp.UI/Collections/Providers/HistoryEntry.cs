// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.Collections.Providers;

/// <summary>
/// Records and persists a users access of an <see cref="Object"/> including the time (<see cref="Date"/>) it was accessed
/// </summary>
public class HistoryEntry : IMasqueradeAs
{
    public IMapsDirectlyToDatabaseTable Object { get; private set; }
    public DateTime Date { get; private set; }

    private HistoryEntry()
    {
            
    }

    public HistoryEntry(IMapsDirectlyToDatabaseTable o, DateTime date)
    {
        Object = o;
        Date = date;
    }
        
    public string Serialize()
    {
        var helper = new PersistStringHelper();
        return helper.GetObjectCollectionPersistString(Object) + PersistStringHelper.ExtraText + Date;
    }

    public static HistoryEntry Deserialize(string s, IRDMPPlatformRepositoryServiceLocator locator)
    {
        var e = new HistoryEntry();

        try
        {
            var helper = new PersistStringHelper();
            e.Date = DateTime.Parse(helper.GetExtraText(s));

            var objectString = s.Substring(0, s.IndexOf(PersistStringHelper.ExtraText));
                

            e.Object = helper.GetObjectCollectionFromPersistString(objectString,locator).Single();
        }
        catch (PersistenceException )
        {
            return null;
        }

        return e;
    }


    protected bool Equals(HistoryEntry other)
    {
        return Equals(Object, other.Object);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((HistoryEntry) obj);
    }

    public override int GetHashCode()
    {
        return (Object != null ? Object.GetHashCode() : 0);
    }

    public object MasqueradingAs()
    {
        return Object;
    }
}
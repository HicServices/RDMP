// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.UI.Collections.Providers;

/// <summary>
/// Records and persists a users access of an <see cref="Object"/> including the time (<see cref="Date"/>) it was accessed
/// </summary>
public class HistoryEntry : IMasqueradeAs
{
    public IMapsDirectlyToDatabaseTable Object { get; }
    public DateTime Date { get; }

    public HistoryEntry(IMapsDirectlyToDatabaseTable o, DateTime date)
    {
        Object = o;
        Date = date;
    }

    public string Serialize()
    {
        return $"{PersistStringHelper.GetObjectCollectionPersistString(Object)}{PersistStringHelper.ExtraText}{Date}";
    }

    public static HistoryEntry Deserialize(string s, IRDMPPlatformRepositoryServiceLocator locator)
    {

        try
        {
            var objectString = s[..s.IndexOf(PersistStringHelper.ExtraText, StringComparison.Ordinal)];
            return new HistoryEntry(
                PersistStringHelper.GetObjectCollectionFromPersistString(objectString, locator).Single(),
                DateTime.Parse(PersistStringHelper.GetExtraText(s)));
        }
        catch (PersistenceException )
        {
            return null;
        }

    }


    protected bool Equals(HistoryEntry other)
    {
        return Equals(Object, other.Object);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((HistoryEntry) obj);
    }

    public override int GetHashCode()
    {
        return Object?.GetHashCode() ?? 0;
    }

    public object MasqueradingAs()
    {
        return Object;
    }
}
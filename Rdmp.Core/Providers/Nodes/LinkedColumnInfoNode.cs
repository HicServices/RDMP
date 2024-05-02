// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Providers.Nodes;

public class LinkedColumnInfoNode : Node, IDeleteable, IMasqueradeAs
{
    public CatalogueItem CatalogueItem { get; }
    public ColumnInfo ColumnInfo { get; }

    public LinkedColumnInfoNode(CatalogueItem catalogueItem, ColumnInfo columnInfo)
    {
        CatalogueItem = catalogueItem;
        ColumnInfo = columnInfo;
    }

    public override string ToString()
    {
        return ColumnInfo.ToString();
    }

    protected bool Equals(LinkedColumnInfoNode other)
    {
        return Equals(CatalogueItem, other.CatalogueItem) && Equals(ColumnInfo, other.ColumnInfo);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LinkedColumnInfoNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CatalogueItem, ColumnInfo);
    }

    public object MasqueradingAs()
    {
        return ColumnInfo;
    }

    public void DeleteInDatabase()
    {
        CatalogueItem.SetColumnInfo(null);
    }
}
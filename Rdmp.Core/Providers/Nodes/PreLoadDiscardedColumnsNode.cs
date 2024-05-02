// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     All virtual columns in this <see cref="Curation.Data.TableInfo" /> which appear in the data load (RAW) but do not
///     exist in the live database (or are diluted).  This enables
///     anonymisation or dropping of columns during the data load (See <see cref="PreLoadDiscardedColumn" />)
/// </summary>
public class PreLoadDiscardedColumnsNode : Node
{
    public TableInfo TableInfo { get; }

    public PreLoadDiscardedColumnsNode(TableInfo tableInfo)
    {
        TableInfo = tableInfo;
    }

    public override string ToString()
    {
        return "Discarded Columns";
    }

    protected bool Equals(PreLoadDiscardedColumnsNode other)
    {
        return Equals(TableInfo, other.TableInfo);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PreLoadDiscardedColumnsNode)obj);
    }

    public override int GetHashCode()
    {
        return TableInfo != null ? TableInfo.GetHashCode() : 0;
    }
}
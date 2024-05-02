// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Collection of all <see cref="AggregateConfiguration" /> graphs for visualising data in the
///     <see cref="Curation.Data.Catalogue" />.
/// </summary>
public class AggregatesNode : Node
{
    /// <summary>
    ///     The <see cref="Curation.Data.Catalogue" /> to which all the <see cref="AggregateConfiguration" /> belong
    /// </summary>
    public Catalogue Catalogue { get; set; }

    public AggregatesNode(Catalogue c, AggregateConfiguration[] regularAggregates)
    {
        Catalogue = c;
    }

    public override string ToString()
    {
        return "Aggregate Graphs";
    }

    protected bool Equals(AggregatesNode other)
    {
        return Catalogue.Equals(other.Catalogue);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AggregatesNode)obj);
    }

    public override int GetHashCode()
    {
        return Catalogue.GetHashCode() * GetType().GetHashCode();
    }
}
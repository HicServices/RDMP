// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Collection node for all <see cref="Lookup" /> that exist under a <see cref="Catalogue" />
/// </summary>
public class CatalogueLookupsNode : Node
{
    public Catalogue Catalogue { get; }
    public Lookup[] Lookups { get; set; }

    public CatalogueLookupsNode(Catalogue catalogue, Lookup[] lookups)
    {
        Catalogue = catalogue;
        Lookups = lookups;
    }

    public override string ToString()
    {
        return "Lookups";
    }

    protected bool Equals(CatalogueLookupsNode other)
    {
        return Equals(Catalogue, other.Catalogue);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(CatalogueLookupsNode) && Equals((CatalogueLookupsNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Catalogue);
    }
}
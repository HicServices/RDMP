// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Collection of all the virtual columns (<see cref="CatalogueItem" />) in a dataset (
///     <see cref="Curation.Data.Catalogue" />)
/// </summary>
public class CatalogueItemsNode : Node, IOrderable
{
    public Catalogue Catalogue { get; }
    public CatalogueItem[] CatalogueItems { get; }

    public ExtractionCategory? Category { get; }

    public int Order
    {
        get => Category.HasValue ? (int)Category + 1 : 20;
        set { } // no setter, we are orderable to enforce specific order in tree
    }

    public CatalogueItemsNode(Catalogue catalogue, IEnumerable<CatalogueItem> cis, ExtractionCategory? category)
    {
        Catalogue = catalogue;
        CatalogueItems = cis.ToArray();
        Category = category;
    }

    public override string ToString()
    {
        return Category == null
            ? "Non Extractable"
            : Category switch
            {
                ExtractionCategory.Core => "Core Items",
                ExtractionCategory.Supplemental => "Supplemental Items",
                ExtractionCategory.SpecialApprovalRequired => "Special Approval Items",
                ExtractionCategory.Internal => "Internal Items",
                ExtractionCategory.Deprecated => "Deprecated Items",
                _ => "Catalogue Items"
            };
    }

    protected bool Equals(CatalogueItemsNode other)
    {
        return Catalogue.Equals(other.Catalogue) && Equals(Category, other.Category);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == typeof(CatalogueItemsNode) && Equals((CatalogueItemsNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Catalogue, Category);
    }
}
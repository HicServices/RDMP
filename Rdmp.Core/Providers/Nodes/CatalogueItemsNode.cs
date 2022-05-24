// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.Core.Providers.Nodes
{
    /// <summary>
    /// Collection of all the virtual columns (<see cref="CatalogueItem"/>) in a dataset (<see cref="Curation.Data.Catalogue"/>)
    /// </summary>
    public class CatalogueItemsNode : Node, IOrderable
    {
        public Catalogue Catalogue { get; }
        public CatalogueItem[] CatalogueItems { get; }

        public ExtractionCategory? Category { get; }
        public int Order
        {
            get { return Category.HasValue ? (int)Category +1: 20; }
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
            if(Category == null)
                return "Non Extractable";

            switch (Category)
            {
                case ExtractionCategory.Core:
                    return "Core Items";
                case ExtractionCategory.Supplemental:
                    return "Supplemental Items";
                case ExtractionCategory.SpecialApprovalRequired:
                    return "Special Approval Items";
                case ExtractionCategory.Internal:
                    return "Internal Items";
                case ExtractionCategory.Deprecated:
                    return "Deprecated Items";
            }

            return "Catalogue Items";
        }

        protected bool Equals(CatalogueItemsNode other)
        {
            return Catalogue.Equals(other.Catalogue) && Equals(Category,other.Category);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (CatalogueItemsNode)) return false;
            return Equals((CatalogueItemsNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Catalogue.GetHashCode() * (Category?.GetHashCode() ?? -12342);
            }
        }
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Providers.Nodes
{
    /// <summary>
    /// Collection of all the virtual columns (<see cref="CatalogueItem"/>) in a dataset (<see cref="Curation.Data.Catalogue"/>)
    /// </summary>
    public class CatalogueItemsNode:Node
    {
        public Catalogue Catalogue { get; set; }
        public CatalogueItem[] CatalogueItems { get; }

        public CatalogueItemsNode(Catalogue catalogue, CatalogueItem[] cis)
        {
            Catalogue = catalogue;
            CatalogueItems = cis;
        }

        public override string ToString()
        {
            return "Catalogue Items";
        }

        protected bool Equals(CatalogueItemsNode other)
        {
            return Catalogue.Equals(other.Catalogue);
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
            return Catalogue.GetHashCode();
        }
    }
}
using System;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    public class CatalogueItemsNode
    {
        public Catalogue Catalogue { get; set; }

        public CatalogueItemsNode(Catalogue catalogue, CatalogueItem[] cis)
        {
            Catalogue = catalogue;
            
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
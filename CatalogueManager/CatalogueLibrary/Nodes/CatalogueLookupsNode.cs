using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    public class CatalogueLookupsNode
    {
        public Catalogue Catalogue { get; set; }
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(CatalogueLookupsNode)) return false;
            return Equals((CatalogueLookupsNode)obj);
        }

        public override int GetHashCode()
        {
            return (Catalogue != null ? Catalogue.GetHashCode() : 0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class CatalogueUsedByLoadMetadataNode
    {
        public LoadMetadata LoadMetadata { get; private set; }
        public Catalogue Catalogue { get; private set; }

        public CatalogueUsedByLoadMetadataNode(LoadMetadata loadMetadata, Catalogue catalogue)
        {
            LoadMetadata = loadMetadata;
            Catalogue = catalogue;
        }

        public override string ToString()
        {
            return Catalogue.ToString();
        }

        protected bool Equals(CatalogueUsedByLoadMetadataNode other)
        {
            return Equals(LoadMetadata, other.LoadMetadata) && Equals(Catalogue, other.Catalogue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CatalogueUsedByLoadMetadataNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LoadMetadata != null ? LoadMetadata.GetHashCode() : 0)*397) ^ (Catalogue != null ? Catalogue.GetHashCode() : 0);
            }
        }
    }
}

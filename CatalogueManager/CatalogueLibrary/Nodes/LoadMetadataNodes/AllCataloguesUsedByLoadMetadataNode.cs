using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class AllCataloguesUsedByLoadMetadataNode
    {
        public LoadMetadata LoadMetadata { get; private set; }

        public AllCataloguesUsedByLoadMetadataNode(LoadMetadata lmd)
        {
            LoadMetadata = lmd;
        }

        public override string ToString()
        {
            return "Catalogues";
        }

        protected bool Equals(AllCataloguesUsedByLoadMetadataNode other)
        {
            return Equals(LoadMetadata, other.LoadMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AllCataloguesUsedByLoadMetadataNode) obj);
        }

        public override int GetHashCode()
        {
            return (LoadMetadata != null ? LoadMetadata.GetHashCode() : 0);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class LoadMetadataScheduleNode
    {
        public LoadMetadata LoadMetadata { get; private set; }

        public LoadMetadataScheduleNode(LoadMetadata loadMetadata)
        {
            LoadMetadata = loadMetadata;
        }

        public override string ToString()
        {
            return "Scheduling";
        }

        protected bool Equals(LoadMetadataScheduleNode other)
        {
            return Equals(LoadMetadata, other.LoadMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadMetadataScheduleNode) obj);
        }

        public override int GetHashCode()
        {
            return (LoadMetadata != null ? LoadMetadata.GetHashCode() : 0);
        }
    }
}

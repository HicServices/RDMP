using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class LoadStageNode : IOrderable
    {
        public LoadMetadata LoadMetadata { get; private set; }
        public LoadStage LoadStage { get; private set; }

        //prevent reordering
        public int Order { get { return (int)LoadStage; } set { } }

        public LoadStageNode(LoadMetadata loadMetadata, LoadStage loadStage)
        {
            LoadMetadata = loadMetadata;
            LoadStage = loadStage;
        }

        public override string ToString()
        {
            return LoadStage.ToString();
        }

        protected bool Equals(LoadStageNode other)
        {
            return Equals(LoadMetadata, other.LoadMetadata) && LoadStage == other.LoadStage;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoadStageNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((LoadMetadata != null ? LoadMetadata.GetHashCode() : 0)*397) ^ (int) LoadStage;
            }
        }
    }
}

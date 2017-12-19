using System.IO;
using CatalogueLibrary.Data.DataLoad;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class HICProjectDirectoryNode: IDirectoryInfoNode
    {
        public LoadMetadata LoadMetadata { get; set; }

        public HICProjectDirectoryNode(LoadMetadata loadMetadata)
        {
            LoadMetadata = loadMetadata;
        }

        public bool IsEmpty { get { return string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles); } }
        

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles) ? "???" : LoadMetadata.LocationOfFlatFiles;
        }

        public DirectoryInfo GetDirectoryInfoIfAny()
        {
            if (string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles ))
                return null;

            return new DirectoryInfo(LoadMetadata.LocationOfFlatFiles);
        }

        protected bool Equals(HICProjectDirectoryNode other)
        {
            return Equals(LoadMetadata, other.LoadMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((HICProjectDirectoryNode) obj);
        }

        public override int GetHashCode()
        {
            return (LoadMetadata != null ? LoadMetadata.GetHashCode() : 0);
        }
    }
}

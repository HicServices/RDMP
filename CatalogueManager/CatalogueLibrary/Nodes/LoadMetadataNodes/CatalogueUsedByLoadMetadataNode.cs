using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class CatalogueUsedByLoadMetadataNode:IMasqueradeAs,IDeletableWithCustomMessage
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

        #region Equality Members
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

        public object MasqueradingAs()
        {
            return Catalogue;
        }

        #endregion

        public void DeleteInDatabase()
        {
            Catalogue.LoadMetadata_ID = null;
            Catalogue.SaveToDatabase();
        }

        public string GetDeleteMessage()
        {
            return "disassociate Catalogue '" + Catalogue +"' from it's Load logic";
        }
    }
}

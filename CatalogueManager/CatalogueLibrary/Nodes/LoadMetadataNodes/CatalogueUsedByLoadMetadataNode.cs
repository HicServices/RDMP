using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.UsedByNodes;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    public class CatalogueUsedByLoadMetadataNode:ObjectUsedByOtherObjectNode<LoadMetadata,Catalogue>,IDeletableWithCustomMessage
    {

        public CatalogueUsedByLoadMetadataNode(LoadMetadata loadMetadata, Catalogue catalogue):base(loadMetadata,catalogue)
        {
        }


        public void DeleteInDatabase()
        {
            ObjectBeingUsed.LoadMetadata_ID = null;
            ObjectBeingUsed.SaveToDatabase();
        }

        public string GetDeleteMessage()
        {
            return "disassociate Catalogue '" + User +"' from it's Load logic";
        }
    }
}

using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Nodes.UsedByNodes;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes.LoadMetadataNodes
{
    class OverrideRawServerNode:ObjectUsedByOtherObjectNode<LoadMetadata,ExternalDatabaseServer>,IDeletableWithCustomMessage
    {
        public OverrideRawServerNode(LoadMetadata user, ExternalDatabaseServer objectBeingUsed) : base(user, objectBeingUsed)
        {

        }

        public void DeleteInDatabase()
        {
            User.OverrideRAWServer_ID = null;
            User.SaveToDatabase();
        }

        public override string ToString()
        {
            return "Override RAW:" + ObjectBeingUsed.Name;
        }

        public string GetDeleteMessage()
        {
            return "stop using explicit RAW server";
        }
    }
}

using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    public class AllLoadMetadatasNode : SingletonNode, IOrderable
    {
        public AllLoadMetadatasNode() : base("Data Loads (LoadMetadata)")
        {
        }

        public int Order { get { return 1; } set{} }
    }
}
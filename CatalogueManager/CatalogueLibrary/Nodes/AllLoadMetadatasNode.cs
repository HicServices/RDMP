using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;

namespace CatalogueLibrary.Nodes
{
    /// <summary>
    /// Collection of all the Data Load Engine configurations you have defined.  Each load populates one or more <see cref="TableInfo"/> dependant on the
    /// associated <see cref="Catalogue"/>s.
    /// </summary>
    public class AllLoadMetadatasNode : SingletonNode, IOrderable
    {
        public AllLoadMetadatasNode() : base("Data Loads (LoadMetadata)")
        {
        }

        public int Order { get { return 1; } set{} }
    }
}
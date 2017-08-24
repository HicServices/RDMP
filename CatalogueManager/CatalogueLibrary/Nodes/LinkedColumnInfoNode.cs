using CatalogueLibrary.Data;

namespace CatalogueLibrary.Nodes
{
    public class LinkedColumnInfoNode
    {
        public CatalogueItem CatalogueItem { get; set; }
        public ColumnInfo ColumnInfo { get; set; }

        public LinkedColumnInfoNode(CatalogueItem catalogueItem, ColumnInfo columnInfo)
        {
            CatalogueItem = catalogueItem;
            ColumnInfo = columnInfo;
        }

        public override string ToString()
        {
            return ColumnInfo.ToString();
        }
    }
}
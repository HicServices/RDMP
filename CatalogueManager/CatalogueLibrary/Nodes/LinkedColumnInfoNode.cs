using System.Windows;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Nodes
{
    public class LinkedColumnInfoNode : IDeleteable, IMasqueradeAs
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

        protected bool Equals(LinkedColumnInfoNode other)
        {
            return Equals(CatalogueItem, other.CatalogueItem) && Equals(ColumnInfo, other.ColumnInfo);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LinkedColumnInfoNode) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((CatalogueItem != null ? CatalogueItem.GetHashCode() : 0)*397) ^ (ColumnInfo != null ? ColumnInfo.GetHashCode() : 0);
            }
        }

        public object MasqueradingAs()
        {
            return ColumnInfo;
        }

        public void DeleteInDatabase()
        {
            CatalogueItem.SetColumnInfo(null);
        }
    }
}
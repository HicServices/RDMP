using System;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.QueryBuilding
{
    public class ColumnInfoToIColumn : IColumn
    {
        static Random r = new Random();

        public ColumnInfoToIColumn(ColumnInfo column)
        {
            ColumnInfo = column;
            Order = -1;
            SelectSQL = column.GetRuntimeName();
            ID = -1 * r.Next(1,5000000);
            Alias = null;
            HashOnDataRelease = false;
            IsExtractionIdentifier = false;
            IsPrimaryKey = false;
        }

        public string GetRuntimeName()
        {
            return ColumnInfo.GetRuntimeName();
        }

        public ColumnInfo ColumnInfo { get; private set; }
        public int Order { get; set; }
        public string SelectSQL { get; set; }
        public int ID { get; private set; }
        public string Alias { get; set; }
        public bool HashOnDataRelease { get; private set; }
        public bool IsExtractionIdentifier { get; private set; }
        public bool IsPrimaryKey { get; private set; }
    }
}
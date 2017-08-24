using System;
using System.Data.Common;

namespace CatalogueLibrary.Data.PerformanceImprovement
{
    public class CatalogueItemClassification
    {
        public int CatalogueItem_ID;
        public int? ExtractionInformation_ID;
        public int? ColumnInfo_ID;
        public ExtractionCategory? ExtractionCategory;
        public bool IsPrimaryKey;
        public bool IsLookupDescription;
        public bool IsLookupForeignKey;
        public bool IsLookupPrimaryKey;
        public int ExtractionFilterCount;
        public int? Order;

        /// <summary>
        /// Don't create these yourself, instead use CatalogueRepository.ClassifyAllCatalogueItems()
        /// </summary>
        /// <param name="r"></param>
        public CatalogueItemClassification(DbDataReader r)
        {
            CatalogueItem_ID = Convert.ToInt32(r[CatalogueItem_ID]);
            ExtractionInformation_ID = DatabaseEntity.ObjectToNullableInt(r["ExtractionInformation_ID"]);
            if (r["ExtractionCategory"] == DBNull.Value)
                ExtractionCategory = null;
            else
                ExtractionCategory = (ExtractionCategory) Enum.Parse(typeof (ExtractionCategory), r["ExtractionCategory"].ToString());

            ColumnInfo_ID = DatabaseEntity.ObjectToNullableInt(r["ColumnInfo_ID"]);
            Order = DatabaseEntity.ObjectToNullableInt(r["Order"]);
            IsPrimaryKey = Convert.ToBoolean(r["IsPrimaryKey"]);
            IsLookupDescription = Convert.ToBoolean(r["IsLookupDescription"]);
            IsLookupForeignKey = Convert.ToBoolean(r["IsLookupForeignKey"]);
            IsLookupPrimaryKey = Convert.ToBoolean(r["IsLookupPrimaryKey"]);
            ExtractionFilterCount = Convert.ToInt32(r["ExtractionFilterCount"]);
        }

        public bool IsExtractionInformationOrphan()
        {
            return ExtractionInformation_ID != null && ColumnInfo_ID == null;
        }
    }
}
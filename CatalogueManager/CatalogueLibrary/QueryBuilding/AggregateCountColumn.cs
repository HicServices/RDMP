using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;

namespace CatalogueLibrary.QueryBuilding
{
    public class AggregateCountColumn:IColumn
    {
        public AggregateCountColumn(string sql)
        {
            string select;
            string alias;

            RDMPQuerySyntaxHelper.SplitLineIntoSelectSQLAndAlias(sql, out select, out alias);
            
            SelectSQL = select;
            Alias = alias;
        }
        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }

        public string GetFullSelectLineStringForSavingIntoAnAggregate()
        {
            if (string.IsNullOrWhiteSpace(Alias))
                return SelectSQL;
            
            return SelectSQL + RDMPQuerySyntaxHelper.AliasPrefix + Alias;
        }

        public ColumnInfo ColumnInfo { get { return null; } }
        public int Order { get; set; }
        
        public string SelectSQL { get; set; }
        public int ID { get { return -1; }}

        public string Alias{get; private set; }
        public bool HashOnDataRelease { get { return false; }}
        public bool IsExtractionIdentifier { get { return false; } }
        public bool IsPrimaryKey { get { return false; } }
    }
}

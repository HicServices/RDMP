using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;

namespace LoadModules.Generic.Mutilators.QueryBuilders
{
    public class TableInfoJoiningQueryBuilder
    {
        public string GetJoinSQL(params TableInfo[] tableInfos)
        {
           QueryBuilder qb = new QueryBuilder("","");

           foreach (TableInfo tableInfo in tableInfos)
                foreach (ColumnInfo col in tableInfo.ColumnInfos)
                    qb.AddColumn(new ColumnInfoToIColumn(col));

            return qb.GetSQLSubstringStartingAtLineNumber(qb.GetLineNumberOfFirst(QueryComponent.FROM));
        }
    }
}
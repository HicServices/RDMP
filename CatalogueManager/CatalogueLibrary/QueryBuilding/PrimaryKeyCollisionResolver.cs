using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;

using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// The RDMP data load engine is designed to prevent duplicate data entering your live database.  This is achieved by requiring a primary key defined by the source
    /// data (i.e. not an autonum).  However it is expected that semantically correct primary keys will not be perfectly supplied in all cases by data providers, for example
    /// if 'TestLabCode' is the primary key on biochemistry but duplicates appear with unique values in 'DataAge' it would be reasonable to assume that newer 'DataAge' records
    /// replace older ones.  Therefore we might decide to keep the primary key as 'TestLabCode' and then discard duplicate records based on preserving the latest 'DataAge'.
    /// 
    /// <para>This class handles creating the query that deletes duplicates based on the column preference order supplied (See ConfigurePrimaryKeyCollisionResolution). </para>
    /// </summary>
    public class PrimaryKeyCollisionResolver
    {
        private readonly TableInfo _tableInfo;
        private MicrosoftQuerySyntaxHelper _syntaxHelper;

        private const string WithCTE = "WITH CTE (DuplicateCount)";
        private const string SelectRownum = "\t SELECT ROW_NUMBER()";
        private const string DeleteBit =
@"DELETE 
FROM CTE 
WHERE DuplicateCount > 1";


        public PrimaryKeyCollisionResolver(TableInfo tableInfo)
        {
            _tableInfo = tableInfo;
            _syntaxHelper = new MicrosoftQuerySyntaxHelper();
        }

        public string GenerateSQL()
        {
            ColumnInfo[] pks;
            List<IResolveDuplication> resolvers;

            return GenerateSQL(out pks, out resolvers);
        }

        private string GenerateSQL(out ColumnInfo[] pks, out List<IResolveDuplication> resolvers)
        {
            string sql = "";
            string tableNameInRAW = GetTableName();

            var cols = _tableInfo.ColumnInfos.ToArray();
            pks = cols.Where(col => col.IsPrimaryKey).ToArray();
            
            if(!pks.Any())
                throw new Exception("TableInfo " + _tableInfo.GetRuntimeName() + " does not have any primary keys defined so cannot resolve primary key collisions");

            string primaryKeys = pks.Aggregate("", (s, n) => s + RDMPQuerySyntaxHelper.EnsureValueIsWrapped(n.GetRuntimeName(LoadStage.AdjustRaw)) + ",");
            primaryKeys = primaryKeys.TrimEnd(new[] {','});


            sql += "/*Notice how entities are not fully indexed with Database, this is because this code will run on RAW servers, prior to reaching STAGING/LIVE - the place where there are primary keys*/" + Environment.NewLine;

            sql += WithCTE + Environment.NewLine;
            sql += "AS" + Environment.NewLine;
            sql += "(" + Environment.NewLine;
            sql += SelectRownum + " OVER(" + Environment.NewLine;
            sql += "\t PARTITION BY" + Environment.NewLine;
            sql += "\t\t " + primaryKeys + Environment.NewLine;
            sql += "\t ORDER BY"  + Environment.NewLine;

            sql += "\t /*Priority in which order they should be used to resolve duplication of the primary key values, order by:*/"  + Environment.NewLine;
            
            resolvers = new List<IResolveDuplication>();

            resolvers.AddRange(cols.Where(c => c.DuplicateRecordResolutionOrder != null));
            resolvers.AddRange(_tableInfo.PreLoadDiscardedColumns.Where(c => c.DuplicateRecordResolutionOrder != null));

            if (!resolvers.Any())
                throw new Exception("The ColumnInfos of TableInfo " + _tableInfo + " do not have primary key resolution orders configured (do not know which order to use non primary key column values in to resolve collisions).  Fix this by right clicking a TableInfo in CatalogueManager and selecting 'Configure Primary Key Collision Resolution'.");

            //order by the priority of columns 
            foreach (IResolveDuplication column in resolvers.OrderBy(col => col.DuplicateRecordResolutionOrder))
            {
                if(column is ColumnInfo && ((ColumnInfo)column).IsPrimaryKey )
                    throw new Exception("Column " + column.GetRuntimeName() + " is flagged as primary key when it also has a DuplicateRecordResolutionOrder, primary keys cannot be used to resolve duplication since they are the hash!  Resolve this in the CatalogueManager by right clicking the offending TableInfo " + _tableInfo.GetRuntimeName() + " and editing the resolution order");
                
                sql = AppendRelevantOrderBySql(sql, column);
            }

            //trim the last remaining open bracket
            sql = sql.TrimEnd(new[] {',','\r','\n'}) + Environment.NewLine;

            sql += ") AS DuplicateCount" + Environment.NewLine;
            sql += "FROM " + tableNameInRAW + Environment.NewLine;
            sql += ")" + Environment.NewLine;

            sql += DeleteBit;

            return sql;
        }

        private string GetTableName()
        {
            return _tableInfo.GetRuntimeName(LoadStage.AdjustRaw);
        }

        private string AppendRelevantOrderBySql(string sql, IResolveDuplication col)
        {
            string colname = RDMPQuerySyntaxHelper.EnsureValueIsWrapped(col.GetRuntimeName(LoadStage.AdjustRaw));

            string direction = col.DuplicateRecordResolutionIsAscending ? " ASC" : " DESC";

            //dont bother adding these because they are hic generated
            if (colname.StartsWith("hic_"))
                return sql;

            RDMPQuerySyntaxHelper.ValueType valueType = RDMPQuerySyntaxHelper.GetDataType(col.Data_type);

            if (valueType == RDMPQuerySyntaxHelper.ValueType.CharacterString)
            {
                //character strings are compared first by LENGTH (to prefer longer data)
                //then by alphabetical comparison to prefer things towards the start of the alphabet (because this makes sense?!)
                return 
                    sql +
                    "LEN(ISNULL(" + colname + "," + RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType(col.Data_type, true) + "))" + direction + "," + Environment.NewLine +
                    "ISNULL(" + colname + "," + RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType(col.Data_type, true) + ")" + direction + "," + Environment.NewLine;
            }

            return sql + "ISNULL(" + colname + "," + RDMPQuerySyntaxHelper.GetNullSubstituteForComparisonsWithDataType(col.Data_type, true) + ")" + direction + "," + Environment.NewLine;
        }

        public string GenerateCollisionDetectionSQL()
        {
            string tableNameInRAW = GetTableName();
            var pks = _tableInfo.ColumnInfos.Where(col => col.IsPrimaryKey).ToArray();

            string sql = "";
            sql += "select case when exists(" + Environment.NewLine;
            sql += "select 1 FROM" + Environment.NewLine;
            sql += tableNameInRAW + Environment.NewLine;
            sql += "group by " + pks.Aggregate("", (s, n) => s + RDMPQuerySyntaxHelper.EnsureValueIsWrapped(n.GetRuntimeName(LoadStage.AdjustRaw)) + ",") + Environment.NewLine;
            sql = sql.TrimEnd(new[] {',','\r','\n'}) + Environment.NewLine;
            sql += "having count(*) > 1" + Environment.NewLine;
            sql += ") then 1 else 0 end" + Environment.NewLine;

            return sql;
        }

        public string GeneratePreviewSQL()
        {
            
            ColumnInfo[] pks;
            List<IResolveDuplication> resolvers;
            string basicSQL = GenerateSQL(out pks, out resolvers);

            string commaSeparatedPKs = string.Join(",", pks.Select(c => RDMPQuerySyntaxHelper.EnsureValueIsWrapped(c.GetRuntimeName(LoadStage.AdjustRaw))));
            string commaSeparatedCols = string.Join(",", resolvers.Select(c => RDMPQuerySyntaxHelper.EnsureValueIsWrapped(c.GetRuntimeName(LoadStage.AdjustRaw))));

            //add all the columns to the WITH CTE bit
            basicSQL = basicSQL.Replace(WithCTE,"WITH CTE (" + commaSeparatedPKs + "," + commaSeparatedCols + ",DuplicateCount)");
            basicSQL = basicSQL.Replace(SelectRownum, "\t SELECT " + commaSeparatedPKs + "," + commaSeparatedCols + ",ROW_NUMBER()");
            basicSQL = basicSQL.Replace(DeleteBit, "");

            basicSQL += "select" + Environment.NewLine;
            basicSQL += "\tCase when DuplicateCount = 1 then 'Retained' else 'Deleted' end as PlannedOperation,*" + Environment.NewLine;
            basicSQL += "FROM CTE" + Environment.NewLine;
            basicSQL += "where" + Environment.NewLine;
            basicSQL += "exists" + Environment.NewLine;
            basicSQL += "(" + Environment.NewLine;
            basicSQL += "\tselect 1" + Environment.NewLine;
            basicSQL += "\tfrom" + Environment.NewLine;
            basicSQL += "\t\t"+ GetTableName() + " child" + Environment.NewLine;
            basicSQL += "\twhere" + Environment.NewLine;

            //add the child.pk1 = CTE.pk1 bit to restrict preview only to rows that are going to get compared for nukage
            basicSQL += string.Join("\r\n\t\tand",pks.Select(pk =>  ("\t\tchild." + RDMPQuerySyntaxHelper.EnsureValueIsWrapped(pk.GetRuntimeName(LoadStage.AdjustRaw)) + "= CTE." + RDMPQuerySyntaxHelper.EnsureValueIsWrapped(pk.GetRuntimeName(LoadStage.AdjustRaw)))));

            basicSQL += "\tgroup by" + Environment.NewLine;
            basicSQL += string.Join(",\r\n", pks.Select( pk => "\t\t" + RDMPQuerySyntaxHelper.EnsureValueIsWrapped(pk.GetRuntimeName(LoadStage.AdjustRaw))));

            basicSQL += "\t\t" + Environment.NewLine;
            basicSQL += "\thaving count(*)>1" + Environment.NewLine;
            basicSQL += ")" + Environment.NewLine;

            basicSQL += "order by " + string.Join(",\r\n", pks.Select(pk => RDMPQuerySyntaxHelper.EnsureValueIsWrapped(pk.GetRuntimeName(LoadStage.AdjustRaw))));
            basicSQL += ",DuplicateCount";

            return basicSQL;
        }
    }
}

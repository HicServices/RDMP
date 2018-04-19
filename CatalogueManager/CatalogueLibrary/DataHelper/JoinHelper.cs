using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.DataHelper
{
    /// <summary>
    /// Generates ANSI Sql for joining tables together in the FROM line of an SQL query
    /// </summary>
    public class JoinHelper
    {
        
        #region ways to build up the JOIN Sql
        /// <summary>
        /// Static version lets you preview what the Lookup will look like without actually having to 
        /// create one ( note that this method will not let you view Supplemental joins, these require 
        /// GetJoinSQL(IJoin) to be used instead
        /// </summary>
        /// <returns></returns>
        public static string GetJoinSQL(ColumnInfo ForeignKey, ColumnInfo PrimaryKey, ExtractionJoinType? type, string Collation)
        {

            TableInfo fkTable = null;
            if (ForeignKey != null)
                fkTable = ForeignKey.TableInfo;

            TableInfo pkTable = null;
            if (PrimaryKey != null)
                pkTable = PrimaryKey.TableInfo;
            
            string foreignTable = fkTable == null ? "" : fkTable.Name;
            string primaryTable = pkTable == null ? "" : pkTable.Name;

            string key1 = ForeignKey == null ? "" : ForeignKey.Name;
            string key2 = PrimaryKey == null ? "" : PrimaryKey.Name;

            string joinType = "";

            if (type != null)
                joinType = type.ToString();

            string SQL = null;
            
            SQL = foreignTable + " " + joinType + " JOIN " + primaryTable + " ON " + key1 + " = " + key2;

            SQL = AppendCollation(SQL, Collation);
            
            return SQL;
        }

        public static string GetJoinSQL(IJoin join)
        {
            string SQL = GetJoinSQL(join.ForeignKey, join.PrimaryKey, join.ExtractionJoinType, join.Collation);

            SQL = AppendSupplementalJoins(SQL, join);
            
            return SQL;
        }

        /// <summary>
        /// Returns the first half of the join with an inverted join type
        /// 
        /// <para>Explanation:joins are defined as FK table JOIN_TYPE PK table so if you are requesting a join to the FK table it is assumed you are coming from the pk table therefore the join type is INVERTED i.e. LEFT becomes RIGHT</para>
        /// 
        /// </summary>
        /// <param name="join"></param>
        /// <returns></returns>
        public static string GetJoinSQLForeignKeySideOnly(IJoin join)
        {
            TableInfo fkTable = null;
            if (join.ForeignKey != null)
                fkTable = join.ForeignKey.TableInfo;

            string foreignTable = fkTable == null ? "" : fkTable.Name;

            string key1 = join.ForeignKey == null ? "" : join.ForeignKey.Name;
            string key2 = join.PrimaryKey == null ? "" : join.PrimaryKey.Name;

            string SQL = " " + join.GetInvertedJoinType() + " JOIN " + foreignTable + " ON " + key1 + " = " + key2;

            SQL = AppendCollation(SQL, join);
            SQL = AppendSupplementalJoins(SQL, join);


            return SQL;
        }

 

        /// <summary>
        /// Gets the JOIN Sql for the JoinInfo as foreign key JOIN primary key on fk.col1 = pk.col2.  Pass in a number
        /// in order to have the primary key table be assigned an alias e.g. 1 to give it t1
        /// 
        /// <para>Because join type refers to FK join PK and you are requesting "X" + " JOIN PK table on x" then the join is inverted e.g. LEFT => RIGHT and RIGHT => LEFT
        /// unless it is a lookup join which is always LEFT</para>
        /// </summary>
        /// <param name="aliasNumber"></param>
        /// <returns></returns>
        public static string GetJoinSQLPrimaryKeySideOnly(IJoin join, int aliasNumber = -1)
        {
            TableInfo pkTable = null;
            if (join.PrimaryKey != null)
                pkTable = join.PrimaryKey.TableInfo;

            string primaryTable = pkTable == null ? "" : pkTable.Name;

            //null check... could be required for display purposes where you have set up half the join when this is called
            string key1 = join.ForeignKey == null ? "" : join.ForeignKey.Name;
            string key2 = join.PrimaryKey == null ? "" : join.PrimaryKey.Name;

            string toReturn = "";

            //The lookup table is not being assigned an alias
            if (aliasNumber == -1)
                toReturn = " " + join.ExtractionJoinType + " JOIN " + primaryTable + " ON " + key1 + " = " + key2;
            else
            {
                //the lookup table IS being assigned an alias so append As X after table name and change key2 of the join to X.col instead of tablename.col
                toReturn = " " + join.ExtractionJoinType + " JOIN " + primaryTable
                           + GetLookupTableAlias(aliasNumber, true) +
                           " ON " + key1 + " = " + key2.Replace(pkTable.Name, GetLookupTableAlias(aliasNumber));
                
            }

            toReturn = AppendCollation(toReturn, join);
            toReturn = AppendSupplementalJoins(toReturn, join, aliasNumber);
            
            return toReturn;
        }
        #endregion

        /// <summary>
        /// Gets the suffix for a given lookup table number
        /// </summary>
        /// <param name="aliasNumber">the lookup number e.g. 1 gives lookup_1</param>
        /// <param name="requirePrefix">pass in true if you require the prefix " AS " (may vary depending on database context in future e.g. perhaps MySQL refers to tables by different alias syntax)</param>
        /// <returns></returns>
        public static string GetLookupTableAlias(int aliasNumber, bool requirePrefix = false)
        {
            if (requirePrefix)
                return " AS " + "lookup_" + aliasNumber;

            return "lookup_" + aliasNumber;

        }


        [Pure]
        private static string AppendSupplementalJoins(string sql, IJoin join, int aliasNumber = -1)
        {
            
            IEnumerable<ISupplementalJoin> supplementalJoins = join.GetSupplementalJoins();

            if (supplementalJoins != null)
                foreach (ISupplementalJoin supplementalJoin in supplementalJoins)
                {
                    string rightHalf = supplementalJoin.PrimaryKey.ToString();

                    if (aliasNumber != -1)
                    {
                        TableInfo lookupTable = join.PrimaryKey.TableInfo;
                        rightHalf = rightHalf.Replace(lookupTable.Name, GetLookupTableAlias(aliasNumber));
                    }

                    sql += " AND " + supplementalJoin.ForeignKey + " = " + rightHalf;
                    sql = AppendCollation(sql, supplementalJoin);
                }

        

            return sql;
        }


        [Pure]
        private static string AppendCollation(string sql, ISupplementalJoin supplementalJoin)
        {
            return AppendCollation(sql, supplementalJoin.Collation);
        }

        [Pure]
        private static string AppendCollation(string sql, IJoin join)
        {
            return AppendCollation(sql, join.Collation);
        }

        [Pure]
        private static string AppendCollation(string sql, string collation)
        {
            if (!string.IsNullOrWhiteSpace(collation))
                return sql + " collate " + collation;

            return sql;
        }
    }
}

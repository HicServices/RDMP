using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueLibrary.QueryBuilding
{
    /// <summary>
    /// The SELECT portion of QueryBuilder is built up via AddColumn which takes an IColumn.  Each IColumn is a single line of SELECT Sql which might be as
    /// simple as the name of a column but might be a method with an alias or even a count e.g. 'sum(distinct mycol) as Total'.  These IColumns are wrapped by
    /// QueryTimeColumn which is a wrapper for IColumn which is gradually populated with facts discovered during QueryBuilding such as whether it is from a Lookup 
    /// Table, whether it maps to an underlying ColumnInfo etc.  These facts are used later on by QueryBuilder to decide which tables/joins are needed in the FROM 
    /// section of the query etc
    /// </summary>
    public class QueryTimeColumn: IComparable
    {
        public bool IsIsolatedLookupDescription { get; set; }
        public bool IsLookupForeignKey { get; private set; }
        public bool IsLookupDescription { get; private set; }
        public int LookupTableAlias { get; private set; }
        public Lookup LookupTable { get; private set; }

        public IColumn IColumn { get; set; }
        public ColumnInfo UnderlyingColumn { get; set; }


        public QueryTimeColumn(IColumn column)
        {
            IColumn = column;
            UnderlyingColumn = column.ColumnInfo;
        }
        
        public override int GetHashCode()
        {
            if (IColumn == null)
                return -1;

            return IColumn.ID;
        }

        public override bool Equals(object obj)
            {
                if (obj is QueryTimeColumn == false)
                    throw new Exception(".Equals only works for objects of type QueryTimeColumn");

                return
                    (obj as QueryTimeColumn).IColumn.ID == this.IColumn.ID
                    && IColumn.ID != -1;
            }

            public int CompareTo(object obj)
            {
                if (obj is QueryTimeColumn)
                {
                    return this.IColumn.Order -
                           (obj as QueryTimeColumn).IColumn.Order;
                }

                return 0;
            }

            public static void SetLookupStatus(QueryTimeColumn[] ColumnsInOrder, List<TableInfo> tablesUsedInQuery)
            {

                ColumnInfo lastForeignKeyFound = null;
                int lookupTablesFound = 0;

                var firstTable = tablesUsedInQuery.FirstOrDefault();

                var allAvailableLookups = new Lookup[0]; 

                if(firstTable != null)
                    allAvailableLookups = firstTable.Repository.GetAllObjects<Lookup>();
                
                for (int i = 0; i < ColumnsInOrder.Count(); i++)
                {
                    //it is a custom column
                    if (ColumnsInOrder[i].UnderlyingColumn == null)
                        continue;

                    Lookup[] foreignKeyLookupInvolvement = allAvailableLookups.Where(l=>l.ForeignKey_ID == ColumnsInOrder[i].UnderlyingColumn.ID).ToArray();
                    Lookup[] lookupDescriptionInvolvement = allAvailableLookups.Where(l=>l.Description_ID == ColumnsInOrder[i].UnderlyingColumn.ID).ToArray();

                    if (Lookup.CountUniquePrimaryKeyTablesInLookupCollection(foreignKeyLookupInvolvement) > 1)
                        throw new Exception("Column " + ColumnsInOrder[i].UnderlyingColumn + " is configured as a foreign key for multiple different Lookup tables");

                    if (foreignKeyLookupInvolvement.Length > 0)
                    {
                        if (lookupDescriptionInvolvement.Length > 0)
                            throw new QueryBuildingException("Column " + ColumnsInOrder[i].UnderlyingColumn + " is both a Lookup.ForeignKey and a Lookup.Description");


                        lastForeignKeyFound = ColumnsInOrder[i].UnderlyingColumn;
                        ColumnsInOrder[i].IsLookupForeignKey = true;
                        ColumnsInOrder[i].IsLookupDescription = false;
                        ColumnsInOrder[i].LookupTableAlias = ++lookupTablesFound;
                        ColumnsInOrder[i].LookupTable = foreignKeyLookupInvolvement[0];
                    }

                    if (lookupDescriptionInvolvement.Length > 0)
                    {
                        bool lookupDescriptionIsIsolated = false;

                        //we have not found any foreign keys yet thats a problem
                        if (lastForeignKeyFound == null)
                        {
                            var potentialWinners =
                                lookupDescriptionInvolvement.Where(
                                    l => tablesUsedInQuery.Any(t => t.ID == l.ForeignKey.TableInfo_ID)).ToArray();

                            if(potentialWinners.Length == 1)//or there are many options but only one which is in our existing table collection
                            {
                                lastForeignKeyFound = potentialWinners[0].ForeignKey;//use it there aren't multiple foreign keys to pick from (which would result in uncertainty)
                                lookupDescriptionIsIsolated = true;
                            }
                            else
                                //otherwise there are multiple foreign keys for this description and the user has not put in a foreign key to let us choose the correct one
                                throw new QueryBuildingException("Found lookup description before encountering any lookup foreign keys (Description column was " + ColumnsInOrder[i].UnderlyingColumn + ") - make sure you always order Descriptions after their Foreign key and ensure they are in a contiguous block");
                        }

                        Lookup[] correctLookupDescriptionInvolvement = lookupDescriptionInvolvement.Where(lookup => lookup.ForeignKey.ID == lastForeignKeyFound.ID).ToArray();

                        if (correctLookupDescriptionInvolvement.Length == 0)
                        {
                            //so there are no compatible foreign keys or the columns are a jumbled mess

                            //either way the last seen fk (or guessed fk) isn't right.  So what fks could potentially be used with the Column?
                            var probableCorrectColumn = lookupDescriptionInvolvement.Where(
                                l =>
                                    //any lookup where there is...
                                    ColumnsInOrder.Any(
                                        qtc =>
                                            //a column with an ID equal to the fk 
                                            qtc.UnderlyingColumn != null && qtc.UnderlyingColumn.ID == l.ForeignKey_ID)).ToArray();


                            string suggestions = "";
                            if (probableCorrectColumn.Any())
                                suggestions = "Possible foreign keys include:" + string.Join(",", probableCorrectColumn.Select(l => l.ForeignKey));

                            throw new QueryBuildingException(
                                "Encountered Lookup Description Column (" + ColumnsInOrder[i].IColumn + ") after first encountering Foreign Key (" + lastForeignKeyFound + ").  Lookup description columns (_Desc) must come after the associated Foreign key." + suggestions );
                        }

                        if (correctLookupDescriptionInvolvement.Length > 1)
                            throw new QueryBuildingException("Lookup description " + ColumnsInOrder[i].UnderlyingColumn + " appears to be configured as a Lookup Description twice with the same Lookup Table");

                        ColumnsInOrder[i].IsIsolatedLookupDescription = lookupDescriptionIsIsolated;
                        ColumnsInOrder[i].IsLookupForeignKey = false;
                        ColumnsInOrder[i].IsLookupDescription = true;
                        ColumnsInOrder[i].LookupTableAlias = lookupTablesFound; // must belong to same one as previously encountered foreign key
                        ColumnsInOrder[i].LookupTable = correctLookupDescriptionInvolvement[0];

                        //see if there are any supplemental joins to tables that are not involved in the query
                        IEnumerable<ISupplementalJoin> supplementalJoins = correctLookupDescriptionInvolvement[0].GetSupplementalJoins();

                        if (supplementalJoins != null)
                            foreach (ISupplementalJoin supplementalJoin in supplementalJoins)
                                if (!tablesUsedInQuery.Any(t => t.ID == supplementalJoin.ForeignKey.TableInfo_ID))
                                    throw new QueryBuildingException("Lookup requires supplemental join to column " + supplementalJoin.ForeignKey + " which is contained in a table that is not part of the SELECT column collection");
                    }
                }
            }

      
        public string GetSelectSQL(string hashingPattern, string salt,IQuerySyntaxHelper syntaxHelper)
        {
            string toReturn = this.IColumn.SelectSQL;

            //deal with hashing
            if (string.IsNullOrWhiteSpace(salt) == false && this.IColumn.HashOnDataRelease)
            {
                if (string.IsNullOrWhiteSpace(this.IColumn.Alias))
                    throw new ArgumentException("IExtractableColumn " + this.IColumn + " is missing an Alias (required for hashing)");

                if(hashingPattern == null)
                    throw new Exception("Hashing Pattern is null but column is marked for HashOnDataRelease");
                    
                toReturn = SqlSyntaxHelper.WrapStringWithHashingAlgorithm(hashingPattern,toReturn, salt);
            }

            // the SELECT SQL may span multiple lines, so collapse it to a single line cleaning up any whitespace issues, e.g. to avoid double spaces in the collapsed version
            var trimmedSelectSQL =
                toReturn.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());
            toReturn = string.Join(" ", trimmedSelectSQL);
                
            //append alias to the end of the line if there is an alias
            if (!string.IsNullOrWhiteSpace(this.IColumn.Alias))
                toReturn += syntaxHelper.AliasPrefix + this.IColumn.Alias.Trim();

            //cannot be both, we check for this earlier (see SetLookupStatus)
            Debug.Assert(!(IsLookupDescription && IsLookupForeignKey));

            //replace table name with table alias if it is a LookupDescription
            if (IsLookupDescription)
            {
                string tableName = LookupTable.PrimaryKey.TableInfo.Name;

                if (!toReturn.Contains(tableName))
                    throw new Exception("Column \"" + toReturn + "\" is a Lookup Description but it's SELECT SQL does not include the Lookup table name \"" + tableName + "\"");

                toReturn = toReturn.Replace(tableName, JoinHelper.GetLookupTableAlias(LookupTableAlias));
            }

            //actually dont need to do anything special for LookupForeignKeys

            return toReturn;
        }



        public void CheckSyntax()
            {
                //make sure to only throw SyntaxErrorException errors in here
                try
                {
                    RDMPQuerySyntaxHelper.CheckSyntax(IColumn);
                    string runtimeName = IColumn.GetRuntimeName();

                    if (string.IsNullOrWhiteSpace(runtimeName))
                        throw new SyntaxErrorException("no runtime name");

                }
                catch (SyntaxErrorException exception)
                {
                    throw new SyntaxErrorException("Syntax failure on IExtractableColumn with SelectSQL=\"" + IColumn.SelectSQL + "\"", exception);
                }
            }

        public bool IsLookupForeignKeyActuallyUsed(List<QueryTimeColumn> selectColumns)
        {
            if (!IsLookupForeignKey)
                return false;

            //see if the description is used anywhere in the actual query columns!
            return selectColumns.Any(c => c.IsLookupDescription && c.LookupTable.ID == this.LookupTable.ID);
        }
    }
}

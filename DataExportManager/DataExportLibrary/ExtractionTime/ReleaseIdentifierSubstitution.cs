using System;
using System.Text.RegularExpressions;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using DataExportLibrary.Interfaces.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.ExtractionTime
{
    /// <summary>
    /// Records how (via SQL) replace the private patient identifier column (e.g. CHI) with the release identifier (e.g. swap [biochemistry]..[chi] for 
    /// [cohort]..[ReleaseId]).  Also includes the Join SQL string for linking the cohort table (which contains the ReleaseId e.g. [cohort]) with the dataset
    /// table (e.g. [biochemistry]). 
    /// 
    /// <para>This class is an IColumn and is designed to be added as a new Column to a QueryBuilder as normal (See ExtractionQueryBuilder)</para>
    /// </summary>
    public class ReleaseIdentifierSubstitution : IColumn
    {
        public string JoinSQL { get; private set; }
        public IColumn OriginalDatasetColumn;

        [Sql]
        public string SelectSQL { get; set; }

        public string Alias { get; private set; }
        
        //all these are hard coded to null or false really
        public ColumnInfo ColumnInfo
        {
            get { return OriginalDatasetColumn.ColumnInfo; }
        }
        public int Order
        {
            get { return OriginalDatasetColumn.Order; }
            set { }
        }
        public int ID { get { return -1; } }
        public bool HashOnDataRelease { get { return false; } }
        public bool IsExtractionIdentifier { get { return true; } }
        public bool IsPrimaryKey { get { return false; } }

        public ReleaseIdentifierSubstitution(IColumn extractionIdentifierToSubFor, IExtractableCohort extractableCohort, bool isPartOfMultiCHISubstitution)
        {
            if(!extractionIdentifierToSubFor.IsExtractionIdentifier)
                throw new Exception("Column " + extractionIdentifierToSubFor + " is not marked IsExtractionIdentifier so cannot be substituted for a ReleaseIdentifier");
            
            OriginalDatasetColumn = extractionIdentifierToSubFor;
            
            if(OriginalDatasetColumn.ColumnInfo == null)
                throw new Exception("The column " + OriginalDatasetColumn.GetRuntimeName() + " references a ColumnInfo that has been deleted");

            var syntaxHelper = extractableCohort.GetQuerySyntaxHelper();

            //the externally referenced Cohort table
            var externalCohortTable = extractableCohort.ExternalCohortTable;
            
            var privateIdentifierFieldDiscovered = externalCohortTable.Discover().ExpectTable(externalCohortTable.TableName).DiscoverColumn(externalCohortTable.PrivateIdentifierField);

            string collateStatement = "";
            
            //the release identifier join might require collation

            //if the private has a collation
            if (!string.IsNullOrWhiteSpace(privateIdentifierFieldDiscovered.Collation))
            {
                var cohortCollation = privateIdentifierFieldDiscovered.Collation;
                var otherTableCollation = OriginalDatasetColumn.ColumnInfo.Collation;

                //if the other column has a collation and it is different than this one
                if (!string.IsNullOrWhiteSpace(otherTableCollation) && !string.Equals(cohortCollation, otherTableCollation))
                {
                    collateStatement = " collate " + cohortCollation;
                }
            }


            if (!isPartOfMultiCHISubstitution)
            {
                SelectSQL = extractableCohort.GetReleaseIdentifier();
                Alias = syntaxHelper.GetRuntimeName(SelectSQL);
            }
            else
            {
                SelectSQL = "(SELECT DISTINCT " +
                    extractableCohort.GetReleaseIdentifier() + 
                    " FROM " +
                    externalCohortTable.TableName + " WHERE " + extractableCohort.WhereSQL() + " AND " + externalCohortTable.PrivateIdentifierField + "=" + OriginalDatasetColumn.SelectSQL + collateStatement +")";
                
                if(!string.IsNullOrWhiteSpace(OriginalDatasetColumn.Alias))
                {
                    string toReplace = extractableCohort.GetPrivateIdentifier(true);
                    string toReplaceWith = extractableCohort.GetReleaseIdentifier(true);

                    //take the same name as the underlying column
                    Alias = OriginalDatasetColumn.Alias;

                    //but replace all instances of CHI with PROCHI (or Barcode, or whatever)
                    if(!Alias.Contains(toReplace) || Regex.Matches(Alias,Regex.Escape(toReplace)).Count > 1)
                        throw new Exception("Expected OriginalDatasetColumn " + OriginalDatasetColumn.Alias + " to have the text \"" + toReplace + "\" appearing once (and only once in it's name)," +
                                            "we planned to replace that text with:" + toReplaceWith);

                   
                   Alias = Alias.Replace(toReplace,toReplaceWith);
                }
                else
                    throw new Exception("In cases where you have multiple columns marked IsExtractionIdentifier, they must all have Aliases, the column " + OriginalDatasetColumn.SelectSQL + " does not have one");
            }

            JoinSQL = OriginalDatasetColumn.SelectSQL + "=" + externalCohortTable.PrivateIdentifierField + collateStatement;

        }

        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }

        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }
}

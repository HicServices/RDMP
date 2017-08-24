using System;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode;

namespace DataExportLibrary.ExtractionTime
{
    public class ReleaseIdentifierSubstitution : IColumn
    {
        public string JoinSQL { get; private set; }
        public IColumn OriginalDatasetColumn;
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

            //the externally referenced Cohort table
            var externalCohortTable = extractableCohort.ExternalCohortTable;

            if (!isPartOfMultiCHISubstitution)
            {
                SelectSQL = extractableCohort.GetReleaseIdentifier();
                Alias = SqlSyntaxHelper.GetRuntimeName(SelectSQL);
            }
            else
            {
                SelectSQL = "(SELECT DISTINCT " +
                    extractableCohort.GetReleaseIdentifier() + 
                    " FROM " +
                    externalCohortTable.TableName + " WHERE " + extractableCohort.WhereSQL() + " AND " + externalCohortTable.PrivateIdentifierField + "=" + OriginalDatasetColumn.SelectSQL + " collate Latin1_General_BIN)";
                
                if(!string.IsNullOrWhiteSpace(OriginalDatasetColumn.Alias))
                {

                    string toReplace = SqlSyntaxHelper.GetRuntimeName(externalCohortTable.PrivateIdentifierField);
                    string toReplaceWith = SqlSyntaxHelper.GetRuntimeName(extractableCohort.GetReleaseIdentifier());

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

            //the release identifier join might require collation
            string collationStatement = "";
            
            if(OriginalDatasetColumn.ColumnInfo == null)
                throw new Exception("The column " + OriginalDatasetColumn.GetRuntimeName() + " references a ColumnInfo that has been deleted");

            //if we know the original dataset columns datatype
            if (OriginalDatasetColumn.ColumnInfo.Data_type != null &&
                OriginalDatasetColumn.ColumnInfo.Data_type.ToLower().Contains("char"))//and it is a character based datatype
                collationStatement = " collate Latin1_General_BIN";//collate it, bit hacky but still better than before which always collated!

            JoinSQL = OriginalDatasetColumn.SelectSQL + "=" + externalCohortTable.PrivateIdentifierField + collationStatement;

        }

        public string GetRuntimeName()
        {
            return RDMPQuerySyntaxHelper.GetRuntimeName(this);
        }
    }
}

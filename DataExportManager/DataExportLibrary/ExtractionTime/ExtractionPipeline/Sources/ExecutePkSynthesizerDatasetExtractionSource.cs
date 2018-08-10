using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using DataExportLibrary.ExtractionTime.Commands;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    /// <summary>
    /// Extraction source which creates a PrimaryKey on the DataTable being extracted.  This is based on <see cref="CatalogueLibrary.Data.ExtractionInformation.IsPrimaryKey"/> of the
    /// columns extracted and is not garuanteed to actually be unique (depending on how you have configured the flags).  
    /// 
    /// <para>The primary use case for this is when extracting to database where you want to have meaningful primary keys</para>
    /// </summary>
    public class ExecutePkSynthesizerDatasetExtractionSource : ExecuteDatasetExtractionSource
    {
        private const string SYNTH_PK_COLUMN = "SynthesizedPk";
        private bool _synthesizePkCol = false;
        public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        {
            // let's look for primary keys in the Extraction Information
            var allPrimaryKeys = Request.ColumnsToExtract.Union(Request.ReleaseIdentifierSubstitutions).Where(col => col.IsPrimaryKey).ToList();

            // if there are some they will be marked in the "GetChunk".
            // If there are none, then we need to synth a new column here.
            if (!allPrimaryKeys.Any())
            {
                var primaryKeys = GetProperTables().SelectMany(GetPrimaryKeys).ToList();

                if (primaryKeys.Any())
                {
                    string newSql;
                    if (primaryKeys.Count > 1) // no need to do anything if there is only one.
                        newSql = "CONCAT(" + String.Join(",'_',", primaryKeys.Select(apk => apk.ToString())) + ")";
                    else
                        newSql = primaryKeys.First().ToString();

                    Request.QueryBuilder.AddColumn(new SpontaneouslyInventedColumn(SYNTH_PK_COLUMN, newSql)
                    {
                        HashOnDataRelease = true,
                        IsPrimaryKey = true,
                        Order = -1,
                    });
                    _synthesizePkCol = true;
                }
            }

            return Request.QueryBuilder.SQL;
        }

        private IEnumerable<ColumnInfo> GetPrimaryKeys(TableInfo t)
        {
            return t.ColumnInfos.Where(ci => ci.IsPrimaryKey);
        }

        private IEnumerable<TableInfo> GetProperTables()
        {
            if(Request.QueryBuilder.SQLOutOfDate)
                Request.QueryBuilder.RegenerateSQL();

            return Request.QueryBuilder.TablesUsedInQuery.Where(ti => !ti.IsLookupTable());
        }

        public override DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var chunk = base.GetChunk(listener, cancellationToken);
            if (GlobalsRequest != null)
                return chunk;

            if (chunk == null)
                return null;

            var allPrimaryKeys = Request.ColumnsToExtract.Union(Request.ReleaseIdentifierSubstitutions).Where(col => col.IsPrimaryKey).Select(c=>c.GetRuntimeName()).ToList();
            if (allPrimaryKeys.Any())
                chunk.PrimaryKey = chunk.Columns.Cast<DataColumn>().Where(c=>allPrimaryKeys.Contains(c.ColumnName,StringComparer.CurrentCultureIgnoreCase)).ToArray();
            else 
                if (_synthesizePkCol)
                    chunk.PrimaryKey = new[] { chunk.Columns[SYNTH_PK_COLUMN] };
                
            return chunk;
        }

        public override void Check(ICheckNotifier notifier)
        {
            base.Check(notifier);
            if (Request == null || Request == ExtractDatasetCommand.EmptyCommand) // it is the globals, and there is no PK involved in there... although there should be...
                return;

            var allPrimaryKeys = Request.ColumnsToExtract.Union(Request.ReleaseIdentifierSubstitutions).Where(col => col.IsPrimaryKey).Select(c => c.GetRuntimeName()).ToList();
            string message;
            CheckResult check;
            if (!allPrimaryKeys.Any())
            {
                message = Request.SelectedDataSets +
                          ": No catalogue item is flagged as primary key";

                var primaryKeys = GetProperTables().SelectMany(GetPrimaryKeys).ToList();

                if (primaryKeys.Any())
                {
                    message += ", but I found these underlying columns: " +
                               String.Join(",", primaryKeys.Select(apk => apk.ToString())) +
                               ". Will concatenate and hash values into new PK column: " + SYNTH_PK_COLUMN;
                    check = CheckResult.Success;
                }
                else
                {
                    message += " and I found no pk columns in the underlying table(s). Resulting extraction will not have any PKs.";
                    check = CheckResult.Warning;
                }
            }
            else
            {
                message = Request.SelectedDataSets + ": Found primary key catalogue items: " + String.Join(",", allPrimaryKeys) + 
                          ". These will become PK columns in the resulting extraction.";
                check = CheckResult.Success;
            }

            notifier.OnCheckPerformed(new CheckEventArgs(message, check));
        }
    }
} 
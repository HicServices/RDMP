using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Spontaneous;
using DataExportLibrary.ExtractionTime.Commands;
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

        protected override void Initialize(ExtractDatasetCommand request)
        {
            base.Initialize(request);
            if (request == ExtractDatasetCommand.EmptyCommand)
                return;

            request.QueryBuilder.RegenerateSQL();

            // let's look for primary keys in the Extraction Information
            var allPrimaryKeys = request.ColumnsToExtract.Union(request.ReleaseIdentifierSubstitutions).Where(col => col.IsPrimaryKey).ToList();
            
            // if there are some they will be marked in the "GetChunk".
            // If there are none, then we need to synth a new column here.
            if (!allPrimaryKeys.Any() && request.QueryBuilder.TablesUsedInQuery.Count == 1) // EASY mode
            {
                var table = Request.QueryBuilder.TablesUsedInQuery.First();
                var primaryKeys = table.ColumnInfos.Where(ci => ci.IsPrimaryKey).ToList();
                if (primaryKeys.Any())
                {
                    string newSql;
                    if (primaryKeys.Count > 1) // no need to do anything if there is only one.
                        newSql = "CONCAT(" + String.Join(",'_',", primaryKeys.Select(apk => apk.ToString())) + ")";
                    else
                        newSql = primaryKeys.First().ToString();

                    request.QueryBuilder.AddColumn(new SpontaneouslyInventedColumn(SYNTH_PK_COLUMN, newSql)
                    {
                        HashOnDataRelease = true,
                        IsPrimaryKey = true,
                        Order = -1,
                    });
                    _synthesizePkCol = true;
                }
            }
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
    }
}
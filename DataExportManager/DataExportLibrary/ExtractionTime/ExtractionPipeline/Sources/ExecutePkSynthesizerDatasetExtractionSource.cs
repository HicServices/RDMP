using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.QueryBuilding;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Spontaneous;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources
{
    public class ExecutePkSynthesizerDatasetExtractionSource : ExecuteDatasetExtractionSource
    {
        protected override void Initialize(ExtractDatasetCommand request)
        {
            base.Initialize(request);
            request.QueryBuilder.RegenerateSQL();

            if (request.QueryBuilder.TablesUsedInQuery.Count == 1) // EASY mode
            {
                var table = Request.QueryBuilder.TablesUsedInQuery.First();
                var primaryKeys = table.ColumnInfos.Where(ci => ci.IsPrimaryKey).ToList();

                var primaryKeySQL = String.Join(",'_',", primaryKeys);

                var newSql = "CONCAT(" + primaryKeySQL + ")";
                
                request.QueryBuilder.AddColumn(new SpontaneousColumn()
                {
                    Alias = "SynthesizedPk",
                    HashOnDataRelease = true,
                    IsPrimaryKey = true,
                    Order = -1,
                    SelectSQL = newSql
                });
            }
        }

        public override DataTable GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            var chunk = base.GetChunk(listener, cancellationToken);
            //if (GlobalsRequest != null)
            //    return chunk;

            //if (chunk == null)
            //    return null;

            //if (Request.QueryBuilder.TablesUsedInQuery.Count == 1) // EASY mode
            //{
            //    chunk.PrimaryKey = new[] { chunk.Columns["SynthPK"] };
            //}

            return chunk;
        }

        public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        {
            //if (GlobalsRequest == null && Request.QueryBuilder.TablesUsedInQuery.Count == 1) // EASY mode
            //{
            //    var table = Request.QueryBuilder.TablesUsedInQuery.First();
            //    var primaryKeys = table.ColumnInfos.Where(ci => ci.IsPrimaryKey).ToList();

            //    var primaryKeySQL = String.Join(",'_',", primaryKeys);

            //    var insertPoint = sql.IndexOf("FROM", StringComparison.InvariantCultureIgnoreCase);

            //    var newSql = sql.Insert(insertPoint, ",CONCAT(" + primaryKeySQL + ") AS SynthPK ");
            //    return newSql;
            //}

            return base.HackExtractionSQL(sql, listener);
        }
    }
}
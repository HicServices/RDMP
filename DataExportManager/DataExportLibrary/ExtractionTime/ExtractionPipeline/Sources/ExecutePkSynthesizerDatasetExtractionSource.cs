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
        private const string SYNTH_PK_COLUMN = "SynthesizedPk";
        private bool _synthesizePkCol = false;

        protected override void Initialize(ExtractDatasetCommand request)
        {
            base.Initialize(request);
            request.QueryBuilder.RegenerateSQL();

            // let's look for primary keys in the Extraction Information
            var allPrimaryKeys = request.ColumnsToExtract.Union(request.ReleaseIdentifierSubstitutions).Where(col => col.IsPrimaryKey).ToList();
            if (allPrimaryKeys.Any())
            {
                string newSql;
                if (allPrimaryKeys.Count > 1)
                    newSql = "CONCAT(" + String.Join(",'_',", allPrimaryKeys.Select(apk => apk.SelectSQL)) + ")";
                else 
                    newSql = allPrimaryKeys.First().GetRuntimeName();

                request.QueryBuilder.AddColumn(new SpontaneousColumn()
                {
                    Alias = SYNTH_PK_COLUMN,
                    HashOnDataRelease = true,
                    IsPrimaryKey = true,
                    Order = -1,
                    SelectSQL = newSql
                });
                
                request.QueryBuilder.RegenerateSQL();
                _synthesizePkCol = true;
                return;
            }
            
            // no primary keys in Extraction Informations, get them from the ColumnInfos:
            if (request.QueryBuilder.TablesUsedInQuery.Count == 1) // EASY mode
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
                
                    request.QueryBuilder.AddColumn(new SpontaneousColumn()
                    {
                        Alias = SYNTH_PK_COLUMN,
                        HashOnDataRelease = true,
                        IsPrimaryKey = true,
                        Order = -1,
                        SelectSQL = newSql
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
            chunk.PrimaryKey = chunk.Columns.Cast<DataColumn>().Where(c=>allPrimaryKeys.Contains(c.ColumnName,StringComparer.CurrentCultureIgnoreCase)).ToArray();


            if (_synthesizePkCol)
                chunk.PrimaryKey = new[] {chunk.Columns[SYNTH_PK_COLUMN]};

            return chunk;
        }

        //public override string HackExtractionSQL(string sql, IDataLoadEventListener listener)
        //{
        //    //if (GlobalsRequest == null && Request.QueryBuilder.TablesUsedInQuery.Count == 1) // EASY mode
        //    //{
        //    //    var table = Request.QueryBuilder.TablesUsedInQuery.First();
        //    //    var primaryKeys = table.ColumnInfos.Where(ci => ci.IsPrimaryKey).ToList();

        //    //    var primaryKeySQL = String.Join(",'_',", primaryKeys);

        //    //    var insertPoint = sql.IndexOf("FROM", StringComparison.InvariantCultureIgnoreCase);

        //    //    var newSql = sql.Insert(insertPoint, ",CONCAT(" + primaryKeySQL + ") AS SynthPK ");
        //    //    return newSql;
        //    //}

        //    return base.HackExtractionSQL(sql, listener);
        //}
    }
}
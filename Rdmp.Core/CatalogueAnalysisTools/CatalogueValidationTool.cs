using Org.BouncyCastle.Bcpg.OpenPgp;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.DataViewing;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rdmp.Core.CatalogueAnalysisTools;

public class CatalogueValidationTool
{
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly Catalogue _catalogue;
    private readonly ColumnInfo _timeColumn;
    private readonly ColumnInfo _pivotColumn;
    private RowPeeker _peeker = new();
    private Dictionary<string, Dictionary<string, ResultObject>> _results = new();


    private string ALL = "ALL";
    public CatalogueValidationTool(ICatalogueRepository catalogueRepository, Catalogue catalogue, ColumnInfo timeColumn, ColumnInfo pivotColumn)
    {
        _catalogueRepository = catalogueRepository;
        _catalogue = catalogue;
        _timeColumn = timeColumn;
        _pivotColumn = pivotColumn;
    }

    private class ResultObject
    {
        public int Count;
        public int Correct;
        public int Wrong;
        public int Missing;
        public int Invalid;
    }


    //Primary Constraint Table
    // ID / ColumnInfo_ID / ConstraintType / Result
    //ConstraintType is an enum ofalpha,alphanumeric,chi,date

    //Secondary Contraint Table
    // ID, ColumnInfo_ID, ConstraintType, ConfigurableValue, Result
    // constraint type is an enum ...

    private DataTable GetChunk(DbDataCommandDataFlowSource flowSource,string pivotCategory)
    {
        DataTable chunk = null;
        var cancellationToken = new GracefulCancellationToken();

        try
        {
            chunk = flowSource.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, cancellationToken);
            chunk = _peeker.AddPeekedRowsIfAny(chunk);
            if (chunk is not null)
            {
                foreach (DataRow row in chunk.Rows)
                {
                    int index = row.Table.Columns[_timeColumn.GetRuntimeName()].Ordinal;
                    if (DateTime.TryParse(row.ItemArray[index].ToString(), out DateTime timeValue))
                    {
                        var key = $"{timeValue.Year}-{timeValue.Month}";
                        if (_results[pivotCategory].TryGetValue(key,out ResultObject value))
                        {
                            value.Count++;//just doing counts to test
                            _results[pivotCategory][key] = value;
                        }
                        else
                        {
                            var newValue = new ResultObject() { Count = 1 };
                            _results[pivotCategory][key] = newValue;
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
            //TODO do something
        }
        
        return chunk;
    }

    private void GenerateReport(string pivotColumnValue)
    {
        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(_catalogue.GetAllExtractionInformation());
        if (pivotColumnValue != ALL)
        {
            qb.AddCustomLine($"{_pivotColumn.GetFullyQualifiedName()} = '{pivotColumnValue}'", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
        }
        qb.RegenerateSQL();
        var sql = qb.SQL;
        var flowSource = new DbDataCommandDataFlowSource(sql, $"Generating validation report for category {pivotColumnValue}", _catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing, false).Builder, 0)
        {
            BatchSize = 10000
        };
        if(!_results.TryGetValue(pivotColumnValue,out _))
        {
            _results[pivotColumnValue] = new Dictionary<string, ResultObject>();
        }
        var dataChunk = GetChunk(flowSource, pivotColumnValue);
        foreach (DataRow drtableOld in dataChunk.Rows)
        {

        }
        while (dataChunk is not null)
        {
            dataChunk = GetChunk(flowSource,pivotColumnValue);
        }
        Console.WriteLine(_results);
    }


    public void GenerateValidationReports()
    {
        GenerateReport(ALL);
        //todo for all values in pivot category
    }
}

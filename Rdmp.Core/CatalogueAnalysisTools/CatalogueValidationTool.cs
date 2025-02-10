using Amazon.Runtime;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.DataViewing;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Validation.Constraints.Primary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;

namespace Rdmp.Core.CatalogueAnalysisTools;

public class CatalogueValidationTool
{
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly Catalogue _catalogue;
    private readonly ColumnInfo _timeColumn;
    private readonly ColumnInfo _pivotColumn;
    private RowPeeker _peeker = new();
    private Dictionary<string, Dictionary<string, ResultObject>> _results = new();
    private Dictionary<string, PrimaryContraint> _primaryConstraints = new();
    private DQERepository _DQERepository;
    private readonly string ALL = "ALL";
    private readonly string CORRECT = "CORRECT";
    public CatalogueValidationTool(ICatalogueRepository catalogueRepository, Catalogue catalogue, ColumnInfo timeColumn, ColumnInfo pivotColumn)
    {
        _catalogueRepository = catalogueRepository;
        _DQERepository = new DQERepository(_catalogueRepository);
        _catalogue = catalogue;
        _timeColumn = timeColumn;
        _pivotColumn = pivotColumn;

        foreach (var columnInfo in catalogue.CatalogueItems.Select(c => c.ColumnInfo))
        {
            _primaryConstraints[columnInfo.GetRuntimeName()] = _DQERepository.GetAllObjectsWhere<PrimaryContraint>("ColumnInfo_ID", columnInfo.ID).FirstOrDefault();
        }
    }

    private class ResultObject
    {
        public int Correct=0;
        public int Wrong=0;
        public int Missing=0;
        public int Invalid=0;


        public void Increment(string attribute)
        {
            switch (attribute)
            {
                case "CORRECT":
                    Correct++;
                    break;
                case "WRONG":
                    Wrong++;
                    break;
                case "MISSING":
                    Missing++;
                    break;
                case "INVALID":
                    Invalid++;
                    break;
                default:
                    throw new Exception($"Unknown result attrubute {attribute}");
            }
        }
    }

    //Secondary Contraint Table
    // ID, ColumnInfo_ID, ConstraintType, ConfigurableValue, Result
    // constraint type is an enum ...

    private bool ValidConstraint(PrimaryConstraint pc, object value)
    {
        if (value is DBNull) return true;
        return pc.Validate(value) ==null;
    }

    private ResultObject EvaulateRow(DataRow row, ResultObject existingResultObject)
    {
        foreach (var primaryContraint in _primaryConstraints.Keys)
        {
            var rowIndex = row.Table.Columns[primaryContraint].Ordinal;
            var rowValue = row.ItemArray[rowIndex];
            _primaryConstraints.TryGetValue(primaryContraint, out PrimaryContraint constraint);
            if (constraint is not null)
            {
                switch (constraint.Contraint)
                {
                    case PrimaryContraint.Contraints.ALPHA:
                        existingResultObject.Increment(ValidConstraint(new Alpha(), rowValue) ? CORRECT : constraint.Result.ToString());
                        break;
                    case PrimaryContraint.Contraints.ALPHANUMERIC:
                        existingResultObject.Increment(ValidConstraint(new AlphaNumeric(), rowValue) ? CORRECT : constraint.Result.ToString());
                        break;
                    case PrimaryContraint.Contraints.CHI:
                        existingResultObject.Increment(ValidConstraint(new Chi(), rowValue) ? CORRECT : constraint.Result.ToString());
                        break;
                    case PrimaryContraint.Contraints.DATE:
                        existingResultObject.Increment(ValidConstraint(new Date(), rowValue) ? CORRECT : constraint.Result.ToString());
                        break;
                    default:
                        existingResultObject.Correct++;
                        break;
                }
            }
            //TODO need to do secondary contrains here also
        }
        return existingResultObject;
    }

    private DataTable GetChunk(DbDataCommandDataFlowSource flowSource, string pivotCategory)
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
                        if (_results[pivotCategory].TryGetValue(key, out ResultObject value))
                        {
                            _results[pivotCategory][key] = EvaulateRow(row, value);
                        }
                        else
                        {
                            var newValue = new ResultObject() {};
                            _results[pivotCategory][key] = EvaulateRow(row, newValue);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            //TODO do something
        }

        return chunk;
    }

    private void GenerateReport(string pivotColumnValue, CatalogueValidation catalogueValidation)
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
        if (!_results.TryGetValue(pivotColumnValue, out _))
        {
            _results[pivotColumnValue] = new Dictionary<string, ResultObject>();
        }
        var dataChunk = GetChunk(flowSource, pivotColumnValue);
        while (dataChunk is not null)
        {
            dataChunk = GetChunk(flowSource, pivotColumnValue);
        }
        flowSource.Dispose(ThrowImmediatelyDataLoadEventListener.Quiet, null);
        foreach (var key in _results[pivotColumnValue].Keys) {
            var result = new CatalogueValidationResult(_DQERepository, catalogueValidation, DateTime.Parse(key), pivotColumnValue, _results[pivotColumnValue][key].Correct, _results[pivotColumnValue][key].Wrong, _results[pivotColumnValue][key].Missing, _results[pivotColumnValue][key].Invalid);
            result.SaveToDatabase();
        }
    }


    public void GenerateValidationReports()
    {
        var catalogueValidation = new CatalogueValidation(_DQERepository, _catalogue);
        catalogueValidation.SaveToDatabase();
        GenerateReport(ALL,catalogueValidation);
        //todo for all values in pivot category
    }
}

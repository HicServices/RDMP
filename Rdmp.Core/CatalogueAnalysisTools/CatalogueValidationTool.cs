using Amazon.Runtime;
using FAnsi.Discovery;
using MongoDB.Driver;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto;
using Rdmp.Core.CatalogueAnalysisTools.Data;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Pipeline.Sources;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataViewing;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Validation.Constraints.Primary;
using Rdmp.Core.Validation.Constraints.Secondary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MongoDB.Driver.WriteConcern;
using static Terminal.Gui.MainLoop;

namespace Rdmp.Core.CatalogueAnalysisTools;

public class CatalogueValidationTool
{
    private readonly ICatalogueRepository _catalogueRepository;
    private readonly Catalogue _catalogue;
    private readonly ColumnInfo _timeColumn;
    private readonly ColumnInfo _pivotColumn;
    private readonly DateTime? _startDate;
    private readonly DateTime? _endDate;
    private readonly bool _updatePreviousResult;
    private RowPeeker _peeker = new();
    private Dictionary<string, Dictionary<string, ResultObject>> _results = new();
    private Dictionary<string, Data.PrimaryConstraint> _primaryConstraints = new();
    private Dictionary<string, List<Data.SecondaryConstraint>> _secondaryConstraints = new();
    private DQERepository _DQERepository;
    private readonly string ALL = "ALL";
    private readonly string CORRECT = "CORRECT";
    private readonly int _batchSize = 10000;
    private Dictionary<string, int> _nullColumnCounts = new();

    public CatalogueValidationTool(ICatalogueRepository catalogueRepository, Catalogue catalogue, ColumnInfo timeColumn, ColumnInfo pivotColumn, DateTime? startDate = null, DateTime? endDate = null, bool updatePreviousResult = false, int batchSize = 10000)
    {
        _catalogueRepository = catalogueRepository;
        _DQERepository = new DQERepository(_catalogueRepository);
        _catalogue = catalogue;
        _timeColumn = timeColumn;
        _pivotColumn = pivotColumn;
        _startDate = startDate;
        _endDate = endDate;
        _updatePreviousResult = updatePreviousResult;
        _batchSize = batchSize;

        foreach (var columnInfo in catalogue.CatalogueItems.Select(c => c.ColumnInfo).Where(c => !SpecialFieldNames.IsHicPrefixed(c.GetRuntimeName())))
        {
            _primaryConstraints[columnInfo.GetRuntimeName()] = _DQERepository.GetAllObjectsWhere<Data.PrimaryConstraint>("ColumnInfo_ID", columnInfo.ID).FirstOrDefault();
            _secondaryConstraints[columnInfo.GetRuntimeName()] = _DQERepository.GetAllObjectsWhere<Data.SecondaryConstraint>("ColumnInfo_ID", columnInfo.ID).ToList();
        }


    }

    private class ResultObject
    {
        public int Correct = 0;
        public int Wrong = 0;
        public int Missing = 0;
        public int Invalid = 0;


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

    private bool ValidConstraint(Validation.Constraints.Primary.PrimaryConstraint pc, object value)
    {
        if (value is DBNull) return true;
        try
        {
            return pc.Validate(value) == null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private bool ValidSecondaryConstraint(Validation.Constraints.Secondary.SecondaryConstraint sc, object value, object[] otherColumns, string[] otherColumnNames)
    {
        if (value is DBNull) return true;
        try
        {
            //todo want to grab the other columns for this
            return sc.Validate(value, null, null) == null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void EvaluateSecondaryConstraints(object rowValue, string column, ResultObject existingResultObject)
    {
        _secondaryConstraints.TryGetValue(column, out List<Data.SecondaryConstraint> constraints);
        if (!constraints.Any())
        {
            existingResultObject.Increment(CORRECT);
            return;
        }
        int consequence = -1;
        foreach (var constraint in constraints)
        {
            var arguments = constraint.GetArguments();
            switch (constraint.Constraint)
            {
                case Data.SecondaryConstraint.Constraints.REGULAREXPRESSION:
                    if (!ValidSecondaryConstraint(new RegularExpression(arguments.Where(a => a.Key == "Pattern").First().Value), rowValue, null, null) && consequence < (int)constraint.Consequence)
                    {
                        consequence = (int)constraint.Consequence;
                    }
                    break;
                case Data.SecondaryConstraint.Constraints.BOUNDDOUBLE:
                    var boundDouble = new BoundDouble();
                    var doubleMin = arguments.Where(a => a.Key == "Lower").FirstOrDefault();
                    if (doubleMin is not null) boundDouble.Lower = int.Parse(doubleMin.Value);
                    var doubleMax = arguments.Where(a => a.Key == "Upper").FirstOrDefault();
                    if (doubleMax is not null) boundDouble.Upper = int.Parse(doubleMax.Value);

                    if (!ValidSecondaryConstraint(boundDouble, rowValue, null, null) && consequence < (int)constraint.Consequence)
                    {
                        consequence = (int)constraint.Consequence;
                    }
                    break;
                case Data.SecondaryConstraint.Constraints.BOUNDDATE:
                    var boundDate = new BoundDate();
                    var dateMin = arguments.Where(a => a.Key == "Lower").FirstOrDefault();
                    if (dateMin is not null) boundDate.Lower = DateTime.Parse(dateMin.Value);
                    var dateMax = arguments.Where(a => a.Key == "Upper").FirstOrDefault();
                    if (dateMax is not null) boundDate.Upper = DateTime.Parse(dateMax.Value);
                    if (!ValidSecondaryConstraint(boundDate, rowValue, null, null) && consequence < (int)constraint.Consequence)
                    {
                        consequence = (int)constraint.Consequence;
                    }
                    break;
                default:
                    break;
            }
        }
        if (consequence > -1)
        {
            existingResultObject.Increment(Enum.GetName(typeof(Data.SecondaryConstraint.Consequences), consequence));
        }
        else
        {
            existingResultObject.Increment(CORRECT);
        }
    }

    private ResultObject EvaulateRow(DataRow row, ResultObject existingResultObject)
    {
        foreach (var primaryConstraint in _primaryConstraints.Keys)
        {
            var rowIndex = row.Table.Columns[primaryConstraint].Ordinal;
            var rowValue = row.ItemArray[rowIndex];
            _primaryConstraints.TryGetValue(primaryConstraint, out Data.PrimaryConstraint constraint);
            if (constraint is not null)
            {
                switch (constraint.Constraint)
                {
                    case Data.PrimaryConstraint.Constraints.ALPHA:
                        if (ValidConstraint(new Alpha(), rowValue))
                        {
                            EvaluateSecondaryConstraints(rowValue, primaryConstraint, existingResultObject);
                        }
                        else
                        {
                            existingResultObject.Increment(constraint.Result.ToString());
                        }
                        break;
                    case Data.PrimaryConstraint.Constraints.ALPHANUMERIC:
                        if (ValidConstraint(new AlphaNumeric(), rowValue))
                        {
                            EvaluateSecondaryConstraints(rowValue, primaryConstraint, existingResultObject);
                        }
                        else
                        {
                            existingResultObject.Increment(constraint.Result.ToString());
                        }
                        break;
                    case Data.PrimaryConstraint.Constraints.CHI:
                        if (ValidConstraint(new Chi(), rowValue))
                        {
                            EvaluateSecondaryConstraints(rowValue, primaryConstraint, existingResultObject);
                        }
                        else
                        {
                            existingResultObject.Increment(constraint.Result.ToString());
                        }
                        break;
                    case Data.PrimaryConstraint.Constraints.DATE:
                        if (ValidConstraint(new Date(), rowValue))
                        {
                            EvaluateSecondaryConstraints(rowValue, primaryConstraint, existingResultObject);
                        }
                        else
                        {
                            existingResultObject.Increment(constraint.Result.ToString());
                        }
                        break;
                    default:
                        existingResultObject.Correct++;
                        break;
                }
            }
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
                            var newValue = new ResultObject() { };
                            _results[pivotCategory][key] = EvaulateRow(row, newValue);
                        }
                    }
                }
                if (pivotCategory == ALL)
                {

                    foreach (var col in chunk.Columns.Cast<DataColumn>())
                    {
                        var colName = col.ColumnName;
                        var nullCount = chunk.AsEnumerable().Where(row => row[colName] != null).Count();
                        var exists = _nullColumnCounts.TryGetValue(colName, out int v);
                        if (exists)
                        {
                            _nullColumnCounts.Add(colName, nullCount);
                        }
                        else
                        {
                            _nullColumnCounts[colName] = v + nullCount;
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

    private void GenerateReport(string pivotColumnValue, CatalogueValidation catalogueValidation, DateTime? startDate = null, DateTime? endDate = null, bool updatePreviousEvaluation = false)
    {
        var qb = new QueryBuilder(null, null);
        qb.AddColumnRange(_catalogue.GetAllExtractionInformation().Where(ei => !SpecialFieldNames.IsHicPrefixed(ei.ColumnInfo.GetRuntimeName())).ToArray());
        if (pivotColumnValue != ALL)
        {
            qb.AddCustomLine($"{_pivotColumn.GetFullyQualifiedName()} = '{pivotColumnValue}'", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);

        }
        if (startDate is not null)
        {
            qb.AddCustomLine($"{catalogueValidation.Catalogue.CatalogueItems.Select(c => c.ColumnInfo).Where(ci => ci.ID == catalogueValidation.TimeColumn_ID).First().GetRuntimeName()} >= '{startDate.ToString()}'", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
        }
        if (endDate is not null)
        {
            qb.AddCustomLine($"{catalogueValidation.Catalogue.CatalogueItems.Select(c => c.ColumnInfo).Where(ci => ci.ID == catalogueValidation.TimeColumn_ID).First().GetRuntimeName()} <= '{endDate.ToString()}'", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
        }
        qb.RegenerateSQL();
        var sql = qb.SQL;
        var flowSource = new DbDataCommandDataFlowSource(sql, $"Generating validation report for category {pivotColumnValue}", _catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing, false).Builder, 0)
        {
            BatchSize = _batchSize
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
        if (updatePreviousEvaluation)
        {
            //get all results that we don't have a date for
            var previousEvals = _DQERepository.GetAllObjectsWhere<CatalogueValidation>("Catalogue_ID", catalogueValidation.Catalogue.ID);
            if (previousEvals.Any())
            {
                var maxDate = previousEvals.Where(pe => pe.Date != catalogueValidation.Date).Max(pe => pe.Date);
                var mostRecentEval = previousEvals.Where(pe => pe.Date == maxDate).FirstOrDefault();
                if (mostRecentEval is not null)
                {
                    var incomingDates = _results[pivotColumnValue].Keys.Select(k => DateTime.Parse(k));
                    var nonOverwrittenResults = mostRecentEval.GetResults().Where(result => result.PivotCategory == pivotColumnValue && !incomingDates.Contains(result.Date));
                    var x = incomingDates.Count();
                    var y = nonOverwrittenResults.Count();
                    foreach (var result in nonOverwrittenResults)
                    {
                        var cvresult = new CatalogueValidationResult(_DQERepository, catalogueValidation, result.Date, pivotColumnValue, result.Correct, result.Wrong, result.Missing, result.Invalid);
                        cvresult.SaveToDatabase();
                    }
                }
            }
        }

        foreach (var key in _results[pivotColumnValue].Keys)
        {
            var result = new CatalogueValidationResult(_DQERepository, catalogueValidation, DateTime.Parse(key), pivotColumnValue, _results[pivotColumnValue][key].Correct, _results[pivotColumnValue][key].Wrong, _results[pivotColumnValue][key].Missing, _results[pivotColumnValue][key].Invalid);
            result.SaveToDatabase();
        }
    }


    private List<string> GetPivotColumnValues()
    {
        var server = _catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing, false);
        var con = server.GetConnection();
        con.Open();
        var repo = new MemoryCatalogueRepository();
        var qb = new QueryBuilder("DISTINCT", null);
        qb.AddColumn(new ColumnInfoToIColumn(repo, _pivotColumn));
        var cmd = server.GetCommand(qb.SQL, con);
        using var da = server.GetDataAdapter(cmd);
        var dt = new DataTable();
        dt.BeginLoadData();
        da.Fill(dt);
        con.Close();
        return dt.AsEnumerable().Select(r => r.ItemArray[0].ToString()).ToList();

    }

    public void GenerateValidationReports()
    {
        var catalogueValidation = new CatalogueValidation(_DQERepository, _catalogue, _timeColumn, _pivotColumn);
        catalogueValidation.SaveToDatabase();
        GenerateReport(ALL, catalogueValidation, _startDate, _endDate, _updatePreviousResult);
        var pivotValues = GetPivotColumnValues();
        foreach (var pivotColumnValue in pivotValues)
        {
            GenerateReport(pivotColumnValue, catalogueValidation, _startDate, _endDate, _updatePreviousResult);
        }
        //todo for all values in pivot category
        GenerateCounts(catalogueValidation, _startDate, _endDate, _updatePreviousResult);
    }


    private void GenerateCounts(CatalogueValidation catalogueValidation, DateTime? startTime, DateTime? endTime, bool updatePreviousResults)
    {
        var server = _catalogue.GetDistinctLiveDatabaseServer(ReusableLibraryCode.DataAccess.DataAccessContext.InternalDataProcessing, false);
        var con = server.GetConnection();
        con.Open();
        if (!updatePreviousResults)
        {
            var recordCount = 0;
            var extractionIdentifierCount = 0;
            var extractionIdentifier = _catalogue.CatalogueItems.FirstOrDefault(ci => ci.ExtractionInformation.IsExtractionIdentifier);
            if (extractionIdentifier != null)
            {
                extractionIdentifierCount = CalculateExtractionIdentifierCount(extractionIdentifier, startTime, endTime, server, con);
            }
            recordCount = _DQERepository.GetAllObjectsWhere<CatalogueValidationResult>("CatalogueValidation_ID", catalogueValidation.ID).Where(cvr => cvr.PivotCategory == ALL).Select(cvr => cvr.Correct + cvr.Wrong + cvr.Missing + cvr.Invalid).Sum();
            var count = new CatalogueValidationResultCounts(_DQERepository, catalogueValidation, recordCount, extractionIdentifierCount);
            count.SaveToDatabase();
            foreach(var col in _nullColumnCounts.Keys)
            {

                var field = new FieldCompletionRate(_DQERepository, count, _catalogue.CatalogueItems.First(c => c.Name==col).ColumnInfo, _nullColumnCounts[col] / count.RecordCount);
                field.SaveToDatabase();
            }
            //todo save field counts here
        }
        else
        {
            //todo
        }
    }


    private int CalculateExtractionIdentifierCount(CatalogueItem catalogueItem, DateTime? startTime, DateTime? endTime, DiscoveredServer server, DbConnection con)
    {
        var extractionIdentifierSQL = $"SELECT COUNT(DISTINCT {catalogueItem.ColumnInfo.GetRuntimeName()}) FROM {catalogueItem.ColumnInfo.TableInfo.Name}";
        if (startTime != null || endTime != null)
        {
            //todo
        }
        using (var cmd = server.GetCommand(extractionIdentifierSQL, con))
        {
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}

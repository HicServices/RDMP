using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Reports.PeriodicityHelpers;
using Rdmp.Core.Logging;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rdmp.Core.DataQualityEngine.Reports;

public  class ReportBuilder
{
    private readonly string _dataLoadRunFieldName;

    //where the data is located
    private DiscoveredServer _server;
    private QueryBuilder _queryBuilder;
    private Validator _validator;
    private bool _containsDataLoadID;

    public static int MaximumPivotValues = 5000;

    private Dictionary<string, DQEStateOverDataLoadRunId> byPivotRowStatesOverDataLoadRunId = new();
    private Dictionary<string, PeriodicityCubesOverTime> byPivotCategoryCubesOverTime = new();

    private IExternalDatabaseServer _loggingServer;
    private string _loggingTask;
    private LogManager _logManager;

    private int? _dataLoadID;

    private string _timePeriodicityField;
    private string _pivotCategory;
    private ICatalogue _catalogue;
    private bool _haveComplainedAboutNullCategories;
    private bool _haveComplainedAboutTrailingWhitespaces;

    public ReportBuilder(ICatalogue catalogue) {
        _catalogue = catalogue;
    }

    public void BuildReportInternals(IDataLoadEventListener listener,
      CancellationToken cancellationToken, ForkDataLoadEventListener forker, DQERepository dqeRepository)
    {
        byPivotCategoryCubesOverTime.Add("ALL", new PeriodicityCubesOverTime("ALL"));
        byPivotRowStatesOverDataLoadRunId.Add("ALL", new DQEStateOverDataLoadRunId("ALL"));

        //Check(new FromDataLoadEventListenerToCheckNotifier(forker));

        var sw = Stopwatch.StartNew();
        using (var con = _server.GetConnection())
        {
            con.Open();
            var qb = _queryBuilder;
            if (_dataLoadID is not null)
                qb.AddCustomLine($"{SpecialFieldNames.DataLoadRunID} = {_dataLoadID}", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
            var cmd = _server.GetCommand(qb.SQL, con);
            cmd.CommandTimeout = 500000;

            var t = cmd.ExecuteReaderAsync(cancellationToken);
            t.Wait(cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException("User cancelled DQE while fetching data");

            var r = t.Result;

            var progress = 0;

            while (r.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress++;
                var dataLoadRunIDOfCurrentRecord = 0;
                //to start with assume we will pass the results for the 'unknown batch' (where data load run ID is null or not available)

                //if the DataReader is likely to have a data load run ID column
                if (_containsDataLoadID)
                {
                    //get data load run id
                    var runID = dqeRepository.ObjectToNullableInt(r[_dataLoadRunFieldName]);

                    //if it has a value use it (otherwise it is null so use 0 - ugh I know, it's a primary key constraint issue)
                    if (runID != null)
                        dataLoadRunIDOfCurrentRecord = (int)runID;
                }

                string pivotValue = null;

                //if the user has a pivot category configured
                if (_pivotCategory != null)
                {
                    pivotValue = GetStringValueForPivotField(r[_pivotCategory], forker);

                    if (!_haveComplainedAboutNullCategories && string.IsNullOrWhiteSpace(pivotValue))
                    {
                        forker.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Warning,
                                $"Found a null/empty value for pivot category '{_pivotCategory}', this record will ONLY be recorded under ALL and not its specific category, you will not be warned of further nulls because there are likely to be many if there are any"));
                        _haveComplainedAboutNullCategories = true;
                        pivotValue = null;
                    }
                }

                //always increase the "ALL" category
                ProcessRecord(dqeRepository, dataLoadRunIDOfCurrentRecord, r,
                    byPivotCategoryCubesOverTime["ALL"], byPivotRowStatesOverDataLoadRunId["ALL"]);

                //if there is a value in the current record for the pivot column
                if (pivotValue != null)
                {
                    //if it is a novel
                    if (!byPivotCategoryCubesOverTime.TryGetValue(pivotValue, out var periodicityCubesOverTime))
                    {
                        //we will need to expand the dictionaries
                        if (byPivotCategoryCubesOverTime.Keys.Count > MaximumPivotValues)
                            throw new OverflowException(
                                $"Encountered more than {MaximumPivotValues} values for the pivot column {_pivotCategory} this will result in crazy space usage since it is a multiplicative scale of DQE tesseracts");

                        //expand both the time periodicity and the state results
                        byPivotRowStatesOverDataLoadRunId.Add(pivotValue,
                            new DQEStateOverDataLoadRunId(pivotValue));
                        periodicityCubesOverTime = new PeriodicityCubesOverTime(pivotValue);
                        byPivotCategoryCubesOverTime.Add(pivotValue, periodicityCubesOverTime);
                    }

                    //now we are sure that the dictionaries have the category field we can increment it
                    ProcessRecord(dqeRepository, dataLoadRunIDOfCurrentRecord, r,
periodicityCubesOverTime, byPivotRowStatesOverDataLoadRunId[pivotValue]);
                }

                if (progress % 5000 == 0)
                    forker.OnProgress(this,
                        new ProgressEventArgs($"Processing {_catalogue}",
                            new ProgressMeasurement(progress, ProgressType.Records), sw.Elapsed));
            }

            //final value
            forker.OnProgress(this,
                new ProgressEventArgs($"Processing {_catalogue}",
                    new ProgressMeasurement(progress, ProgressType.Records), sw.Elapsed));
            con.Close();
        }

        sw.Stop();
        foreach (var state in byPivotRowStatesOverDataLoadRunId.Values)
            state.CalculateFinalValues();
    }

    private string GetStringValueForPivotField(object o, IDataLoadEventListener listener)
    {
        if (o == null || o == DBNull.Value)
            return null;

        var stringValue = o.ToString();
        var trimmedValue = stringValue.Trim();

        if (!_haveComplainedAboutTrailingWhitespaces && stringValue != trimmedValue)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Found trailing/leading whitespace in value in Pivot field, this will be trimmed off:'{o}'"));
            _haveComplainedAboutTrailingWhitespaces = true;
        }

        return trimmedValue;
    }
    private void ProcessRecord(DQERepository dqeRepository, int dataLoadRunIDOfCurrentRecord, DbDataReader r,
      PeriodicityCubesOverTime periodicity, DQEStateOverDataLoadRunId states)
    {
        //make sure all the results dictionaries
        states.AddKeyToDictionaries(dataLoadRunIDOfCurrentRecord, _validator, _queryBuilder);

        //ask the validator to validate!
        _validator.ValidateVerboseAdditive(
            r, //validate the data reader
            states.ColumnValidationFailuresByDataLoadRunID[
                dataLoadRunIDOfCurrentRecord], //additively adjust the validation failures dictionary
            out var worstConsequence); //and tell us what the worst consequence in the row was


        //increment the time periodicity hypercube!
        if (_timePeriodicityField != null)
        {
            DateTime? dt;

            try
            {
                dt = dqeRepository.ObjectToNullableDateTime(r[_timePeriodicityField]);
            }
            catch (InvalidCastException e)
            {
                throw new Exception(
                    $"Found value {r[_timePeriodicityField]} of type {r[_timePeriodicityField].GetType().Name} in your time periodicity field which was not a valid date time, make sure your time periodicity field is a datetime datatype",
                    e);
            }

            if (dt != null)
                periodicity.IncrementHyperCube(dt.Value.Year, dt.Value.Month, worstConsequence);
        }

        //now we need to update everything we know about all the columns
        foreach (var state in states.AllColumnStates[dataLoadRunIDOfCurrentRecord])
        {
            //start out by assuming everything is dandy
            state.CountCorrect++;

            if (r[state.TargetProperty] == DBNull.Value)
                state.CountDBNull++;
        }

        //update row level dictionaries
        if (worstConsequence == null)
            states.RowsPassingValidationByDataLoadRunID[dataLoadRunIDOfCurrentRecord]++;
        else
            states.WorstConsequencesByDataLoadRunID[dataLoadRunIDOfCurrentRecord][(Consequence)worstConsequence]++;
    }
}

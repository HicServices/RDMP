// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.DataQualityEngine.Reports.PeriodicityHelpers;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;

namespace Rdmp.Core.DataQualityEngine.Reports;

/// <summary>
/// Runs the DQE and populates Evaluation and sub tables with the results.  This includes counts of the number of rows / columns passing / failing validation
/// nullness etc both overall and subdivided by month / pivot.
/// </summary>
public class CatalogueConstraintReport : DataQualityReport
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

    /// <summary>
    /// Set this property to use an explicit DQE results store database instead of the
    /// default DQE database indicated by the <see cref="IServerDefaults.GetDefaultFor(PermissableDefaults)"/>
    /// </summary>
    public DQERepository ExplicitDQERepository { get; set; }

    public CatalogueConstraintReport(ICatalogue catalogue, string dataLoadRunFieldName)
    {
        _dataLoadRunFieldName = dataLoadRunFieldName;
        _catalogue = catalogue;
    }

    private void SetupLogging(ICatalogueRepository repository)
    {
        //if we have already setup logging successfully then don't worry about doing it again
        if (_loggingServer != null && _logManager != null && _loggingTask != null)
            return;

        _loggingServer = repository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

        if (_loggingServer != null)
        {
            _logManager = new LogManager(_loggingServer);
            _loggingTask = _logManager.ListDataTasks().SingleOrDefault(task => task.ToLower().Equals("dqe"));

            if (_loggingTask == null)
            {
                _logManager.CreateNewLoggingTaskIfNotExists("DQE");
                _loggingTask = "DQE";
            }
        }
        else
        {
            throw new NotSupportedException(
                "You must set a Default LiveLoggingServer so we can audit the DQE run, do this through the ManageExternalServers dialog");
        }
    }

    private bool haveComplainedAboutNullCategories;


    public void UpdateReport(ICatalogue catalogue, int dataLoadId, IDataLoadEventListener listener,
        CancellationToken cancellationToken)
    {
        _ReportGeneration(catalogue, dataLoadId, listener, cancellationToken);
    }

    public override void GenerateReport(ICatalogue c, IDataLoadEventListener listener,
        CancellationToken cancellationToken)
    {
        _ReportGeneration(c, 0, listener, cancellationToken);
    }

    private void _ReportGeneration(ICatalogue c, int dataLoadId, IDataLoadEventListener listener,
        CancellationToken cancellationToken)
    {
        SetupLogging(c.CatalogueRepository);

        var toDatabaseLogger = new ToLoggingDatabaseDataLoadEventListener(this, _logManager, _loggingTask,
            $"DQE evaluation of {c}");

        var forker = new ForkDataLoadEventListener(listener, toDatabaseLogger);
        try
        {
            _catalogue = c;
            var dqeRepository = ExplicitDQERepository ?? new DQERepository(c.CatalogueRepository);

            byPivotCategoryCubesOverTime.Add("ALL", new PeriodicityCubesOverTime("ALL"));
            byPivotRowStatesOverDataLoadRunId.Add("ALL", new DQEStateOverDataLoadRunId("ALL"));

            Check(new FromDataLoadEventListenerToCheckNotifier(forker));

            var sw = Stopwatch.StartNew();
            using (var con = _server.GetConnection())
            {
                con.Open();
                if(dataLoadId != 0)
                {
                    _queryBuilder.AddCustomLine($"{SpecialFieldNames.DataLoadRunID}='{dataLoadId}'", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
                }
                var cmd = _server.GetCommand(_queryBuilder.SQL, con);
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
                    var dataLoadRunIDOfCurrentRecord = dataLoadId;
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

                        if (!haveComplainedAboutNullCategories && string.IsNullOrWhiteSpace(pivotValue))
                        {
                            forker.OnNotify(this,
                                new NotifyEventArgs(ProgressEventType.Warning,
                                    $"Found a null/empty value for pivot category '{_pivotCategory}', this record will ONLY be recorded under ALL and not its specific category, you will not be warned of further nulls because there are likely to be many if there are any"));
                            haveComplainedAboutNullCategories = true;
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

            //now commit results
            using (var con = dqeRepository.BeginNewTransactedConnection())
            {
                try
                {
                    //mark down that we are beginning an evaluation on this the day of our lord etc...
                    var evaluation = new Evaluation(dqeRepository, _catalogue);

                    foreach (var state in byPivotRowStatesOverDataLoadRunId.Values)
                        state.CommitToDatabase(evaluation, _catalogue, con.Connection, con.Transaction);

                    if (_timePeriodicityField != null)
                        foreach (var periodicity in byPivotCategoryCubesOverTime.Values)
                            periodicity.CommitToDatabase(evaluation);

                    dqeRepository.EndTransactedConnection(true);
                }
                catch (Exception)
                {
                    dqeRepository.EndTransactedConnection(false);
                    throw;
                }
            }

            forker.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "CatalogueConstraintReport completed successfully  and committed results to DQE server"));
        }
        catch (Exception e)
        {
            forker.OnNotify(this,
                e is OperationCanceledException
                    ? new NotifyEventArgs(ProgressEventType.Warning, "DQE Execution Cancelled", e)
                    : new NotifyEventArgs(ProgressEventType.Error, "Fatal Crash", e));
        }
        finally
        {
            toDatabaseLogger.FinalizeTableLoadInfos();
        }
    }

    private bool _haveComplainedAboutTrailingWhitespaces;

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

    private string _timePeriodicityField;
    private string _pivotCategory;


    public override void Check(ICheckNotifier notifier)
    {
        //there is a catalogue
        if (_catalogue == null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                "Catalogue has not been set, either use the constructor with Catalogue parameter or use the blank constructor and call CatalogueSupportsReport instead",
                CheckResult.Fail));
            return;
        }

        try
        {
            var dqeRepository = ExplicitDQERepository ?? new DQERepository(_catalogue.CatalogueRepository);
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Found DQE reporting server {dqeRepository.DiscoveredServer.Name}", CheckResult.Success));
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Failed to create DQE Repository, possibly there is no DataQualityEngine Reporting Server (ExternalDatabaseServer).  You will need to create/set one in CatalogueManager by using 'Locations=>Manage External Servers...'",
                    CheckResult.Fail, e));
        }

        try
        {
            SetupLogging(_catalogue.CatalogueRepository);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to setup logging of DQE runs", CheckResult.Fail, e));
            return;
        }

        //there is XML
        if (string.IsNullOrWhiteSpace(_catalogue.ValidatorXML))
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"There is no ValidatorXML specified for the Catalogue {_catalogue}, you must configure validation rules",
                CheckResult.Fail));
            return;
        }

        notifier.OnCheckPerformed(new CheckEventArgs(
            $"Found ValidatorXML specified for the Catalogue {_catalogue}:{Environment.NewLine}{_catalogue.ValidatorXML}",
            CheckResult.Success));

        //the XML is legit
        try
        {
            _validator = Validator.LoadFromXml(_catalogue.ValidatorXML);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"ValidatorXML for Catalogue {_catalogue} could not be deserialized into a Validator", CheckResult.Fail,
                e));
            return;
        }

        notifier.OnCheckPerformed(new CheckEventArgs("Deserialized validation XML successfully", CheckResult.Success));

        //there is a server
        try
        {
            _server = _catalogue.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, true);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Could not get connection to Catalogue {_catalogue}",
                CheckResult.Fail, e));
            return;
        }

        notifier.OnCheckPerformed(new CheckEventArgs($"Found connection string for Catalogue {_catalogue}",
            CheckResult.Success));

        //we can connect to the server
        try
        {
            _server.TestConnection();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Could not connect to server for Catalogue {_catalogue}",
                CheckResult.Fail, e));
        }

        //there is extraction SQL
        try
        {
            _queryBuilder = new QueryBuilder("", "");
            _queryBuilder.AddColumnRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            var duplicates = _queryBuilder.SelectColumns.GroupBy(c => c.IColumn.GetRuntimeName())
                .SelectMany(grp => grp.Skip(1)).ToArray();

            if (duplicates.Any())
                foreach (var column in duplicates)
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"The column name {column.IColumn.GetRuntimeName()} is duplicated in the SELECT command, column names must be unique!  Most likely you have 2+ columns with the same name (from different tables) or duplicate named CatalogueItem/Aliases for the same underlying ColumnInfo",
                            CheckResult.Fail));

            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Query Builder decided the extraction SQL was:{Environment.NewLine}{_queryBuilder.SQL}",
                CheckResult.Success));

            SetupAdditionalValidationRules(notifier);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Failed to generate extraction SQL", CheckResult.Fail, e));
        }

        //for each thing we are about to try and validate
        foreach (var itemValidator in _validator.ItemValidators)
            //is there a column in the query builder that matches it
            if (
                //there isnt!
                !_queryBuilder.SelectColumns.Any(
                    c => c.IColumn.GetRuntimeName().Equals(itemValidator.TargetProperty)))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Could not find a column in the extraction SQL that would match TargetProperty {itemValidator.TargetProperty}",
                        CheckResult.Fail));
            else
                //there is that is good
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Found column in query builder columns which matches TargetProperty {itemValidator.TargetProperty}",
                        CheckResult.Success));

        _containsDataLoadID =
            _queryBuilder.SelectColumns.Any(
                c => c.IColumn.GetRuntimeName().Equals(_dataLoadRunFieldName));

        if (_containsDataLoadID)
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Found {_dataLoadRunFieldName} field in ExtractionInformation",
                    CheckResult.Success));
        else
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Did not find ExtractionInformation for a column called {_dataLoadRunFieldName}, this will prevent you from viewing the resulting report subdivided by data load batch (make sure you have this column and that it is marked as extractable)",
                    CheckResult.Warning));


        if (_catalogue.PivotCategory_ExtractionInformation_ID == null)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Catalogue does not have a pivot category so all records will appear as PivotCategory 'ALL'",
                    CheckResult.Warning));
        }
        else
        {
            _pivotCategory = _catalogue.PivotCategory_ExtractionInformation.GetRuntimeName();
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Found time Pivot Category field {_pivotCategory} so we will be able to generate a categorised tesseract (evaluation, periodicity, consequence, pivot category)",
                    CheckResult.Success));
        }

        var tblValuedFunctions = _catalogue.GetTableInfoList(true).Where(t => t.IsTableValuedFunction).ToArray();
        if (tblValuedFunctions.Any())
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Catalogue contains 1+ table valued function in its TableInfos ({string.Join(",", tblValuedFunctions.Select(t => t.ToString()))}",
                    CheckResult.Fail));

        if (_catalogue.TimeCoverage_ExtractionInformation_ID == null)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    "Catalogue does not have a time coverage field set",
                    CheckResult.Fail));
        }
        else
        {
            var periodicityExtractionInformation = _catalogue.TimeCoverage_ExtractionInformation;

            _timePeriodicityField = periodicityExtractionInformation.GetRuntimeName();
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Found time coverage field {_timePeriodicityField}",
                    CheckResult.Success));

            if (!periodicityExtractionInformation.ColumnInfo.Data_type.ToLower().Contains("date"))
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Time periodicity field {_timePeriodicityField} was of type {periodicityExtractionInformation.ColumnInfo.Data_type} (expected the type name to contain the word 'date' - ignoring caps).  It is possible (but unlikely) that you have dealt with this by applying a transform to the underlying ColumnInfo as part of the ExtractionInformation, if so you can ignore this message.",
                        CheckResult.Warning));
            else
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Time periodicity field {_timePeriodicityField} is a legit date!",
                        CheckResult.Success));
        }
    }

    private void SetupAdditionalValidationRules(ICheckNotifier notifier)
    {
        //for each description
        foreach (var descQtc in _queryBuilder.SelectColumns.Where(static qtc => qtc.IsLookupDescription))
            try
            {
                //if we have a the foreign key too
                foreach (var foreignQtc in _queryBuilder.SelectColumns.Where(fk =>
                             fk.IsLookupForeignKey && fk.LookupTable.ID == descQtc.LookupTable.ID))
                {
                    var descriptionFieldName = descQtc.IColumn.GetRuntimeName();
                    var foreignKeyFieldName = foreignQtc.IColumn.GetRuntimeName();

                    var itemValidator = _validator.GetItemValidator(descriptionFieldName);

                    //there is not yet one for this field
                    if (itemValidator == null)
                    {
                        itemValidator = new ItemValidator(descriptionFieldName);
                        _validator.ItemValidators.Add(itemValidator);
                    }

                    //if it doesn't already have a prediction
                    if (itemValidator.SecondaryConstraints.All(static constraint =>
                            constraint.GetType() != typeof(Prediction)))
                    {
                        //Add an item validator onto the fk column that targets the description column with a nullity prediction
                        var newRule = new Prediction(new ValuePredictsOtherValueNullity(), foreignKeyFieldName)
                        {
                            Consequence = Consequence.Missing
                        };

                        //add one that says 'if I am null my fk should also be null'
                        itemValidator.SecondaryConstraints.Add(newRule);

                        notifier.OnCheckPerformed(
                            new CheckEventArgs(
                                $"Dynamically added value->value Nullity constraint with consequence Missing onto columns {foreignKeyFieldName} and {descriptionFieldName} because they have a configured Lookup relationship in the Catalogue",
                                CheckResult.Success));
                    }
                }
            }
            catch (Exception ex)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        $"Failed to add new lookup validation rule for column {descQtc.IColumn.GetRuntimeName()}",
                        CheckResult.Warning, ex));
            }
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
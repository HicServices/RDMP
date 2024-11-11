// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FAnsi.Discovery;
using Org.BouncyCastle.Security.Certificates;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.DataQualityEngine.Reports.PeriodicityHelpers;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.Validation;
using Rdmp.Core.Validation.Constraints;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;
using static Terminal.Gui.Application;

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

    private int? _dataLoadID;

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

    //private bool haveComplainedAboutNullCategories;


    public override void GenerateReport(ICatalogue c, IDataLoadEventListener listener,
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
            DbDataReader r;
            Check(new FromDataLoadEventListenerToCheckNotifier(forker));
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

                r = t.Result;
                var reportBuilder = new ReportBuilder(c, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, r);
                reportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);

                byPivotCategoryCubesOverTime = reportBuilder.GetByPivotCategoryCubesOverTime();
                byPivotRowStatesOverDataLoadRunId = reportBuilder.GetByPivotRowStatesOverDataLoadRunId();
            }




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


    public void UpdateReport(ICatalogue c, int dataLoadID, IDataLoadEventListener listener,
        CancellationToken cancellationToken)
    {
        _dataLoadID = dataLoadID;
        SetupLogging(c.CatalogueRepository);

        var toDatabaseLogger = new ToLoggingDatabaseDataLoadEventListener(this, _logManager, _loggingTask,
            $"DQE evaluation of {c}");

        var forker = new ForkDataLoadEventListener(listener, toDatabaseLogger);
        try
        {
            _catalogue = c;
            var dqeRepository = ExplicitDQERepository ?? new DQERepository(c.CatalogueRepository);
            //make report for new data
            DataTable rDT = new();
            Check(new FromDataLoadEventListenerToCheckNotifier(forker));

            using (var con = _server.GetConnection())
            {
                con.Open();
                var qb = _queryBuilder;
                if (_dataLoadID is not null)
                    qb.AddCustomLine($"{SpecialFieldNames.DataLoadRunID} = {_dataLoadID}", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
                var cmd = _server.GetCommand(qb.SQL, con);
                cmd.CommandTimeout = 500000;
                var adapter = _server.GetDataAdapter(cmd);
                rDT.BeginLoadData();
                adapter.Fill(rDT);
                rDT.EndLoadData();
                con.Close();
                //var t = cmd.ExecuteReaderAsync(cancellationToken);
                //t.Wait(cancellationToken);

                //if (cancellationToken.IsCancellationRequested)
                //    throw new OperationCanceledException("User cancelled DQE while fetching data");

                //r = t.Result;
            }
            var reportBuilder = new ReportBuilder(c, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, rDT);
            reportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);
            var newByPivotRowStatesOverDataLoadRunId = reportBuilder.GetByPivotRowStatesOverDataLoadRunId();
            var newByPivotCategoryCubesOverTime = reportBuilder.GetByPivotCategoryCubesOverTime();
            using (var con = dqeRepository.BeginNewTransactedConnection())
            {
                try
                {
                    //mark down that we are beginning an evaluation on this the day of our lord etc...
                    var previousEvaluation = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", _catalogue.ID).LastOrDefault() ?? throw new Exception("No DQE results currently exist");
                    var evaluation = new Evaluation(dqeRepository, _catalogue);

                    //find entries that have been put in the archive
                    //is this the correct table info?
                    var tableInfo = _catalogue.CatalogueItems.First().ColumnInfo.TableInfo;
                    var dataDiffFetcher = new DiffDatabaseDataFetcher(9, tableInfo, (int)_dataLoadID, 50000);
                    dataDiffFetcher.FetchData(new AcceptAllCheckNotifier());
                    var replaced = dataDiffFetcher.Updates_Replaced; //all the stuff that has been replaced by the new data load

                    var previousReportBuilder = new ReportBuilder(_catalogue, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, replaced);
                    previousReportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);
                    var previousRows = previousReportBuilder.GetByPivotRowStatesOverDataLoadRunId();
                    var previousColumns = previousReportBuilder.GetByPivotCategoryCubesOverTime();

                    var pivotColumn = c.PivotCategory_ExtractionInformation.ColumnInfo.Name;

                    foreach (var state in newByPivotRowStatesOverDataLoadRunId.Values)
                    {
                        state.CommitToDatabase(evaluation, _catalogue, con.Connection, con.Transaction);
                    }

                    foreach (var rowState in previousEvaluation.RowStates)
                    {
                        var correct = rowState.Correct;
                        var missing = rowState.Missing;
                        var wrong = rowState.Wrong;
                        var invalid = rowState.Invalid;
                        if (replaced.AsEnumerable().Any() && previousRows.TryGetValue(rowState.PivotCategory, out var pivotCategoryRow))
                        {
                            var oldCorrect = pivotCategoryRow.RowsPassingValidationByDataLoadRunID[0];
                            var oldMissing = pivotCategoryRow.WorstConsequencesByDataLoadRunID[0][Consequence.Missing];
                            var oldWrong = pivotCategoryRow.WorstConsequencesByDataLoadRunID[0][Consequence.Wrong];
                            var oldInvalid = pivotCategoryRow.WorstConsequencesByDataLoadRunID[0][Consequence.InvalidatesRow];
                            correct -= oldCorrect;
                            missing -= oldMissing;
                            wrong -= oldWrong;
                            invalid -= oldInvalid;
                            Console.WriteLine("1");
                        }

                        if (correct < 1 && missing < 1 && wrong < 1 && invalid < 1) continue;

                        _ = new RowState(evaluation, rowState.DataLoadRunID, correct, missing, wrong, invalid, rowState.ValidatorXML, rowState.PivotCategory, con.Connection, con.Transaction);
                    }
                    //TODO need to update the column states ad the counts for ALL are wrong
                    foreach (var columnState in previousEvaluation.ColumnStates)
                    {
                        var cs = new ColumnState(columnState.TargetProperty, columnState.DataLoadRunID, columnState.ItemValidatorXML)
                        {
                            CountMissing = columnState.CountMissing,
                            CountWrong = columnState.CountWrong,
                            CountInvalidatesRow = columnState.CountInvalidatesRow,
                            CountCorrect = columnState.CountCorrect,
                            CountDBNull = columnState.CountDBNull
                        };
                        var x = previousRows.TryGetValue(columnState.PivotCategory, out var pivotCategoryRow);
                        if (pivotCategoryRow != null)
                        {
                            pivotCategoryRow.AllColumnStates.TryGetValue(columnState.DataLoadRunID, out var allcolumnStates);
                            if (allcolumnStates is not null)
                            {
                                //var allcolumnStates = pivotCategoryRow.AllColumnStates[columnState.DataLoadRunID];
                                var col = allcolumnStates.Where(c => c.TargetProperty == columnState.TargetProperty).FirstOrDefault();
                                if (col is not null)
                                {
                                    //cs.CountMissing -= col.CountMissing;
                                    cs.CountMissing = col.CountMissing - cs.CountMissing;
                                    cs.CountWrong -= col.CountWrong;
                                    cs.CountInvalidatesRow -= col.CountInvalidatesRow;
                                    cs.CountCorrect -= col.CountCorrect;
                                    cs.CountDBNull -= col.CountDBNull;
                                }
                            }
                        }


                        if (replaced.AsEnumerable().Any() && previousColumns.TryGetValue(columnState.PivotCategory, out var pivotCategoryColumns))
                        {
                            var y = pivotCategoryColumns;
                        }
                        cs.Commit(evaluation, columnState.PivotCategory, con.Connection, con.Transaction);
                    }

                    if (_timePeriodicityField != null)
                    {
                        var categories = previousEvaluation.RowStates.Select(r => r.PivotCategory).Distinct().ToList();
                        foreach (var category in categories)
                        {
                            var periodicityDT = PeriodicityState.GetPeriodicityForDataTableForEvaluation(previousEvaluation, category, false);
                            if (!newByPivotCategoryCubesOverTime.ContainsKey(category))
                            {
                                newByPivotCategoryCubesOverTime[category] = new PeriodicityCubesOverTime(category);
                            }
                            //exists, just add
                            foreach (var row in periodicityDT.AsEnumerable())
                            {
                                for (int i = 0; i < int.Parse(row.ItemArray[2].ToString()); i++)
                                {
                                    Enum.TryParse<Consequence>(row.ItemArray[3].ToString(), out Consequence cons);
                                    newByPivotCategoryCubesOverTime[category].IncrementHyperCube(DateTime.Parse(row.ItemArray[1].ToString()).Year, DateTime.Parse(row.ItemArray[1].ToString()).Month, cons);
                                }
                            }

                        }
                        foreach (var periodicity in newByPivotCategoryCubesOverTime.Values)
                        {
                            periodicity.CommitToDatabase(evaluation);
                        }
                    }
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

}
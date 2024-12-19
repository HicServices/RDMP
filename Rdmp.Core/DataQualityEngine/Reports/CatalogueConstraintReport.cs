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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using FAnsi.Discovery;
using MongoDB.Driver;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
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

    //Notes
    // this is technically more efficient than a full DQE, but ot's pretty rubbish for categories with updates as we recalculate for the whole category
    //may be worth thinking about how we can keep existing records and modify/add to them depending on what's goin on


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
            }
            var reportBuilder = new ReportBuilder(c, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, rDT);
            reportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);
            var newByPivotRowStatesOverDataLoadRunId = reportBuilder.GetByPivotRowStatesOverDataLoadRunId();
            var newByPivotCategoryCubesOverTime = reportBuilder.GetByPivotCategoryCubesOverTime();

            var pivotColumn = c.PivotCategory_ExtractionInformation.ColumnInfo.GetRuntimeName();
            var timeColumn = c.TimeCoverage_ExtractionInformation.ColumnInfo.GetRuntimeName();

            var incomingPivotCategories = rDT.AsEnumerable().Select(r => r[pivotColumn].ToString()).ToList().Distinct();

            using (var con = dqeRepository.BeginNewTransactedConnection())
            {
                var previousEvaluation = dqeRepository.GetAllObjectsWhere<Evaluation>("CatalogueID", _catalogue.ID).LastOrDefault() ?? throw new Exception("No DQE results currently exist");
                var previousColumnStates = previousEvaluation.ColumnStates;
                var previousRowSates = previousEvaluation.RowStates;
                var previousCategories = previousEvaluation.GetPivotCategoryValues().Where(c => c != "ALL");

                var evaluation = new Evaluation(dqeRepository, _catalogue);

                //new pivoutCategories coming in
                var newIncomingPivotCategories = incomingPivotCategories.Where(c => !previousCategories.Contains(c));
                List<ColumnState> ColumnStates = [];


                var pivotColumnInfo = _catalogue.CatalogueItems.Where(ci => ci.Name == _pivotCategory).FirstOrDefault();
                if (pivotColumnInfo is null) throw new Exception("Can't find column infor for pivot category");
                var tableInfo = pivotColumnInfo.ColumnInfo.TableInfo;
                var dataDiffFetcher = new DiffDatabaseDataFetcher(10000000, tableInfo, (int)_dataLoadID, 50000);//todo update these numbers
                dataDiffFetcher.FetchData(new AcceptAllCheckNotifier());
                //pivot categories that have been replaces 100%?
                var replacedPivotCategories = previousCategories.Where(c =>
                {
                    if (incomingPivotCategories.Contains(c)) return false;//not a total replacement
                    var replacedCount = dataDiffFetcher.Updates_Replaced.AsEnumerable().Where(r => r[_pivotCategory].ToString() == c).Count();
                    var previousRowState = previousRowSates.Where(rs => rs.PivotCategory == c).FirstOrDefault();
                    if (previousRowState is null) return false; //did not exist before
                    var previousEvaluationTotal = previousRowState.Correct + previousRowState.Missing + previousRowState.Wrong + previousRowState.Invalid;
                    return replacedCount == previousEvaluationTotal;
                });

                // existing pivot categories coming in
                var existingIncomingPivotCategories = incomingPivotCategories.Where(c => previousCategories.Contains(c) && !replacedPivotCategories.Contains(c) && c != "ALL");


                //* Row States *//
                //unchanges categories
                foreach (var previousRowState in previousRowSates.Where(rs => rs.PivotCategory != "ALL" && !existingIncomingPivotCategories.Contains(rs.PivotCategory) && !replacedPivotCategories.Contains(rs.PivotCategory)))
                {
                    //copy row states that have not changes
                    evaluation.AddRowState(previousRowState.DataLoadRunID, previousRowState.Correct, previousRowState.Missing, previousRowState.Wrong, previousRowState.Invalid, previousRowState.ValidatorXML, previousRowState.PivotCategory, con.Connection, con.Transaction);
                }
                //new categories
                foreach (var newCategory in newIncomingPivotCategories)
                {
                    newByPivotRowStatesOverDataLoadRunId.TryGetValue(newCategory, out DQEStateOverDataLoadRunId incomingState);
                    incomingState.RowsPassingValidationByDataLoadRunID.TryGetValue((int)_dataLoadID, out int correct);
                    incomingState.WorstConsequencesByDataLoadRunID.TryGetValue((int)_dataLoadID, out Dictionary<Consequence, int> results);
                    results.TryGetValue(Consequence.Missing, out int mising);
                    results.TryGetValue(Consequence.Wrong, out int wrong);
                    results.TryGetValue(Consequence.InvalidatesRow, out int invalidatesRow);
                    evaluation.AddRowState((int)_dataLoadID, correct, mising, wrong, invalidatesRow, _catalogue.ValidatorXML, newCategory, con.Connection, con.Transaction);

                    incomingState.AllColumnStates.TryGetValue((int)_dataLoadID, out ColumnState[] columnStates);
                    foreach (var columnState in columnStates)
                    {
                        columnState.Commit(evaluation, newCategory, con.Connection, con.Transaction);
                        ColumnStates.Add(columnState);
                    }
                }
                //Updates
                if (existingIncomingPivotCategories.Any())
                {
                    //existing row states with new entries
                    var updatedRowsDataTable = new DataTable();
                    var qb = new QueryBuilder(null, "");

                    using (var updateCon = _server.GetConnection())
                    {
                        updateCon.Open();
                        qb.AddColumnRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
                        qb.AddCustomLine($"{pivotColumn} in ({string.Join(',', existingIncomingPivotCategories.Select(i => $"'{i}'"))})", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
                        var cmd = _server.GetCommand(qb.SQL, updateCon);
                        cmd.CommandTimeout = 500000;
                        var adapter = _server.GetDataAdapter(cmd);
                        updatedRowsDataTable.BeginLoadData();
                        adapter.Fill(updatedRowsDataTable);
                        updatedRowsDataTable.EndLoadData();
                        updateCon.Close();
                    }
                    var updatedRowsReportBuilder = new ReportBuilder(c, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, updatedRowsDataTable);
                    updatedRowsReportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);
                    var updatedByPivotRowStatesOverDataLoadRunId = updatedRowsReportBuilder.GetByPivotRowStatesOverDataLoadRunId();

                    foreach (var updatedCategory in existingIncomingPivotCategories)
                    {
                        updatedByPivotRowStatesOverDataLoadRunId.TryGetValue(updatedCategory, out DQEStateOverDataLoadRunId incomingState);
                        foreach (var loadId in incomingState.RowsPassingValidationByDataLoadRunID.Keys)
                        {
                            incomingState.RowsPassingValidationByDataLoadRunID.TryGetValue(loadId, out int _correct);
                            incomingState.WorstConsequencesByDataLoadRunID.TryGetValue(loadId, out Dictionary<Consequence, int> results);
                            results.TryGetValue(Consequence.Missing, out int _missing);
                            results.TryGetValue(Consequence.Wrong, out int _wrong);
                            results.TryGetValue(Consequence.InvalidatesRow, out int _invalidatesRow);
                            evaluation.AddRowState(loadId, _correct, _missing, _wrong, _invalidatesRow, _catalogue.ValidatorXML, updatedCategory, con.Connection, con.Transaction);

                            incomingState.AllColumnStates.TryGetValue(loadId, out ColumnState[] columnStates);
                            foreach (var columnState in columnStates)
                            {
                                columnState.Commit(evaluation, updatedCategory, con.Connection, con.Transaction);
                                ColumnStates.Add(columnState);
                            }
                        }
                    }
                }
                List<RowState> AllStates = new();
                foreach (var rowState in evaluation.RowStates)
                {
                    if (!AllStates.Any(state => state.DataLoadRunID == rowState.DataLoadRunID))
                    {
                        AllStates.Add(new RowState(rowState.DataLoadRunID, rowState.Correct, rowState.Missing, rowState.Wrong, rowState.Invalid, _catalogue.ValidatorXML, "ALL"));
                    }
                    else
                    {
                        var current = AllStates.Where(state => state.DataLoadRunID == rowState.DataLoadRunID).FirstOrDefault();
                        if (current is not null)
                        {
                            var newState = new RowState(rowState.DataLoadRunID, rowState.Correct + current.Correct, rowState.Missing + current.Missing, rowState.Wrong + current.Wrong, rowState.Invalid + current.Invalid, _catalogue.ValidatorXML, "ALL");
                            AllStates = AllStates.Where(state => state.DataLoadRunID != rowState.DataLoadRunID).ToList();
                            AllStates.Add(newState);
                        }
                    }
                }
                foreach (var state in AllStates)
                {
                    evaluation.AddRowState(state.DataLoadRunID, state.Correct, state.Missing, state.Wrong, state.Invalid, _catalogue.ValidatorXML, "ALL", con.Connection, con.Transaction);

                }
                //* Column States *//
                //unchanged 
                foreach (var previousColumnState in previousColumnStates.Where(rs => rs.PivotCategory != "ALL" && !existingIncomingPivotCategories.Contains(rs.PivotCategory) && !replacedPivotCategories.Contains(rs.PivotCategory)))
                {
                    var cm = new ColumnState(previousColumnState.TargetProperty, previousColumnState.DataLoadRunID, previousColumnState.ItemValidatorXML)
                    {
                        CountCorrect = previousColumnState.CountCorrect,
                        CountMissing = previousColumnState.CountMissing,
                        CountWrong = previousColumnState.CountWrong,
                        CountInvalidatesRow = previousColumnState.CountInvalidatesRow,
                        CountDBNull = previousColumnState.CountDBNull
                    };
                    cm.Commit(evaluation, previousColumnState.PivotCategory, con.Connection, con.Transaction);
                    ColumnStates.Add(cm);
                }
                List<ColumnState> AllColumns = new();
                foreach (var columnState in ColumnStates)
                {
                    if (!AllColumns.Any(state => state.DataLoadRunID == columnState.DataLoadRunID && state.TargetProperty == columnState.TargetProperty && state.PivotCategory == columnState.PivotCategory))
                    {
                        var cm = new ColumnState(columnState.TargetProperty, columnState.DataLoadRunID, columnState.ItemValidatorXML)
                        {
                            CountCorrect = columnState.CountCorrect,
                            CountMissing = columnState.CountMissing,
                            CountWrong = columnState.CountWrong,
                            CountInvalidatesRow = columnState.CountInvalidatesRow,
                            CountDBNull = columnState.CountDBNull
                        };
                        AllColumns.Add(cm);
                    }
                    else
                    {
                        var index = AllColumns.FindIndex(state => state.DataLoadRunID == columnState.DataLoadRunID && state.TargetProperty == columnState.TargetProperty && state.PivotCategory == columnState.PivotCategory);
                        if (index != -1)
                        {
                            AllColumns[index].CountCorrect += columnState.CountCorrect;
                            AllColumns[index].CountMissing += columnState.CountMissing;
                            AllColumns[index].CountWrong += columnState.CountWrong;
                            AllColumns[index].CountInvalidatesRow += columnState.CountInvalidatesRow;
                            AllColumns[index].CountDBNull += columnState.CountDBNull;
                        }
                    }
                }
                foreach (var column in AllColumns)
                {
                    column.Commit(evaluation, "ALL", con.Connection, con.Transaction);
                }

                //* Periodicity States *//

                //Unchanged
                newByPivotCategoryCubesOverTime = new();//reset

                var unchangedPivotCategories = previousRowSates.Where(rs => rs.PivotCategory != "ALL" && !existingIncomingPivotCategories.Contains(rs.PivotCategory) && !replacedPivotCategories.Contains(rs.PivotCategory)).Select(rs => rs.PivotCategory).Distinct(); foreach (var previousRowState in previousRowSates.Where(rs => rs.PivotCategory != "ALL" && !existingIncomingPivotCategories.Contains(rs.PivotCategory) && !replacedPivotCategories.Contains(rs.PivotCategory))) ;
                newByPivotCategoryCubesOverTime.TryGetValue("ALL", out var value);
                if (value is null)
                {
                    newByPivotCategoryCubesOverTime["ALL"] = new PeriodicityCubesOverTime("ALL");
                }
                foreach (var pivotCategory in unchangedPivotCategories)
                {
                    var previousPeriodicity = PeriodicityState.GetPeriodicityForDataTableForEvaluation(previousEvaluation, pivotCategory, false);
                    newByPivotCategoryCubesOverTime.TryGetValue(pivotCategory, out value);
                    if (value is null)
                    {
                        newByPivotCategoryCubesOverTime[pivotCategory] = new PeriodicityCubesOverTime(pivotCategory);
                    }

                    foreach (var row in previousPeriodicity.AsEnumerable())
                    {
                        var countOfRecords = int.Parse(row[2].ToString());
                        for (var i = 0; i < countOfRecords; i++)
                        {

                            Consequence.TryParse(row[3].ToString(), out Consequence consequence);
                            var date = DateTime.Parse(row[1].ToString());
                            newByPivotCategoryCubesOverTime[pivotCategory].IncrementHyperCube(date.Year, date.Month, consequence);

                            newByPivotCategoryCubesOverTime["ALL"].IncrementHyperCube(date.Year, date.Month, consequence);
                        }
                    }
                }
                //what about the replacements?
                if (existingIncomingPivotCategories.Any())
                {
                    var updatedRowsDataTable = new DataTable();
                    var qb = new QueryBuilder(null, "");

                    using (var updateCon = _server.GetConnection())
                    {
                        updateCon.Open();
                        qb.AddColumnRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
                        qb.AddCustomLine($"{pivotColumn} in ({string.Join(',', existingIncomingPivotCategories.Select(i => $"'{i}'"))})", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
                        var cmd = _server.GetCommand(qb.SQL, updateCon);
                        cmd.CommandTimeout = 500000;
                        var adapter = _server.GetDataAdapter(cmd);
                        updatedRowsDataTable.BeginLoadData();
                        adapter.Fill(updatedRowsDataTable);
                        updatedRowsDataTable.EndLoadData();
                        updateCon.Close();
                    }
                    var updatedRowsReportBuilder = new ReportBuilder(c, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, updatedRowsDataTable);
                    updatedRowsReportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);
                    var cc = updatedRowsReportBuilder.GetByPivotCategoryCubesOverTime();
                    foreach (var category in cc.Keys)
                    {
                        var hyperCube = cc[category].GetHyperCube();
                        foreach (var year in hyperCube.Keys)
                        {
                            var periodicityCubes = hyperCube[year];
                            foreach (var month in periodicityCubes.Keys)
                            {
                                var cube = periodicityCubes[month];
                                foreach (var consequence in Enum.GetValues(typeof(Consequence)).Cast<Consequence>().ToList())
                                {
                                    var state = cube.GetStateForConsequence(consequence);
                                    for (var i = 0; i < state.CountOfRecords; i++)
                                    {
                                        newByPivotCategoryCubesOverTime.TryGetValue(category, out value);
                                        if (value is null)
                                        {
                                            newByPivotCategoryCubesOverTime[category] = new PeriodicityCubesOverTime(category);
                                        }
                                        newByPivotCategoryCubesOverTime[category].IncrementHyperCube(year, month, consequence);
                                    }

                                }
                            }

                        }
                        //want to add this to newByPivotCategoryCubesOverTime 

                    }
                }
                //foreach (var newCategory in newIncomingPivotCategories)
                if (newIncomingPivotCategories.Any())
                {
                    var updatedRowsDataTable = new DataTable();
                    var qb = new QueryBuilder(null, "");

                    using (var updateCon = _server.GetConnection())
                    {
                        updateCon.Open();
                        qb.AddColumnRange(_catalogue.GetAllExtractionInformation(ExtractionCategory.Any));
                        qb.AddCustomLine($"{pivotColumn} in ({string.Join(',', newIncomingPivotCategories.Select(i => $"'{i}'"))})", FAnsi.Discovery.QuerySyntax.QueryComponent.WHERE);
                        var cmd = _server.GetCommand(qb.SQL, updateCon);
                        cmd.CommandTimeout = 500000;
                        var adapter = _server.GetDataAdapter(cmd);
                        updatedRowsDataTable.BeginLoadData();
                        adapter.Fill(updatedRowsDataTable);
                        updatedRowsDataTable.EndLoadData();
                        updateCon.Close();
                    }
                    var updatedRowsReportBuilder = new ReportBuilder(c, _validator, _queryBuilder, _dataLoadRunFieldName, _containsDataLoadID, _timePeriodicityField, _pivotCategory, updatedRowsDataTable);
                    updatedRowsReportBuilder.BuildReportInternals(cancellationToken, forker, dqeRepository);
                    var cc = updatedRowsReportBuilder.GetByPivotCategoryCubesOverTime();
                    foreach (var category in cc.Keys)
                    {
                        var hyperCube = cc[category].GetHyperCube();
                        foreach (var year in hyperCube.Keys)
                        {
                            var periodicityCubes = hyperCube[year];
                            foreach (var month in periodicityCubes.Keys)
                            {
                                var cube = periodicityCubes[month];
                                foreach (var consequence in Enum.GetValues(typeof(Consequence)).Cast<Consequence>().ToList())
                                {
                                    var state = cube.GetStateForConsequence(consequence);
                                    for (var i = 0; i < state.CountOfRecords; i++)
                                    {
                                        newByPivotCategoryCubesOverTime.TryGetValue(category, out value);
                                        if (value is null)
                                        {
                                            newByPivotCategoryCubesOverTime[category] = new PeriodicityCubesOverTime(category);
                                        }
                                        newByPivotCategoryCubesOverTime[category].IncrementHyperCube(year, month, consequence);
                                    }

                                }
                            }

                        }
                        //want to add this to newByPivotCategoryCubesOverTime 

                    }
                }
                //ADD all the new stuff
                foreach (var v in newByPivotCategoryCubesOverTime.Values)
                {
                    v.CommitToDatabase(evaluation);
                }

                //var previousPeriodicity = PeriodicityState.GetPeriodicityForDataTableForEvaluation(previousEvaluation, false);

                dqeRepository.EndTransactedConnection(true);

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
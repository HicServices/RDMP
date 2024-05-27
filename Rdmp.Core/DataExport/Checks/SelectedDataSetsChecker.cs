// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Logging;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.Checks;

/// <summary>
///     Checks that the <see cref="SelectedDataSets" /> can be built into a valid SQL extraction Query and that the SQL
///     generated can be executed
///     without syntax errors.
/// </summary>
public class SelectedDataSetsChecker : ICheckable
{
    private readonly bool _checkGlobals;
    private readonly IPipeline _alsoCheckPipeline;
    private readonly IBasicActivateItems _activator;

    /// <summary>
    ///     The selected dataset being checked
    /// </summary>
    public ISelectedDataSets SelectedDataSet { get; }

    /// <summary>
    ///     prepares to check the dataset as it is selected in an <see cref="ExtractionConfiguration" />.  Optionally checks an
    ///     extraction <see cref="Pipeline" /> and globals
    /// </summary>
    /// <param name="activator"></param>
    /// <param name="selectedDataSet"></param>
    /// <param name="checkGlobals"></param>
    /// <param name="alsoCheckPipeline"></param>
    public SelectedDataSetsChecker(IBasicActivateItems activator, ISelectedDataSets selectedDataSet,
        bool checkGlobals = false, IPipeline alsoCheckPipeline = null)
    {
        _checkGlobals = checkGlobals;
        _alsoCheckPipeline = alsoCheckPipeline;
        _activator = activator;
        SelectedDataSet = selectedDataSet;
    }

    /// <summary>
    ///     Checks the <see cref="SelectedDataSet" /> and reports success/failures to the <paramref name="notifier" />
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        var ds = SelectedDataSet.ExtractableDataSet;
        var config = SelectedDataSet.ExtractionConfiguration;
        var cohort = config.Cohort;
        var project = config.Project;
        const int timeout = 5;

        notifier.OnCheckPerformed(new CheckEventArgs($"Inspecting dataset {ds}", CheckResult.Success));

        var selectedcols = new List<IColumn>(config.GetAllExtractableColumnsFor(ds));

        if (!selectedcols.Any())
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Dataset {ds} in configuration '{config}' has no selected columns",
                    CheckResult.Fail));

            return;
        }

        var eis = selectedcols
            .OfType<ExtractableColumn>()
            .Select(c => c.CatalogueExtractionInformation)
            .ToArray();

        var orphans = selectedcols
            .OfType<ExtractableColumn>()
            .Where(c => c.CatalogueExtractionInformation == null)
            .ToArray();

        if (orphans.Any())
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    ErrorCodes.ExtractionInformationMissing,
                    Environment.NewLine +
                    string.Join(Environment.NewLine, orphans.Select(o => o.GetRuntimeName()).ToArray()))
            );


        WarnAboutExtractionCategory(notifier, config, ds, eis, ErrorCodes.ExtractionContainsSpecialApprovalRequired,
            ExtractionCategory.SpecialApprovalRequired);
        WarnAboutExtractionCategory(notifier, config, ds, eis, ErrorCodes.ExtractionContainsInternal,
            ExtractionCategory.Internal);
        WarnAboutExtractionCategory(notifier, config, ds, eis, ErrorCodes.ExtractionContainsDeprecated,
            ExtractionCategory.Deprecated);

        ICatalogue cata;
        try
        {
            cata = ds.Catalogue;
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Unable to find Catalogue for ExtractableDataSet",
                CheckResult.Fail, e));
            return;
        }

        if (cata.IsInternalDataset)
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"Dataset '{ds}' is marked {nameof(ICatalogue.IsInternalDataset)} so should not be extracted",
                CheckResult.Fail));

        var request = new ExtractDatasetCommand(config, cohort, new ExtractableDatasetBundle(ds),
                selectedcols, new HICProjectSalt(project), new ExtractionDirectory(project.ExtractionDirectory, config))
            { TopX = 1 };

        try
        {
            request.GenerateQueryBuilder();
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(
                new CheckEventArgs(
                    $"Could not generate valid extraction SQL for dataset {ds} in configuration {config}. {e.Message}",
                    CheckResult.Fail, e));
            return;
        }

        var server = request.GetDistinctLiveDatabaseServer();
        var serverExists = server.Exists();

        notifier.OnCheckPerformed(new CheckEventArgs($"Server {server} Exists:{serverExists}",
            serverExists ? CheckResult.Success : CheckResult.Fail));

        var cohortServer = request.ExtractableCohort.ExternalCohortTable.Discover();

        if (cohortServer == null || !cohortServer.Exists())
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Cohort server did not exist or was unreachable",
                CheckResult.Fail));
            return;
        }

        //when 2+ columns have the same Name it's a problem
        foreach (var grouping in request.ColumnsToExtract.GroupBy(c => c.GetRuntimeName()).Where(g => g.Count() > 1))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"There are {grouping.Count()} columns in the extract ({request.DatasetBundle?.DataSet}) called '{grouping.Key}'",
                CheckResult.Fail));


        // ntext and text columns don't play nicely with DISTINCT, so warn user
        var textCols = request.ColumnsToExtract.Where(IsTextDatatype).ToArray();
        if (textCols.Any())
            notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.TextColumnsInExtraction,
                string.Join(",", textCols.Select(c => c.GetRuntimeName()).ToArray())));

        //when 2+ columns have the same Order it's a problem because
        foreach (var grouping in request.ColumnsToExtract.GroupBy(c => c.Order).Where(g => g.Count() > 1))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"There are {grouping.Count()} columns in the extract ({request.DatasetBundle?.DataSet}) that share the same Order '{grouping.Key}'",
                CheckResult.Fail));

        // Warn user if stuff is out of sync with the Catalogue version (changes have happened to the master but not propagated to the copy in this extraction)
        var outOfSync = selectedcols.OfType<ExtractableColumn>().Where(c => c.IsOutOfSync()).ToArray();
        if (outOfSync.Any())
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"'{ds}' columns out of sync with CatalogueItem version(s): {Environment.NewLine + string.Join(',', outOfSync.Select(o => o + Environment.NewLine))}{Environment.NewLine} Extraction Configuration: '{config}' ",
                CheckResult.Warning));

        var nonSelectedCore = cata.GetAllExtractionInformation(ExtractionCategory.Core)
            .Union(cata.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific))
            .Where(ei => !ei.IsExtractionIdentifier && selectedcols.OfType<ExtractableColumn>()
                .All(ec => ec.CatalogueExtractionInformation_ID != ei.ID))
            .ToArray();

        if (nonSelectedCore.Any())
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"'{ds}' Core columns not selected for extractions: {Environment.NewLine + string.Join(',', nonSelectedCore.Select(o => o + Environment.NewLine))}{Environment.NewLine} Extraction Configuration: '{config}' ",
                CheckResult.Warning));

        ComplainIfUserHasHotSwappedCohort(notifier, cohort);

        //Make sure cohort and dataset are on same server before checking (can still get around this at runtime by using ExecuteCrossServerDatasetExtractionSource)
        if (!cohortServer.Server.Name.Equals(server.Name, StringComparison.CurrentCultureIgnoreCase) ||
            !cohortServer.Server.DatabaseType.Equals(server.DatabaseType))
            notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.CohortAndExtractableDatasetsAreOnDifferentServers,
                cohortServer.Server.Name, cohortServer.Server.DatabaseType, request.DatasetBundle?.DataSet, server.Name,
                server.DatabaseType));
        else
            //Try to fetch TOP 1 data
            try
            {
                using var con = server.BeginNewTransactedConnection();

                //in case user somehow manages to write a filter/transform that nukes data or something
                DbCommand cmd;

                try
                {
                    cmd = server.GetCommand(request.QueryBuilder.SQL, con);
                    cmd.CommandTimeout = timeout;
                    notifier.OnCheckPerformed(
                        new CheckEventArgs(
                            $"/*About to send Request SQL :*/{Environment.NewLine}{request.QueryBuilder.SQL}",
                            CheckResult.Success));
                }
                catch (QueryBuildingException e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs($"Failed to assemble query for dataset {ds}",
                        CheckResult.Fail, e));
                    return;
                }

                try
                {
                    using var r = cmd.ExecuteReader();
                    if (r.Read())
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"Read at least 1 row successfully from dataset {ds}",
                            CheckResult.Success));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs(
                            $"Dataset {ds} is completely empty (when linked with the cohort). Extraction may fail if the Source does not allow empty extractions",
                            CheckResult.Warning));
                }
                catch (Exception e)
                {
                    if (server.GetQuerySyntaxHelper().IsTimeout(e))
                        notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.ExtractTimeoutChecking, e, timeout));
                    else
                        notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.ExtractionFailedToExecuteTop1, e, ds));
                }

                con.ManagedTransaction.AbandonAndCloseConnection();
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.ExtractionFailedToExecuteTop1, e, ds));
            }

        var fetchOptions = _checkGlobals ? FetchOptions.ExtractableGlobalsAndLocals : FetchOptions.ExtractableLocals;

        foreach (var supportingDocument in cata.GetAllSupportingDocuments(fetchOptions))
            new SupportingDocumentsFetcher(supportingDocument).Check(notifier);

        //check catalogue locals
        foreach (var table in cata.GetAllSupportingSQLTablesForCatalogue(fetchOptions))
            new SupportingSQLTableChecker(table).Check(notifier);

        if (_alsoCheckPipeline != null)
        {
            var engine = new ExtractionPipelineUseCase(_activator, request.Project, request, _alsoCheckPipeline,
                    DataLoadInfo.Empty)
                .GetEngine(_alsoCheckPipeline, new FromCheckNotifierToDataLoadEventListener(notifier));
            engine.Check(notifier);
        }
    }

    private void ComplainIfUserHasHotSwappedCohort(ICheckNotifier notifier, IExtractableCohort cohort)
    {
        var progress = SelectedDataSet.ExtractionProgressIfAny;

        // no problem, changing cohort mid way through extraction is only a problem
        // if we are doing an iterative partial set of extractions

        // it's the first batch, thats good - user reset the progress after they changed the cohort
        // so extraction should begin at the start date correctly and cleanup any remnants
        if (progress?.ProgressDate == null) return;

        ReleasePotential rp;

        try
        {
            rp = new FlatFileReleasePotential(_activator.RepositoryLocator, SelectedDataSet);
            rp.Check(IgnoreAllErrorsCheckNotifier.Instance);
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.NoSqlAuditedForExtractionProgress, ex, progress));
            return;
        }

        if (string.IsNullOrWhiteSpace(rp.SqlExtracted))
        {
            notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.NoSqlAuditedForExtractionProgress, progress));
            return;
        }

        var whereSql = cohort.WhereSQL();

        if (!rp.SqlExtracted.Contains(whereSql))
            notifier.OnCheckPerformed(new CheckEventArgs(ErrorCodes.CohortSwappedMidExtraction, progress, whereSql));
    }

    /// <summary>
    ///     Returns true if the <paramref name="arg" /> column is ntext or text
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private bool IsTextDatatype(IColumn arg)
    {
        if (arg.ColumnInfo == null)
            return false;

        var type = arg.ColumnInfo.Data_type;

        if (string.IsNullOrEmpty(type))
            return false;

        type = type.Trim();

        return type.Equals("text", StringComparison.CurrentCultureIgnoreCase) ||
               type.Equals("ntext", StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    ///     Warns the <paramref name="notifier" /> that one or more of the <paramref name="cols" /> have the sensitive
    ///     <paramref name="category" />
    ///     and should be warned/failed about (depending on user settings).
    /// </summary>
    /// <param name="notifier"></param>
    /// <param name="configuration"></param>
    /// <param name="dataset"></param>
    /// <param name="cols"></param>
    /// <param name="errorCode"></param>
    /// <param name="category"></param>
    public static void WarnAboutExtractionCategory(ICheckNotifier notifier, IExtractionConfiguration configuration,
        IExtractableDataSet dataset, ExtractionInformation[] cols, ErrorCode errorCode, ExtractionCategory category)
    {
        if (cols.Any(c => c?.ExtractionCategory == category))
            notifier.OnCheckPerformed(new CheckEventArgs(errorCode, configuration, dataset,
                string.Join(",", cols.Where(c => c?.ExtractionCategory == category)
                    .Select(c => c.GetRuntimeName()))));
    }
}
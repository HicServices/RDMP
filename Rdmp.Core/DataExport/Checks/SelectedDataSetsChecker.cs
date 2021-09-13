// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.Logging;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using Rdmp.Core.CommandExecution;
using ReusableLibraryCode.Settings;

namespace Rdmp.Core.DataExport.Checks
{
    /// <summary>
    /// Checks that the <see cref="SelectedDataSets"/> can be built into a valid SQL extraction Query and that the SQL generated can be executed
    /// without syntax errors.
    /// </summary>
    public class SelectedDataSetsChecker : ICheckable
    {
        private readonly bool _checkGlobals;
        private readonly IPipeline _alsoCheckPipeline;
        private readonly IBasicActivateItems _activator;

        /// <summary>
        /// The selected dataset being checked
        /// </summary>
        public ISelectedDataSets SelectedDataSet { get; private set; }

        /// <summary>
        /// prepares to check the dataset as it is selected in an <see cref="ExtractionConfiguration"/>.  Optionally checks an extraction <see cref="Pipeline"/> and globals
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="selectedDataSet"></param>
        /// <param name="checkGlobals"></param>
        /// <param name="alsoCheckPipeline"></param>
        public SelectedDataSetsChecker(IBasicActivateItems activator, ISelectedDataSets selectedDataSet, bool checkGlobals = false, IPipeline alsoCheckPipeline = null)
        {
            _checkGlobals = checkGlobals;
            _alsoCheckPipeline = alsoCheckPipeline;
            _activator = activator;
            SelectedDataSet = selectedDataSet;
        }

        /// <summary>
        /// Checks the <see cref="SelectedDataSet"/> and reports success/failures to the <paramref name="notifier"/>
        /// </summary>
        /// <param name="notifier"></param>
        public void Check(ICheckNotifier notifier)
        {
            var ds = SelectedDataSet.ExtractableDataSet;
            var config = SelectedDataSet.ExtractionConfiguration;
            var cohort = config.Cohort;
            var project = config.Project;
            const int timeout = 5;

            notifier.OnCheckPerformed(new CheckEventArgs("Inspecting dataset " + ds, CheckResult.Success));

            if(ds.Catalogue.IsInternalDataset)
            {
                notifier.OnCheckPerformed(new CheckEventArgs($"Dataset '{ds}' is marked {nameof(ICatalogue.IsInternalDataset)} so should not be extracted",CheckResult.Fail));
            }

            var selectedcols = new List<IColumn>(config.GetAllExtractableColumnsFor(ds));

            if (!selectedcols.Any())
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Dataset " + ds + " in configuration '" + config + "' has no selected columns",
                        CheckResult.Fail));

                return;
            }

            ICatalogue cata;
            try
            {
                cata = ds.Catalogue;
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Unable to find Catalogue for ExtractableDataSet", CheckResult.Fail, e));
                return;
            }

            var request = new ExtractDatasetCommand( config, cohort, new ExtractableDatasetBundle(ds),
                selectedcols, new HICProjectSalt(project), new ExtractionDirectory(project.ExtractionDirectory, config)) { TopX = 1 };

            try
            {
                request.GenerateQueryBuilder();
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "Could not generate valid extraction SQL for dataset " + ds +
                        " in configuration " + config, CheckResult.Fail, e));
                return;
            }

            var server = request.GetDistinctLiveDatabaseServer();
            bool serverExists = server.Exists();

            notifier.OnCheckPerformed(new CheckEventArgs("Server " + server + " Exists:" + serverExists,
                serverExists ? CheckResult.Success : CheckResult.Fail));

            var cohortServer = request.ExtractableCohort.ExternalCohortTable.Discover();

            if (cohortServer == null || !cohortServer.Exists())
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Cohort server did not exist or was unreachable",CheckResult.Fail));
                return;
            }

            //when 2+ columns have the same Name it's a problem
            foreach (IGrouping<string, IColumn> grouping in request.ColumnsToExtract.GroupBy(c=>c.GetRuntimeName()).Where(g=>g.Count()>1))
                notifier.OnCheckPerformed(new CheckEventArgs($"There are { grouping.Count() } columns in the extract ({request.DatasetBundle?.DataSet}) called '{ grouping.Key }'",CheckResult.Fail));

            //when 2+ columns have the same Order it's a problem because
            foreach (IGrouping<int, IColumn> grouping in request.ColumnsToExtract.GroupBy(c=>c.Order).Where(g=>g.Count()>1))
                notifier.OnCheckPerformed(new CheckEventArgs($"There are { grouping.Count() } columns in the extract ({request.DatasetBundle?.DataSet}) that share the same Order '{ grouping.Key }'",CheckResult.Fail));

            // Warn user if stuff is out of sync with the Catalogue version (changes have happened to the master but not propgated to the copy in this extraction)
            var outOfSync = selectedcols.OfType<ExtractableColumn>().Where(c => c.IsOutOfSync()).ToArray();
            if(outOfSync.Any())
                notifier.OnCheckPerformed(new CheckEventArgs($"'{ds}' columns out of sync with CatalogueItem version(s): { Environment.NewLine + string.Join(',', outOfSync.Select(o => o.ToString() + Environment.NewLine)) }" +
                    $"{ Environment.NewLine } Extraction Configuration: '{config}' ", CheckResult.Warning));

            var nonSelectedCore = cata.GetAllExtractionInformation(ExtractionCategory.Core)
                                      .Union(cata.GetAllExtractionInformation(ExtractionCategory.ProjectSpecific))
                                      .Where(ei => !ei.IsExtractionIdentifier &&
                                                   !selectedcols.OfType<ExtractableColumn>().Any(ec => ec.CatalogueExtractionInformation_ID == ei.ID))
                                      .ToArray();

            if (nonSelectedCore.Any())
                notifier.OnCheckPerformed(new CheckEventArgs($"'{ds}' Core columns not selected for extractions: { Environment.NewLine + string.Join(',', nonSelectedCore.Select(o => o.ToString() + Environment.NewLine)) }" +
                    $"{ Environment.NewLine } Extraction Configuration: '{config}' ", CheckResult.Warning));

            //Make sure cohort and dataset are on same server before checking (can still get around this at runtime by using ExecuteCrossServerDatasetExtractionSource)
            if (!cohortServer.Server.Name.Equals(server.Name,StringComparison.CurrentCultureIgnoreCase) || !cohortServer.Server.DatabaseType.Equals(server.DatabaseType))
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Cohort is on server '{cohortServer.Server.Name}' ({cohortServer.Server.DatabaseType}) but dataset '{request.DatasetBundle?.DataSet}' is on '{server.Name}' ({server.DatabaseType})"
                    , CheckResult.Warning));
            }
            else
            {
                //Try to fetch TOP 1 data
                try
                {
                    using (var con = server.BeginNewTransactedConnection())
                    {
                        //incase user somehow manages to write a filter/transform that nukes data or something

                        DbCommand cmd;

                        try
                        {
                            cmd = server.GetCommand(request.QueryBuilder.SQL, con);
                            cmd.CommandTimeout = timeout;
                            notifier.OnCheckPerformed(
                                new CheckEventArgs(
                                    "/*About to send Request SQL :*/" + Environment.NewLine + request.QueryBuilder.SQL,
                                    CheckResult.Success));
                        }
                        catch (QueryBuildingException e)
                        {
                            notifier.OnCheckPerformed(new CheckEventArgs("Failed to assemble query for dataset " + ds,
                                CheckResult.Fail, e));
                            return;
                        }
                    
                        try
                        {
                            using (var r = cmd.ExecuteReader())
                            {
                                if (r.Read())
                                    notifier.OnCheckPerformed(new CheckEventArgs("Read at least 1 row successfully from dataset " + ds,
                                        CheckResult.Success));
                                else
                                    notifier.OnCheckPerformed(new CheckEventArgs("Dataset " + ds + " is completely empty (when linked with the cohort). " +
                                                                                 "Extraction may fail if the Source does not allow empty extractions",
                                        CheckResult.Warning));
                            }
                        }
                        catch (Exception e)
                        {
                            if (server.GetQuerySyntaxHelper().IsTimeout(e))
                            {
                                if (UserSettings.WarnOnTimeoutOnExtractionChecks)
                                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to read rows after " + timeout + "s", CheckResult.Warning, e));
                            }
                            else
                                notifier.OnCheckPerformed(new CheckEventArgs("Failed to execute the query (See below for query)", CheckResult.Fail, e));
                        }

                        con.ManagedTransaction.AbandonAndCloseConnection();
                    }
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs("Failed to execute Top 1 on dataset " + ds, CheckResult.Fail, e));
                }
            }

            var fetchOptions = _checkGlobals ? FetchOptions.ExtractableGlobalsAndLocals : FetchOptions.ExtractableLocals;

            foreach (var supportingDocument in cata.GetAllSupportingDocuments(fetchOptions))
                new SupportingDocumentsFetcher(supportingDocument).Check(notifier);

            //check catalogue locals
            foreach (SupportingSQLTable table in cata.GetAllSupportingSQLTablesForCatalogue(fetchOptions))
                new SupportingSQLTableChecker(table).Check(notifier);

            if (_alsoCheckPipeline != null)
            {
                var engine = new ExtractionPipelineUseCase(_activator,request.Project, request, _alsoCheckPipeline, DataLoadInfo.Empty)
                                    .GetEngine(_alsoCheckPipeline, new FromCheckNotifierToDataLoadEventListener(notifier));
                engine.Check(notifier);
            }
        }
    }
}
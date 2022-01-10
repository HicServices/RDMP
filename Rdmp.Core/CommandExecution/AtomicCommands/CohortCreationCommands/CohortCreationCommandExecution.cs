// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.Providers;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public abstract class CohortCreationCommandExecution : BasicCommandExecution, IAtomicCommandWithTarget
    {
        protected const string Desc_ExternalCohortTableParameter = "Destination cohort database in which to store identifiers";
        protected const string Desc_CohortNameParameter = "Name for cohort.  If the named cohort already exists then a new Version will be assumed e.g. Version 2";
        protected const string Desc_ProjectParameter = "Project to associate cohort with, must have a ProjectNumber";

        protected ExternalCohortTable ExternalCohortTable;
        protected IProject Project;
        protected IPipeline Pipeline;
        private string _explicitCohortName;

        /// <summary>
        /// Initialises base class with no targetting parameters, these will be prompted from the user at execution time assuming <see cref="IBasicActivateItems.IsInteractive"/>
        /// </summary>
        /// <param name="activator"></param>
        protected CohortCreationCommandExecution(IBasicActivateItems activator)
            : this(activator, null, null, null, null)
        {

        }

        /// <summary>
        /// Initialises common targetting parameters (where to store resulting identifiers etc)
        /// </summary>
        /// <param name="activator"></param>
        /// <param name="externalCohortTable"></param>
        /// <param name="cohortName"></param>
        /// <param name="project"></param>
        /// <param name="pipeline"></param>
        protected CohortCreationCommandExecution(IBasicActivateItems activator, ExternalCohortTable externalCohortTable, string cohortName, Project project, IPipeline pipeline) : base(activator)
        {
            var dataExport = activator.CoreChildProvider as DataExportChildProvider;

            //May be null
            _explicitCohortName = cohortName;
            ExternalCohortTable = externalCohortTable;
            Project = project;
            Pipeline = pipeline;

            if (dataExport == null)
            {
                SetImpossible("No data export repository available");
                return;
            }

            if (!dataExport.CohortSources.Any())
                SetImpossible("There are no cohort sources configured, you must create one in the Saved Cohort tabs");
        }
        protected ICohortCreationRequest GetCohortCreationRequest(string auditLogDescription)
        {
            //user wants to create a new cohort
            var ect = ExternalCohortTable;

            //do we know where it's going to end up?
            if (ect == null)
                if (!SelectOne(
                    GetChooseCohortDialogArgs(),
                    BasicActivator.RepositoryLocator.DataExportRepository,
                    out ect)) //not yet, get user to pick one
                {
                    return null;//user didn't select one and cancelled dialog
                }
                    

            //and document the request

            // if we have everything we need to create the cohort right here
            if (!string.IsNullOrWhiteSpace(_explicitCohortName) && Project?.ProjectNumber != null)
                return GenerateCohortCreationRequestFromNameAndProject(_explicitCohortName, auditLogDescription,ect);
            else
            {
                // otherwise we are going to have to ask the user for it

                //Get a new request for the source they are trying to populate
                var req = BasicActivator.GetCohortCreationRequest(ect, Project, auditLogDescription);

                if (Project == null)
                    Project = req?.Project;

                return req;
            }
        }

        /// <summary>
        /// Describes in a user friendly way the activity of picking an <see cref="ExternalCohortTable"/>
        /// </summary>
        /// <returns></returns>
        public static DialogArgs GetChooseCohortDialogArgs()
        {
            return new DialogArgs()
            {
                WindowTitle = "Choose where to save cohort",
                TaskDescription = "Select the Cohort Database in which to store the identifiers.  If you have multiple methods of anonymising cohorts or manage different types of identifiers (e.g. CHI lists, ECHI lists and/or BarcodeIDs) then you must pick the Cohort Database that matches your cohort identifier type/anonymisation protocol.",
                EntryLabel = "Select Cohort Database",
                AllowAutoSelect = true,
            };
        }

        private ICohortCreationRequest GenerateCohortCreationRequestFromNameAndProject(string name, string auditLogDescription,ExternalCohortTable ect)
        {
            var existing = ExtractableCohort.GetImportableCohortDefinitions(ect).Where(d => d.Description.Equals(_explicitCohortName)).ToArray();
            var version = 1;

            // If the user has used this description before then we can just bump the version by 1
            if (existing != null && existing.Any())
            {
                version = existing.Max(v => v.Version) + 1;
            }

            return new CohortCreationRequest(Project, new CohortDefinition(null, name, version, Project.ProjectNumber.Value, ect), BasicActivator.RepositoryLocator.DataExportRepository, auditLogDescription);
        }

        public virtual IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is Project)
                Project = (Project)target;

            if (target is ExternalCohortTable)
                ExternalCohortTable = (ExternalCohortTable)target;

            return this;
        }


        protected IPipelineRunner GetConfigureAndExecuteControl(ICohortCreationRequest request, string description)
        {
            var catalogueRepository = BasicActivator.RepositoryLocator.CatalogueRepository;

            var pipelineRunner = BasicActivator.GetPipelineRunner(request, Pipeline);

            pipelineRunner.PipelineExecutionFinishedsuccessfully += (o, args) => OnCohortCreatedSuccessfully(pipelineRunner, request);

            //add in the logging server
            var loggingServer = catalogueRepository.GetServerDefaults().GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer != null)
            {
                var logManager = new LogManager(loggingServer);
                logManager.CreateNewLoggingTaskIfNotExists(ExtractableCohort.CohortLoggingTask);

                //create a db listener 
                var toDbListener = new ToLoggingDatabaseDataLoadEventListener(this, logManager, ExtractableCohort.CohortLoggingTask, description);

                //make all messages go to both the db and the UI
                pipelineRunner.SetAdditionalProgressListener(toDbListener);

                //after executing the pipeline finalise the db listener table info records
                pipelineRunner.PipelineExecutionFinishedsuccessfully += (s, e) => toDbListener.FinalizeTableLoadInfos();
            }

            return pipelineRunner;
        }

        private void OnCohortCreatedSuccessfully(IPipelineRunner runner, ICohortCreationRequest request)
        {
            if (request.CohortCreatedIfAny != null)
                Publish(request.CohortCreatedIfAny);
        }
    }
}

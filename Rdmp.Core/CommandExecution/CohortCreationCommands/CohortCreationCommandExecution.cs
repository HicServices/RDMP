// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.Core.Providers;

namespace Rdmp.Core.CommandExecution.CohortCreationCommands
{
    public abstract class CohortCreationCommandExecution : BasicCommandExecution, IAtomicCommandWithTarget
    {
        protected ExternalCohortTable ExternalCohortTable;
        protected IProject Project;

        protected CohortCreationCommandExecution(IBasicActivateItems activator) : base(activator)
        {
            var dataExport = activator.CoreChildProvider as DataExportChildProvider;

            if (dataExport == null)
            {
                SetImpossible("No data export repository available");
                return;
            }

            if (!dataExport.CohortSources.Any())
                SetImpossible("There are no cohort sources configured, you must create one in the Saved Cohort tabs");
        }
        protected ICohortCreationRequest GetCohortCreationRequest(string cohortInitialDescription)
        {
            //user wants to create a new cohort

            //do we know where it's going to end up?
            if (ExternalCohortTable == null)
                if (!SelectOne(BasicActivator.RepositoryLocator.DataExportRepository, out ExternalCohortTable, null, true)) //not yet, get user to pick one
                    return null;//user didn't select one and cancelled dialog

            //and document the request
            

            //Get a new request for the source they are trying to populate
             var req = BasicActivator.GetCohortCreationRequest(ExternalCohortTable,Project, cohortInitialDescription);

            if (Project == null)
                Project = req.Project;

            return req;
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

            var pipelineRunner = BasicActivator.GetPipelineRunner(request,null);

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

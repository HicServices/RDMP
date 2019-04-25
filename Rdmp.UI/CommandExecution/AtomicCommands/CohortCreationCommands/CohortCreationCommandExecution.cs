// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Defaults;
using Rdmp.Core.CohortComitting.Pipeline;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.Providers;
using Rdmp.Core.Logging;
using Rdmp.Core.Logging.Listeners;
using Rdmp.UI.CohortUI.ImportCustomData;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public abstract class CohortCreationCommandExecution :BasicUICommandExecution,IAtomicCommandWithTarget
    {
        protected ExternalCohortTable ExternalCohortTable;
        protected Project Project;
        
        protected CohortCreationCommandExecution(IActivateItems activator) : base(activator)
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
        protected CohortCreationRequest GetCohortCreationRequest(string cohortInitialDescription)
        {
            //user wants to create a new cohort

            //do we know where it's going to end up?
            if (ExternalCohortTable == null)
                if (!SelectOne(Activator.RepositoryLocator.DataExportRepository, out ExternalCohortTable)) //not yet, get user to pick one
                    return null;//user didn't select one and cancelled dialog
            
            //and document the request

            //Get a new request for the source they are trying to populate
            CohortCreationRequestUI requestUI = new CohortCreationRequestUI(Activator,ExternalCohortTable, Project);

            if(!string.IsNullOrWhiteSpace(cohortInitialDescription))
                requestUI.CohortDescription = cohortInitialDescription + " (" + Environment.UserName + " - " + DateTime.Now + ")";

            if (requestUI.ShowDialog() != DialogResult.OK)
                return null;

            if (Project == null)
                Project = requestUI.Project;

            return requestUI.Result;
        }

        public virtual IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if (target is Project)
                Project = (Project)target;

            if (target is ExternalCohortTable)
                ExternalCohortTable = (ExternalCohortTable) target;

            return this;
        }


        protected ConfigureAndExecutePipelineUI GetConfigureAndExecuteControl(CohortCreationRequest request, string description)
        {
            var catalogueRepository = Activator.RepositoryLocator.CatalogueRepository;
            
            ConfigureAndExecutePipelineUI configureAndExecuteDialog = new ConfigureAndExecutePipelineUI(request,Activator);
            configureAndExecuteDialog.Dock = DockStyle.Fill;
            
            configureAndExecuteDialog.PipelineExecutionFinishedsuccessfully += (o, args) => OnCohortCreatedSuccessfully(configureAndExecuteDialog, request);

            //add in the logging server
            var loggingServer = catalogueRepository.GetServerDefaults().GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);

            if (loggingServer != null)
            {
                var logManager = new LogManager(loggingServer);
                logManager.CreateNewLoggingTaskIfNotExists(ExtractableCohort.CohortLoggingTask);

                //create a db listener 
                var toDbListener = new ToLoggingDatabaseDataLoadEventListener(this, logManager,ExtractableCohort.CohortLoggingTask, description);

                //make all messages go to both the db and the UI
                configureAndExecuteDialog.SetAdditionalProgressListener(toDbListener);

                //after executing the pipeline finalise the db listener table info records
                configureAndExecuteDialog.PipelineExecutionFinishedsuccessfully += (s,e)=>toDbListener.FinalizeTableLoadInfos();
            }

            return configureAndExecuteDialog;
        }

        private void OnCohortCreatedSuccessfully(ContainerControl responsibleControl, CohortCreationRequest request)
        {
            if (responsibleControl.InvokeRequired)
            {
                responsibleControl.Invoke(new MethodInvoker(() => OnCohortCreatedSuccessfully(responsibleControl, request)));
                return;
            }

            if (request.CohortCreatedIfAny != null)
                Publish(request.CohortCreatedIfAny);

            if (MessageBox.Show("Pipeline reports it has successfully loaded the cohort, would you like to close the Form?", "Successfully Created Cohort", MessageBoxButtons.YesNo) == DialogResult.Yes)
                responsibleControl.ParentForm.Close();
        }

        public abstract Image GetImage(IIconProvider iconProvider);
    }
}

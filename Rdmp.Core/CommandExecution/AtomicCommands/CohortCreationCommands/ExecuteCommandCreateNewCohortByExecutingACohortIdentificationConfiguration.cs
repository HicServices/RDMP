// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands
{
    /// <summary>
    /// Generates and runs an SQL query based on a <see cref="CohortIdentificationConfiguration"/> and pipes the resulting private identifier list to create a new <see cref="ExtractableCohort"/>.
    /// </summary>
    public class ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration : CohortCreationCommandExecution
    {
        private CohortIdentificationConfiguration _cic;
        private CohortIdentificationConfiguration[] _allConfigurations;

        public ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(IBasicActivateItems activator, ExternalCohortTable externalCohortTable) :
            this(activator, null, externalCohortTable, null, null, null)
        {
            _allConfigurations = activator.CoreChildProvider.AllCohortIdentificationConfigurations;

            if (!_allConfigurations.Any())
                SetImpossible("You do not have any CohortIdentificationConfigurations yet, you can create them through the 'Cohorts Identification Toolbox' accessible through Window=>Cohort Identification");

            UseTripleDotSuffix = true;
        }

        [UseWithObjectConstructor]
        public ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(IBasicActivateItems activator,
            [DemandsInitialization("The cohort builder query that should be executed")]
            CohortIdentificationConfiguration cic,
            [DemandsInitialization(Desc_ExternalCohortTableParameter)]
            ExternalCohortTable ect,
            [DemandsInitialization(Desc_CohortNameParameter)]
            string cohortName,
            [DemandsInitialization(Desc_ProjectParameter)]
            Project project,
            [DemandsInitialization("Pipeline for executing the query, performing any required transforms on the output list and allocating release identifiers")]
            IPipeline pipeline) :
            base(activator, ect, cohortName, project, pipeline)
        {
            _cic = cic;
        }

        public override string GetCommandHelp()
        {
            return "Run the cohort identification configuration (query) and save the resulting final cohort identifier list into a saved cohort database";
        }

        public override void Execute()
        {
            base.Execute();

            if (_cic == null)
                _cic = (CohortIdentificationConfiguration)BasicActivator.SelectOne("Select Cohort Builder Query", BasicActivator.GetAll<CohortIdentificationConfiguration>().ToArray());

            if (_cic == null)
                return;
            
            var request = GetCohortCreationRequest("Patients in CohortIdentificationConfiguration '" + _cic + "' (ID=" + _cic.ID + ")");

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            request.CohortIdentificationConfiguration = _cic;

            var configureAndExecute = GetConfigureAndExecuteControl(request, "Execute CIC " + _cic + " and commmit results");

            configureAndExecute.PipelineExecutionFinishedsuccessfully += OnImportCompletedSuccessfully;

            configureAndExecute.Run(BasicActivator.RepositoryLocator, null, null, null);
        }

        void OnImportCompletedSuccessfully(object sender, PipelineEngineEventArgs u)
        {
            //see if we can associate the cic with the project
            var cmd = new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(BasicActivator).SetTarget((Project)Project).SetTarget(_cic);

            //we can!
            if (!cmd.IsImpossible)
                cmd.Execute();
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration, OverlayKind.Import);
        }

        public override IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            base.SetTarget(target);

            if (target is CohortIdentificationConfiguration)
                _cic = (CohortIdentificationConfiguration)target;

            return this;
        }
    }
}

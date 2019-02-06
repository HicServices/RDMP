// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace DataExportManager.CommandExecution.AtomicCommands.CohortCreationCommands
{
    public class ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration:CohortCreationCommandExecution
    {
        private CohortIdentificationConfiguration _cic;
        private CohortIdentificationConfiguration[] _allConfigurations;

        public ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(IActivateItems activator,ExternalCohortTable externalCohortTable = null) : base(activator)
        {
            _allConfigurations = activator.CoreChildProvider.AllCohortIdentificationConfigurations;
            ExternalCohortTable = externalCohortTable;

            if (!_allConfigurations.Any())
                SetImpossible("You do not have any CohortIdentificationConfigurations yet, you can create them through the 'Cohorts Identification Toolbox' accessible through Window=>Cohort Identification");
        }

        public override string GetCommandHelp()
        {
            return "Run the cohort identification configuration (query) and save the resulting final cohort identifier list into a saved cohort database";
        }

        public override void Execute()
        {
            base.Execute();

            if(_cic == null)
                if(!SelectOne(Activator.RepositoryLocator.CatalogueRepository,out _cic))
                    return;

            var request = GetCohortCreationRequest("Patients in CohortIdentificationConfiguration '" + _cic  +"' (ID=" +_cic.ID +")" );

            //user choose to cancel the cohort creation request dialogue
            if (request == null)
                return;

            request.CohortIdentificationConfiguration = _cic;

            var configureAndExecute = GetConfigureAndExecuteControl(request, "Execute CIC " + _cic + " and commmit results");
            
            configureAndExecute.PipelineExecutionFinishedsuccessfully += OnImportCompletedSuccessfully;

            Activator.ShowWindow(configureAndExecute);
        }

        void OnImportCompletedSuccessfully(object sender, CatalogueLibrary.DataFlowPipeline.Events.PipelineEngineEventArgs args)
        {
            //see if we can associate the cic with the project
            var cmd = new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(Activator).SetTarget(Project).SetTarget(_cic);

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
                _cic = (CohortIdentificationConfiguration) target;

            return this;
        }
    }
}

// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandExecuteExtractionConfiguration:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private ExtractionConfiguration _extractionConfiguration;
        private SelectedDataSets _selectedDataSet;

        [ImportingConstructor]
        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, ExtractionConfiguration extractionConfiguration) : this(activator)
        {
            _extractionConfiguration = extractionConfiguration;
        }

        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator) : base(activator)
        {
            OverrideCommandName = "Extract...";
        }

        public ExecuteCommandExecuteExtractionConfiguration(IActivateItems activator, SelectedDataSets selectedDataSet) : this(activator)
        {
            _extractionConfiguration = (ExtractionConfiguration)selectedDataSet.ExtractionConfiguration;
            _selectedDataSet = selectedDataSet;

        }

        public override string GetCommandHelp()
        {
            return "Extract all the datasets in the configuration linking each against the configuration's cohort";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.ExtractionConfiguration,OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _extractionConfiguration = (ExtractionConfiguration)target;

            //must have datasets, must not be released and must have a cohort

            if(_extractionConfiguration.IsReleased)
            {
                SetImpossible("ExtractionConfiguration is released so cannot be executed");
                return this;
            }

            if(_extractionConfiguration.Cohort_ID == null)
            {
                SetImpossible("No cohort has been configured for ExtractionConfiguration");
                return this;
            }

            if (!_extractionConfiguration.GetAllExtractableDataSets().Any())
                SetImpossible("ExtractionConfiguration does not have an selected datasets");

            return this;
        }

        public override void Execute()
        {
            base.Execute();
            var ui = Activator.Activate<ExecuteExtractionUI, ExtractionConfiguration>(_extractionConfiguration);

            if (_selectedDataSet != null)
                ui.TickAllFor(_selectedDataSet);
        }
    }
}

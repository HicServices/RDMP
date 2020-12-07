// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.ProjectUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewSelectedDataSetsExtractionSql:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private ExtractionConfiguration _extractionConfiguration;
        private SelectedDataSets _selectedDataSet;

        [UseWithObjectConstructor]
        public ExecuteCommandViewSelectedDataSetsExtractionSql(IActivateItems activator,ExtractionConfiguration extractionConfiguration)
            : base(activator)
        {
            _extractionConfiguration = extractionConfiguration;
        }

        public ExecuteCommandViewSelectedDataSetsExtractionSql(IActivateItems activator) : base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "Shows the SQL that will be executed for the given dataset when it is extracted including the linkage with the cohort table";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.SQL,OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            if(target is SelectedDataSets)
            {
                _selectedDataSet =  target as SelectedDataSets;

                if (_selectedDataSet != null)
                    //must have datasets and have a cohort configured
                    if (_selectedDataSet.ExtractionConfiguration.Cohort_ID == null)
                        SetImpossible("No cohort has been selected for ExtractionConfiguration");
            }

            if(target is ExtractionConfiguration)
                _extractionConfiguration = target as ExtractionConfiguration;
            
            return this;
        }

        public override void Execute()
        {
            base.Execute();

            if (_selectedDataSet == null && _extractionConfiguration != null)
                _selectedDataSet = SelectOne(Activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<SelectedDataSets>(_extractionConfiguration));

            if(_selectedDataSet == null)
                return;

            Activator.Activate<ViewExtractionConfigurationSQLUI, SelectedDataSets>(_selectedDataSet);
        }
    }
}

// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.SubComponents;

namespace Rdmp.UI.Menus
{

    [System.ComponentModel.DesignerCategory("")]
    class CohortIdentificationConfigurationMenu :RDMPContextMenuStrip
    {
        private CohortIdentificationConfiguration _cic;
        private IAtomicCommandWithTarget _executeAndImportCommand;
        private IAtomicCommandWithTarget _executeCommandClone;

        public CohortIdentificationConfigurationMenu(RDMPContextMenuStripArgs args, CohortIdentificationConfiguration cic): base(args, cic)
        {
            _cic = cic;

            Items.Add("View SQL", _activator.CoreIconProvider.GetImage(RDMPConcept.SQL), (s, e) => _activator.Activate<ViewCohortIdentificationConfigurationUI, CohortIdentificationConfiguration>(cic));
                
            Items.Add(new ToolStripSeparator());

            _executeAndImportCommand = new ExecuteCommandCreateNewCohortByExecutingACohortIdentificationConfiguration(_activator).SetTarget(cic);
            
            Add(_executeAndImportCommand);
            
            //associate with project
            Add(new ExecuteCommandAssociateCohortIdentificationConfigurationWithProject(_activator).SetTarget(cic));
            
            Items.Add(new ToolStripSeparator());

            _executeCommandClone = new ExecuteCommandCloneCohortIdentificationConfiguration(_activator).SetTarget(cic);
            Add(_executeCommandClone);

            Add(new ExecuteCommandFreezeCohortIdentificationConfiguration(_activator, cic, !cic.Frozen));
            
            Items.Add(new ToolStripSeparator());

            Add(new ExecuteCommandCreateNewCohortIdentificationConfiguration(_activator));
        }

        public CohortIdentificationConfigurationMenu(RDMPContextMenuStripArgs args, ProjectCohortIdentificationConfigurationAssociation association) : this(args,association.CohortIdentificationConfiguration)
        {
            _executeAndImportCommand.SetTarget((DatabaseEntity)association.Project);
            _executeCommandClone.SetTarget((DatabaseEntity)association.Project);
        }
        
    }
}

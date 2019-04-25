// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using System.Drawing;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.DataQualityUIs;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    public class ExecuteCommandRunDQEOnCatalogue:BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private Catalogue _catalogue;
        
        [ImportingConstructor]
        public ExecuteCommandRunDQEOnCatalogue(IActivateItems activator,Catalogue catalogue): base(activator)
        {
            _catalogue = catalogue;
        }

        public ExecuteCommandRunDQEOnCatalogue(IActivateItems activator):base(activator)
        {
        }

        public override string GetCommandHelp()
        {
            return "Runs the data quality engine on the dataset using the currently configured validation rules and stores the results in the default DQE results database";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.DQE, OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            _catalogue = (Catalogue) target;
            return this;
        }

        public override void Execute()
        {
            base.Execute();
            Activator.Activate<DQEExecutionControlUI, Catalogue>(_catalogue);
        }

        public override string GetCommandName()
        {
            return "Data Quality Engine";
        }
    }
}

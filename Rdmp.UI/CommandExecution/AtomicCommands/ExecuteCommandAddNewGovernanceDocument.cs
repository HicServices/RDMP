// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Rdmp.Core.CatalogueLibrary.Data.Governance;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandAddNewGovernanceDocument : BasicUICommandExecution,IAtomicCommand
    {
        private readonly GovernancePeriod _period;
        private FileInfo _file;

        public ExecuteCommandAddNewGovernanceDocument(IActivateItems activator,GovernancePeriod period) : base(activator)
        {
            _period = period;
        }

        public ExecuteCommandAddNewGovernanceDocument(IActivateItems activator, GovernancePeriod period,FileInfo file): base(activator)
        {
            _period = period;
            _file = file;
        }

        public override void Execute()
        {
            base.Execute();

            if (_file == null)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if(ofd.ShowDialog() == DialogResult.OK)
                    _file = new FileInfo(ofd.FileName);
            }

            if(_file == null)
                return;

            var doc = new GovernanceDocument(Activator.RepositoryLocator.CatalogueRepository, _period, _file);
            Publish(_period);
            Emphasise(doc);
            Activate(doc);
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.GovernanceDocument, OverlayKind.Add);
        }
    }
}

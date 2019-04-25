// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.CatalogueLibrary.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.Menus
{
    class DataAccessCredentialUsageNodeMenu : RDMPContextMenuStrip
    {
        public DataAccessCredentialUsageNodeMenu(RDMPContextMenuStripArgs args, DataAccessCredentialUsageNode node): base(args, node)
        {
            var setUsage = new ToolStripMenuItem("Set Context");

            var existingUsages = _activator.RepositoryLocator.CatalogueRepository.TableInfoCredentialsManager.GetCredentialsIfExistsFor(node.TableInfo);

            foreach (DataAccessContext context in Enum.GetValues(typeof(DataAccessContext)))
                Add(new ExecuteCommandSetDataAccessContextForCredentials(_activator, node, context, existingUsages), Keys.None, setUsage);
            
            Items.Add(setUsage);
        }
    }
}
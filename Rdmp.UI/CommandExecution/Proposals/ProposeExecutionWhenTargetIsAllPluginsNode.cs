// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Providers.Nodes;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying.Commands;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsAllPluginsNode : RDMPCommandExecutionProposal<AllPluginsNode>
    {
        public ProposeExecutionWhenTargetIsAllPluginsNode(IActivateItems itemActivator) : base(itemActivator)
        {
            
        }

        public override void Activate(AllPluginsNode target)
        {
            if(ItemActivator.RepositoryLocator.CatalogueRepository.MEF.DownloadDirectory.Exists)
                UsefulStuff.GetInstance().ShowFolderInWindowsExplorer(ItemActivator.RepositoryLocator.CatalogueRepository.MEF.DownloadDirectory);
        }

        public override bool CanActivate(AllPluginsNode target)
        {
            return true;
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, AllPluginsNode target, InsertOption insertOption = InsertOption.Default)
        {
            //drop files on to attempt to upload plugins
            if(cmd is FileCollectionCommand f)
                return new ExecuteCommandAddPlugins(ItemActivator,f);

            return null;
        }
    }
}

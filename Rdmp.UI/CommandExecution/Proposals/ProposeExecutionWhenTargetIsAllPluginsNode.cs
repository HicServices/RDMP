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

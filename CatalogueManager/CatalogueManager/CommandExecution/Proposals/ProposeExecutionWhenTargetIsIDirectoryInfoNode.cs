using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.Proposals;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsIDirectoryInfoNode:RDMPCommandExecutionProposal<IDirectoryInfoNode>
    {
        public ProposeExecutionWhenTargetIsIDirectoryInfoNode(IActivateItems itemActivator): base(itemActivator)
        {
        }

        public override bool CanActivate(IDirectoryInfoNode target)
        {
            return target.GetDirectoryInfoIfAny() != null;
        }

        public override void Activate(IDirectoryInfoNode target)
        {
            new ExecuteCommandOpenInExplorer(ItemActivator,target.GetDirectoryInfoIfAny()).Execute();
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, IDirectoryInfoNode target, InsertOption insertOption = InsertOption.Default)
        {
            //no drag and drop support
            return null;
        }
    }
}

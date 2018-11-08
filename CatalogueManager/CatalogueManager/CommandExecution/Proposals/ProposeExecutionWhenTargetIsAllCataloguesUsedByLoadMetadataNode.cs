using System;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.Copying.Commands;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsAllCataloguesUsedByLoadMetadataNode : RDMPCommandExecutionProposal<AllCataloguesUsedByLoadMetadataNode>
    {
        public ProposeExecutionWhenTargetIsAllCataloguesUsedByLoadMetadataNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(AllCataloguesUsedByLoadMetadataNode target)
        {
            return false;
        }

        public override void Activate(AllCataloguesUsedByLoadMetadataNode target)
        {
            throw new NotSupportedException();
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, AllCataloguesUsedByLoadMetadataNode target,InsertOption insertOption = InsertOption.Default)
        {
            var cata = cmd as CatalogueCommand;
            var manyCata = cmd as ManyCataloguesCommand;

            ICommandExecution cmdExecution = null;

            if (cata != null)
                cmdExecution = new ExecuteCommandAssociateCatalogueWithLoadMetadata(ItemActivator,target.LoadMetadata).SetTarget(new[]{cata.Catalogue});

            if(manyCata != null)
                cmdExecution = new ExecuteCommandAssociateCatalogueWithLoadMetadata(ItemActivator, target.LoadMetadata).SetTarget(manyCata.Catalogues);


            return cmdExecution;
        }
    }
}
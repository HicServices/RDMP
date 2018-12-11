using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.Sharing;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs;
using CatalogueManager.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCatalogue:RDMPCommandExecutionProposal<Catalogue>
    {
        public ProposeExecutionWhenTargetIsCatalogue(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Catalogue target)
        {
            return true;
        }

        public override void Activate(Catalogue c)
        {
            ItemActivator.Activate<CatalogueTab, Catalogue>(c);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Catalogue targetCatalogue, InsertOption insertOption = InsertOption.Default)
        {
            var sourceFileCollection = cmd as FileCollectionCommand;

            if(sourceFileCollection != null)
                if (sourceFileCollection.IsShareDefinition)
                    return new ExecuteCommandImportCatalogueDescriptionsFromShare(ItemActivator, sourceFileCollection,targetCatalogue);
                else
                    return new ExecuteCommandAddNewSupportingDocument(ItemActivator, sourceFileCollection, targetCatalogue);

            return null;
        }
    }
}

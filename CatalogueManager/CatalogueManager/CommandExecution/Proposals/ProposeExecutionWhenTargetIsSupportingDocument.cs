using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsSupportingDocument : RDMPCommandExecutionProposal<SupportingDocument>
    {
        public ProposeExecutionWhenTargetIsSupportingDocument(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(SupportingDocument target)
        {
            return true;
        }

        public override void Activate(SupportingDocument target)
        {
            ItemActivator.Activate<SupportingDocumentUI, SupportingDocument>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, SupportingDocument target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
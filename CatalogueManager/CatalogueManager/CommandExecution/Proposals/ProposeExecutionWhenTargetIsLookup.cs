using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.JoinsAndLookups;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsLookup : RDMPCommandExecutionProposal<Lookup>
    {
        public ProposeExecutionWhenTargetIsLookup(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(Lookup target)
        {
            return true;
        }

        public override void Activate(Lookup target)
        {
            ItemActivator.Activate<LookupUI, Lookup>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, Lookup target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
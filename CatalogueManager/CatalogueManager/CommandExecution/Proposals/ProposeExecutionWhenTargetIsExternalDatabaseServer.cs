using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.MainFormUITabs.SubComponents;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsExternalDatabaseServer : RDMPCommandExecutionProposal<ExternalDatabaseServer>
    {
        public ProposeExecutionWhenTargetIsExternalDatabaseServer(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(ExternalDatabaseServer target)
        {
            return true;
        }

        public override void Activate(ExternalDatabaseServer target)
        {
            ItemActivator.Activate<ExternalDatabaseServerUI, ExternalDatabaseServer>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExternalDatabaseServer target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
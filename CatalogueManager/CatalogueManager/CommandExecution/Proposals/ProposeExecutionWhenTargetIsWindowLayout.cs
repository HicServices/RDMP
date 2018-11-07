using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.DashboardTabs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsWindowLayout : RDMPCommandExecutionProposal<WindowLayout>
    {
        public ProposeExecutionWhenTargetIsWindowLayout(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(WindowLayout target)
        {
            return true;
        }

        public override void Activate(WindowLayout target)
        {
            ItemActivator.WindowArranger.Setup(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, WindowLayout target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
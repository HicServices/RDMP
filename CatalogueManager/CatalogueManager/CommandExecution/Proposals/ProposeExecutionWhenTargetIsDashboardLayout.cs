using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.DashboardTabs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsDashboardLayout:RDMPCommandExecutionProposal<DashboardLayout>
    {
        public ProposeExecutionWhenTargetIsDashboardLayout(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(DashboardLayout target)
        {
            return true;
        }

        public override void Activate(DashboardLayout target)
        {
            ItemActivator.Activate<DashboardLayoutUI, DashboardLayout>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, DashboardLayout target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
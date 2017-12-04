using CatalogueLibrary.Data;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsLoadPeriodically : RDMPCommandExecutionProposal<LoadPeriodically>
    {
        public ProposeExecutionWhenTargetIsLoadPeriodically(IActivateItems itemActivator)
            : base(itemActivator)
        {
        }

        public override bool CanActivate(LoadPeriodically target)
        {
            return true;
        }

        public override void Activate(LoadPeriodically target)
        {
            ItemActivator.Activate<LoadPeriodicallyUI, LoadPeriodically>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, LoadPeriodically target, InsertOption insertOption = InsertOption.Default)
        {
            //nothing can be dropped on Load Metadatas
            return null;
        }
    }
}
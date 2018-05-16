using CatalogueLibrary.Data;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using RDMPObjectVisualisation.Copying.Commands;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsPermissionWindow : RDMPCommandExecutionProposal<PermissionWindow>
    {
        public ProposeExecutionWhenTargetIsPermissionWindow(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(PermissionWindow target)
        {
            return true;
        }

        public override void Activate(PermissionWindow target)
        {
            ItemActivator.Activate<PermissionWindowUI, PermissionWindow>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, PermissionWindow target, InsertOption insertOption = InsertOption.Default)
        {
            var cacheProgressCommand = cmd as CacheProgressCommand;
            if(cacheProgressCommand != null)
                return new ExecuteCommandSetPermissionWindow(ItemActivator,cacheProgressCommand.CacheProgress).SetTarget(target);

            return null;
        }
    }
}
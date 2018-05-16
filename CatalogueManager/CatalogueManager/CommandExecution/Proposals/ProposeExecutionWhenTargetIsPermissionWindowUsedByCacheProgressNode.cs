using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Nodes.LoadMetadataNodes;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsPermissionWindowUsedByCacheProgressNode : RDMPCommandExecutionProposal<PermissionWindowUsedByCacheProgressNode>
    {
        public ProposeExecutionWhenTargetIsPermissionWindowUsedByCacheProgressNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(PermissionWindowUsedByCacheProgressNode target)
        {
            return true;
        }

        public override void Activate(PermissionWindowUsedByCacheProgressNode target)
        {
            if (target.DirectionIsCacheToPermissionWindow)
                ItemActivator.Activate<PermissionWindowUI, PermissionWindow>(target.PermissionWindow);
            else
                ItemActivator.Activate<CacheProgressUI, CacheProgress>(target.CacheProgress);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, PermissionWindowUsedByCacheProgressNode target,InsertOption insertOption = InsertOption.Default)
        {
            //no drag and drop
            return null;
        }
    }
}
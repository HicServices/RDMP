using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Cache;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsCacheProgress:RDMPCommandExecutionProposal<CacheProgress>
    {
        public ProposeExecutionWhenTargetIsCacheProgress(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(CacheProgress target)
        {
            return true;
        }

        public override void Activate(CacheProgress target)
        {
            ItemActivator.Activate<CacheProgressUI, CacheProgress>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, CacheProgress target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}

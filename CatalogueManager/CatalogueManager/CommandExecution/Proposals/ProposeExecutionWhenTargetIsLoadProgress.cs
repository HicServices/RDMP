using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsLoadProgress:RDMPCommandExecutionProposal<LoadProgress>
    {
        public ProposeExecutionWhenTargetIsLoadProgress(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(LoadProgress target)
        {
            return true;
        }

        public override void Activate(LoadProgress target)
        {
            ItemActivator.Activate<LoadProgressUI, LoadProgress>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, LoadProgress target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}

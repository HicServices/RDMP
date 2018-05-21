using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.DataLoadUIs.LoadMetadataUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsLoadMetadata : RDMPCommandExecutionProposal<LoadMetadata>
    {
        public ProposeExecutionWhenTargetIsLoadMetadata(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(LoadMetadata target)
        {
            return true;
        }

        public override void Activate(LoadMetadata target)
        {
            ItemActivator.Activate<LoadMetadataUI,LoadMetadata>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, LoadMetadata target, InsertOption insertOption = InsertOption.Default)
        {
            //nothing can be dropped on Load Metadatas
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsExtractionInformation:RDMPCommandExecutionProposal<ExtractionInformation>
    {
        public ProposeExecutionWhenTargetIsExtractionInformation(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ExtractionInformation target)
        {
            return true;
        }

        public override void Activate(ExtractionInformation target)
        {
            ItemActivator.Activate<ExtractionInformationUI, ExtractionInformation>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractionInformation target, InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}

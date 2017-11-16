using CatalogueLibrary.Data;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    public class ProposeExecutionWhenTargetIsExtractionFilterParameterSet:RDMPCommandExecutionProposal<ExtractionFilterParameterSet>
    {
        public ProposeExecutionWhenTargetIsExtractionFilterParameterSet(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(ExtractionFilterParameterSet target)
        {
            return true;
        }

        public override void Activate(ExtractionFilterParameterSet target)
        {
            ItemActivator.Activate<ExtractionFilterParameterSetUI, ExtractionFilterParameterSet>(target);
        }

        public override ICommandExecution ProposeExecution(ICommand cmd, ExtractionFilterParameterSet target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
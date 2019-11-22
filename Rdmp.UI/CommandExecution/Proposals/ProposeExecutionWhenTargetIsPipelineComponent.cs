using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs;
using Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

namespace Rdmp.UI.CommandExecution.Proposals
{
    class ProposeExecutionWhenTargetIsPipelineComponent : RDMPCommandExecutionProposal<PipelineComponent>
    {
        public ProposeExecutionWhenTargetIsPipelineComponent(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(PipelineComponent target)
        {
            return true;
        }

        public override void Activate(PipelineComponent target)
        {
            var ui = new ArgumentCollectionUI();
            ui.Setup(target,target.GetClassAsSystemType(),ItemActivator.RepositoryLocator.CatalogueRepository);
            ItemActivator.ShowWindow(ui,true);
        }

        public override ICommandExecution ProposeExecution(ICombineToMakeCommand cmd, PipelineComponent target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}
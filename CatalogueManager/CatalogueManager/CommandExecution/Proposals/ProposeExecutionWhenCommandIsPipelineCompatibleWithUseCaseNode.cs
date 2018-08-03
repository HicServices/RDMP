using System;
using System.Data;
using System.Linq;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Nodes.PipelineNodes;
using CatalogueManager.ItemActivation;
using DataExportLibrary.DataRelease.ReleasePipeline;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents;
using ReusableUIComponents.CommandExecution;

namespace CatalogueManager.CommandExecution.Proposals
{
    class ProposeExecutionWhenCommandIsPipelineCompatibleWithUseCaseNode : RDMPCommandExecutionProposal<PipelineCompatibleWithUseCaseNode>
    {
        public ProposeExecutionWhenCommandIsPipelineCompatibleWithUseCaseNode(IActivateItems itemActivator) : base(itemActivator)
        {
        }

        public override bool CanActivate(PipelineCompatibleWithUseCaseNode target)
        {
            return true;
        }

        public override void Activate(PipelineCompatibleWithUseCaseNode target)
        {
            var context = target.UseCase.GetContext();
            var flowType = context.GetFlowType();

            if(flowType == typeof(DataTable))
                Activate<DataTable>(target);
            else if (flowType == typeof (ReleaseAudit))
                Activate<ReleaseAudit>(target);
            else
                throw new Exception("Could not understand flow type:" + flowType.Name);
        }

        private void Activate<T>(PipelineCompatibleWithUseCaseNode target)
        {
            //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
            var dialog = new ConfigurePipelineUI<T>(target.Pipeline, (IDataFlowSource<T>)target.UseCase.ExplicitSource, (IDataFlowDestination<T>)target.UseCase.ExplicitDestination, (DataFlowPipelineContext<T>)target.UseCase.GetContext(), target.UseCase.GetInitializationObjects().ToList(), ItemActivator.RepositoryLocator.CatalogueRepository);
            dialog.ShowDialog();
        }
        public override ICommandExecution ProposeExecution(ICommand cmd, PipelineCompatibleWithUseCaseNode target,
            InsertOption insertOption = InsertOption.Default)
        {
            return null;
        }
    }
}

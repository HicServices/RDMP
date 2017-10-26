using System.Drawing;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;
using ReusableUIComponents.Icons.IconProvision;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    internal class ExecuteCommandSetPipeline : BasicCommandExecution,IAtomicCommand
    {
        private readonly IActivateItems _activator;
        private readonly PipelineUser _user;
        private readonly Pipeline _pipeline;

        public ExecuteCommandSetPipeline(IActivateItems activator, PipelineUser user, Pipeline pipeline)
        {
            _activator = activator;
            _user = user;
            _pipeline = pipeline;
        }

        public override string GetCommandName()
        {
            return _pipeline.Name;
        }

        public override void Execute()
        {
            base.Execute();
            
            _user.Setter(_pipeline);
            _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_user.User));
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline);
        }
    }
}
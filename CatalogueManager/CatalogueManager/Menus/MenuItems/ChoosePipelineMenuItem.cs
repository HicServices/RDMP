using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.ItemActivation;

namespace CatalogueManager.Menus.MenuItems
{
    internal class ChoosePipelineMenuItem : RDMPToolStripMenuItem
    {
        private readonly PipelineUser _user;
        private readonly IPipelineUseCase _useCase;

        public ChoosePipelineMenuItem(IActivateItems activator,PipelineUser user,IPipelineUseCase useCase, string label):base(activator,label)
        {
            _activator = activator;
            _user = user;
            _useCase = useCase;

            AddCompatiblePipelines();

            DropDownItems.Add(new ToolStripSeparator());

            Add(new ExecuteCommandEditPipeline(activator,user,useCase));

            Add(new ExecuteCommandCreateNewPipeline(activator, user,useCase));
        }


        private void AddCompatiblePipelines()
        {
            var allPipelines = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>();
            
            foreach (var compatible in _useCase.FilterCompatiblePipelines(allPipelines))
            {

                var cmd = new ExecuteCommandSetPipeline(_activator, _user, compatible);

                if(!cmd.IsImpossible)
                    Add(cmd);
                else
                {
                    var mi = new ToolStripMenuItem(cmd.GetCommandName(),cmd.GetImage(_activator.CoreIconProvider));
                    mi.Checked = true;
                    DropDown.Items.Add(mi);
                }

                
            }
        }
    }
}
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
    internal class ChoosePipelineMenuItem : ToolStripMenuItem
    {
        private readonly IActivateItems _activator;
        private readonly PipelineUser _user;
        private readonly IPipelineUseCase _useCase;
        private AtomicCommandUIFactory _atomicCommandUIFactory;

        public ChoosePipelineMenuItem(IActivateItems activator,PipelineUser user,IPipelineUseCase useCase, string label)
        {
            _activator = activator;
            _user = user;
            _useCase = useCase;

            _atomicCommandUIFactory = new AtomicCommandUIFactory(activator.CoreIconProvider);

            AddCompatiblePipelines();

            DropDownItems.Add(new ToolStripSeparator());

            DropDown.Items.Add(_atomicCommandUIFactory.CreateMenuItem(new ExecuteCommandEditPipeline(activator,user,useCase)));

            DropDown.Items.Add(_atomicCommandUIFactory.CreateMenuItem(new ExecuteCommandCreateNewPipeline(activator, user)));
            
            Text = label;
        }


        private void AddCompatiblePipelines()
        {
            var allPipelines = _activator.RepositoryLocator.CatalogueRepository.GetAllObjects<Pipeline>();
            
            foreach (var compatible in _useCase.FilterCompatiblePipelines(allPipelines))
            {

                var cmd = new ExecuteCommandSetPipeline(_activator, _user, compatible);

                if(!cmd.IsImpossible)
                    DropDown.Items.Add(_atomicCommandUIFactory.CreateMenuItem(cmd));
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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using CachingEngine.Factories;
using CachingEngine.Requests;
using CachingEngine.Requests.FetchRequestProvider;
using CatalogueLibrary;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.Pipelines;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.CommandExecution.AtomicCommands;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandEditPipeline : BasicUICommandExecution, IAtomicCommand
    {
        private readonly PipelineUser _user;
        private readonly IPipelineUseCase _useCase;
        private IPipeline _pipeline;

        public ExecuteCommandEditPipeline(IActivateItems activator, PipelineUser user,IPipelineUseCase useCase) : base(activator)
        {
            _user = user;
            _useCase = useCase;
            
            _pipeline = _user.Getter();

            if (_pipeline == null)
            {
                SetImpossible(_user.User + " does not currently have a Pipeline");
                return;
            }

            
        }

        public override string GetCommandName()
        {
            if (_pipeline == null)
                return "Edit Selected Pipeline";//it will be impossible anyway

            return "Edit '" + _pipeline + "'";
        }

        public override void Execute()
        {
            base.Execute();

            var cataRepo = Activator.RepositoryLocator.CatalogueRepository;
            var mef = cataRepo.MEF;

            var context = _useCase.GetContext();

            var initObjects = _useCase.GetInitializationObjects();

            
            var uiFactory = new ConfigurePipelineUIFactory(mef, cataRepo);
            var pipelineForm = uiFactory.Create(context.GetType().GenericTypeArguments[0].FullName,
                _pipeline, null, null, context,initObjects.ToList());
            
            pipelineForm.ShowDialog();
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Pipeline,OverlayKind.Edit);
        }
    }


}
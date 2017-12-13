using System;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Reflection;

namespace RDMPObjectVisualisation.Pipelines.PluginPipelineUsers
{
    public class PipelineSelectionUIFactory
    {
        private readonly CatalogueRepository _repository;
        private readonly IPipelineUser _user;
        private readonly IPipelineUseCase _useCase;

        private IPipelineSelectionUI _pipelineSelectionUIInstance;

        public PipelineSelectionUIFactory(CatalogueRepository repository, IPipelineUser user, IPipelineUseCase useCase)
        {
            _repository = repository;
            _user = user;
            _useCase = useCase;
        }

        public PipelineSelectionUIFactory(CatalogueRepository repository, RequiredPropertyInfo requirement, Argument argument, object demanderInstance)
        {
            _repository = repository;

            var pluginUserAndCase = new PluginPipelineUser(requirement, argument, demanderInstance);
            _user = pluginUserAndCase;
            _useCase = pluginUserAndCase;
        }

        public IPipelineSelectionUI Create(string text = null, DockStyle dock = DockStyle.None, Control containerControl = null)
        {
            var context = _useCase.GetContext();

            //setup getter as an event handler for the selection ui
            

            var pipelineSelectionUIType = typeof(PipelineSelectionUI<>).MakeGenericType(context.GetFlowType());
            var uiConstructor = pipelineSelectionUIType.GetConstructors().Single();

            var initObjects = _useCase.GetInitializationObjects().ToList();

            _pipelineSelectionUIInstance = (IPipelineSelectionUI) uiConstructor.Invoke(new object[] { _useCase.ExplicitSource, _useCase.ExplicitDestination, _repository });
            _pipelineSelectionUIInstance.SetContext(context);
            _pipelineSelectionUIInstance.InitializationObjectsForPreviewPipeline =  initObjects;

            if (_user != null)
            {
                _pipelineSelectionUIInstance.Pipeline = _user.Getter();

                _pipelineSelectionUIInstance.PipelineChanged += 
                    (sender, args) =>
                        _user.Setter(((IPipelineSelectionUI)sender).Pipeline as Pipeline);
            }

            var c = (Control)_pipelineSelectionUIInstance;

            if (dock == DockStyle.None)
                c.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            else
                c.Dock = dock;

            if (text != null)
                c.Text = text;

            if (containerControl != null)
                containerControl.Controls.Add(c);

            return _pipelineSelectionUIInstance;
        }

    }
}
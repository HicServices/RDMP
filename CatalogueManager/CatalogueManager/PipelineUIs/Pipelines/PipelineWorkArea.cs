using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueManager.PipelineUIs.DemandsInitializationUIs;
using CatalogueManager.PipelineUIs.Pipelines.Models;
using ReusableUIComponents;
using ReusableUIComponents.Dialogs;

namespace CatalogueManager.PipelineUIs.Pipelines
{
    /// <summary>
    /// Main component control of ConfigurePipelineUI (See ConfigurePipelineUI for details).  Shows you all compatible components on the left including any plugin components.  Components in 
    /// red are not compatible with the current context for example a DelimitedFlatFileDataFlowSource requires a FlatFileToLoad and is therefore incompatible under any context where that object is
    /// not available.
    /// </summary>
    public partial class PipelineWorkArea : UserControl
    {
        private PipelineDiagram _pipelineDiagram;
        private ArgumentCollection _arumentsCollection1;
        private IPipeline _pipeline;
        private readonly IPipelineUseCase _useCase;
        private readonly ICatalogueRepository _catalogueRepository;


        public PipelineWorkArea(IPipeline pipeline, IPipelineUseCase useCase, ICatalogueRepository catalogueRepository)
        {
            _pipeline = pipeline;
            _useCase = useCase;
            _catalogueRepository = catalogueRepository;

            InitializeComponent();
            
            olvComponents.BuildGroups(olvRole, SortOrder.Ascending);
            olvComponents.AlwaysGroupByColumn = olvRole;

            _pipelineDiagram = new PipelineDiagram();
            _pipelineDiagram.AllowSelection = true;
            _pipelineDiagram.AllowReOrdering = true;
            _pipelineDiagram.SelectedComponentChanged += _pipelineDiagram_SelectedComponentChanged;
            _pipelineDiagram.Dock = DockStyle.Fill;
            diagramPanel.Controls.Add(_pipelineDiagram);

            _arumentsCollection1 = new ArgumentCollection();
            _arumentsCollection1.Dock = DockStyle.Fill;
            gbArguments.Controls.Add(_arumentsCollection1);

            olvComponents.RowFormatter+= RowFormatter;
            var context = _useCase.GetContext();

            try
            {
                //middle and destination components
                var allComponentTypes = _catalogueRepository.MEF.GetGenericTypes(typeof (IDataFlowComponent<>),context.GetFlowType());
                
                //source components (list of all types with MEF exports of [Export(typeof(IDataFlowSource<DataTable>))])
                var allSourceTypes = _catalogueRepository.MEF.GetGenericTypes(typeof(IDataFlowSource<>), context.GetFlowType());

                olvComponents.AddObjects(allComponentTypes.Select(t => new AdvertisedPipelineComponentTypeUnderContext(t, _useCase)).ToArray());
                olvComponents.AddObjects(allSourceTypes.Select(t => new AdvertisedPipelineComponentTypeUnderContext(t, useCase)).ToArray());
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Failed to get list of supported MEF components that could be added to the pipeline ", exception);
            }

            gbArguments.Enabled = false;
        }

        void _pipelineDiagram_SelectedComponentChanged(object sender, IPipelineComponent selected)
        {
            gbArguments.Enabled = true;

            if (selected == null)
                _arumentsCollection1.Setup(null, null,_catalogueRepository);
            else
                _arumentsCollection1.Setup(selected, selected.GetClassAsSystemType(), _catalogueRepository);
        }

        private void RowFormatter(OLVListItem olvItem)
        {
            var advertised = (AdvertisedPipelineComponentTypeUnderContext)olvItem.RowObject;

            if (!advertised.IsCompatible())
                olvItem.ForeColor = Color.Red;
        }

        public void SetTo(IPipeline pipeline,IPipelineUseCase useCase)
        {
            _pipelineDiagram.SetTo(pipeline,useCase);
        }

        
        private void olvComponents_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var model = (AdvertisedPipelineComponentTypeUnderContext)e.Model;
            
            ContextMenuStrip RightClickMenu = new ContextMenuStrip();
            
            if (model != null)
            {
                if(!model.IsCompatible())
                    RightClickMenu.Items.Add("Component incompatible",null, (s,v) => WideMessageBox.Show(model.ToString(),model.GetReasonIncompatible(),WideMessageBoxTheme.Help));
            }

            //show it
            if(RightClickMenu.Items.Count != 0)
                RightClickMenu.Show(this, e.Location);

        }

        public void SetPreview(object preview)
        {
            var dt = preview as DataTable;

            if (dt != null)
                _pipelineDiagram.Preview = dt;

            if (preview == null)
                return;

            var table = preview as DataTable;

            if (table != null)
                _arumentsCollection1.Preview = table;
            else
                throw new NotSupportedException("Only DataTables are currently supported by IArgumentCollectionUI previews");
        
        }

        private void btnReRunChecks_Click(object sender, EventArgs e)
        {
            _pipelineDiagram.RefreshUIFromDatabase();
        }
    }
}

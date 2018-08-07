using System;
using System.Collections.Generic;
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

namespace CatalogueManager.PipelineUIs.Pipelines
{
    /// <summary>
    /// Main component control of ConfigurePipelineUI (See ConfigurePipelineUI for details).  Shows you all compatible components on the left including any plugin components.  Components in 
    /// red are not compatible with the current context for example a DelimitedDataFlowSource requires a FlatFileToLoad and is therefore incompatible under any context where that object is
    /// not available.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PipelineWorkArea<T> : UserControl
    {
        private PipelineDiagram<T> _pipelineDiagram;
        private ArgumentCollection _arumentsCollection1;
        private IPipeline _pipeline;
        private readonly DataFlowPipelineContext<T> _context;
        private readonly CatalogueRepository _catalogueRepository;


        public PipelineWorkArea(IPipeline pipeline, DataFlowPipelineContext<T> context, CatalogueRepository catalogueRepository, object[] initializationObjects)
        {
            if (initializationObjects.Any(o=>o == null))
                throw new ArgumentException("initializationObjects array cannot contain null elements", "initializationObjects");

            _pipeline = pipeline;
            _context = context;
            _catalogueRepository = catalogueRepository;

            InitializeComponent();
            
            olvComponents.BuildGroups(olvRole, SortOrder.Ascending);
            olvComponents.AlwaysGroupByColumn = olvRole;

            _pipelineDiagram = new PipelineDiagram<T>();
            _pipelineDiagram.AllowSelection = true;
            _pipelineDiagram.AllowReOrdering = true;
            _pipelineDiagram.SelectedComponentChanged += _pipelineDiagram_SelectedComponentChanged;
            _pipelineDiagram.Dock = DockStyle.Fill;
            diagramPanel.Controls.Add(_pipelineDiagram);

            _arumentsCollection1 = new ArgumentCollection();
            _arumentsCollection1.Dock = DockStyle.Fill;
            gbArguments.Controls.Add(_arumentsCollection1);

            olvComponents.RowFormatter+= RowFormatter;
            
            try
            {
                var allTypes = _catalogueRepository.MEF.GetTypes<IDataFlowComponent<T>>();

                //destinations are not allowed under this context (i.e. NONE of them will be compatible) so don't even bother showing them
                if (context.CannotHave.Contains(typeof (IDataFlowDestination<T>)))
                    allTypes = allTypes.Where(t => !typeof (IDataFlowDestination<T>).IsAssignableFrom(t));//change allTypes to those that are not destinations

                olvComponents.AddObjects(allTypes.Select(t => new AdvertisedPipelineComponentTypeUnderContext<T>(t, context, initializationObjects)).ToArray());
                olvComponents.AddObjects(_catalogueRepository.MEF.GetTypes<IDataFlowSource<T>>().Select(t => new AdvertisedPipelineComponentTypeUnderContext<T>(t, context, initializationObjects)).ToArray());
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
                _arumentsCollection1.Setup(_catalogueRepository, null, null);
            else
                _arumentsCollection1.Setup(_catalogueRepository, selected, selected.GetClassAsSystemType());
        }

        private void RowFormatter(OLVListItem olvItem)
        {
            var advertised = (AdvertisedPipelineComponentTypeUnderContext<T>)olvItem.RowObject;

            if (!advertised.IsCompatible())
                olvItem.ForeColor = Color.Red;
        }

        public void SetTo(DataFlowPipelineEngineFactory<T> pipelineFactory, IPipeline pipeline, object[] initializationObjects)
        {
            _pipelineDiagram.SetTo(pipelineFactory,pipeline,initializationObjects);
        }

        
        private void olvComponents_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var model = (AdvertisedPipelineComponentTypeUnderContext<T>)e.Model;
            
            ContextMenuStrip RightClickMenu = new ContextMenuStrip();
            
            if (model != null)
            {
                if(!model.IsCompatible())
                    RightClickMenu.Items.Add("Explain Why Component Incompatible",null, (s,v) => WideMessageBox.Show(model.GetReasonIncompatible()));
            }

            //show it
            if(RightClickMenu.Items.Count != 0)
                RightClickMenu.Show(this, e.Location);

        }

        public void SetPreview(T preview)
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

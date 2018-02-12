using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.Repositories;
using CatalogueManager.SimpleDialogs.SimpleFileImporting;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.LoadExecution;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService;
using RDMPAutomationService.Pipeline;
using RDMPObjectVisualisation.Pipelines;

namespace CatalogueManager.SimpleDialogs.Automation
{
    /// <summary>
    /// Part of AutomationServiceSlotUI, this dialog is where you configure your custom Automation activities if you have one or more Automation Plugins.  If you do not have any 
    /// automation plugins or do not know what those are then you won't need this user interface!
    /// 
    /// TECHNICAL: Automation plugins are custom plugins that generate async Tasks which are executed on the automation server.  For example you could write a plugin which ran.  Check on
    /// all Catalogues (something which might take quite some time).  After programming the plugin source and uploading the plugin (See PluginManagementForm) you can use this control to create
    /// a new Pipeline which uses the plugin source.
    /// 
    /// Plugin sources (IAutomationSource) GetChunk method is polled so make sure you only package up an automation job if you have detected appropriate conditions (e.g. one isn't executing 
    /// already at the destination) otherwise you will flood the pipeline with new Async Tasks.
    /// </summary>
    public partial class AutomateablePipelineCollectionUI : RDMPUserControl
    {
        private AutomationServiceSlot _automationServiceSlot;
        private PipelineDiagram<OnGoingAutomationTask> _diagram;

        public AutomationServiceSlot AutomationServiceSlot
        {
            get { return _automationServiceSlot; }
            set
            {
                _automationServiceSlot = value;
                RefreshUIFromDatabase();
            }
        }

        private void RefreshUIFromDatabase()
        {
            lbAutomationPipelines.Items.Clear();

            if (AutomationServiceSlot != null)
            {
                lbAutomationPipelines.Items.AddRange(AutomationServiceSlot.AutomateablePipelines);
                btnCreateNew.Enabled = true;
            }
            else
            {

                btnCreateNew.Enabled = false;
            }
        }

        public AutomateablePipelineCollectionUI()
        {
            InitializeComponent();

            _diagram = new PipelineDiagram<OnGoingAutomationTask>();
            _diagram.Dock = DockStyle.Fill;
            panel1.Controls.Add(_diagram);
        }

        private void lbAutomationPipelines_SelectedIndexChanged(object sender, EventArgs e)
        {
            var automateablePipeline =  lbAutomationPipelines.SelectedItem as AutomateablePipeline;
            
            if(automateablePipeline == null)
            {
                btnEditPipeline.Enabled = false;
                _diagram.Clear();
                return;
            }
            else
                btnEditPipeline.Enabled = true;

            var factory =
                new DataFlowPipelineEngineFactory<OnGoingAutomationTask>(RepositoryLocator.CatalogueRepository.MEF,
                    AutomationPipelineContext.Context);

            factory.ExplicitDestination = new AutomationDestination();
            
            _diagram.SetTo(factory,automateablePipeline.Pipeline,new object[]{AutomationServiceSlot,RepositoryLocator});
        }

        private void btnEditPipeline_Click(object sender, EventArgs e)
        {
            var automateablePipeline =  (AutomateablePipeline)lbAutomationPipelines.SelectedItem;
            var uiFactory = new ConfigurePipelineUIFactory(RepositoryLocator.CatalogueRepository.MEF,RepositoryLocator.CatalogueRepository);

            var form = uiFactory.Create(typeof (OnGoingAutomationTask).FullName, automateablePipeline.Pipeline, null,
                new AutomationDestination(), AutomationPipelineContext.Context,
                new List<object>(new object[] {AutomationServiceSlot,RepositoryLocator}));

            form.ShowDialog();
        }

        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            new AutomateablePipeline(RepositoryLocator.CatalogueRepository, AutomationServiceSlot);
            RefreshUIFromDatabase();
        }

        private void lbAutomationPipelines_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var deletable = lbAutomationPipelines.SelectedItem as AutomateablePipeline;

                if(deletable != null && MessageBox.Show("Confirm Deleting Pipeline " + deletable + "?","Confirm Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    var pipe = deletable.Pipeline;
                    deletable.DeleteInDatabase(); //Delete the automation permission
                    pipe.DeleteInDatabase();//delete the actual pipeline too

                    RefreshUIFromDatabase();

                    btnEditPipeline.Enabled = false;
                    _diagram.Clear();
                }
            }
        }
    }
}

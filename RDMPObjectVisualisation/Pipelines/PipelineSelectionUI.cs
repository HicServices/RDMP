using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Annotations;

namespace RDMPObjectVisualisation.Pipelines
{
    /// <summary>
    /// Allows you to pick an IPipeline (or create a new one) to achieve a data flow task (e.g. load a file as a new dataset or attach custom data to a cohort etc).  See ConfigureAndExecutePipeline
    /// for a description of what a pipeline is.  
    /// 
    /// If you cannot see the pipeline you expected to see then it is possible that the pipeline is broken or somehow otherwise incompatible with the current context.  If this is the case
    /// then you can untick 'Only Show Compatible Pipelines' which will show all Pipelines of the type T (usually DataTable).  You should only use this feature to edit Pipelines as there is zero
    /// chance they will execute Successfully if they are not compatible with the DataFlowPipelineContext.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class PipelineSelectionUI<T> : UserControl, IPipelineSelectionUI
    {
        private readonly IDataFlowSource<T> _sourceIfExists;
        private readonly IDataFlowDestination<T> _destinationIfExists;
        private readonly CatalogueRepository _repository;
        private DataFlowPipelineContext<T> _context;
        private IPipeline _pipeline;
        public event Action PipelineDeleted = delegate { };
        
        public event EventHandler PipelineChanged;
        public event EventHandler OnBeforeLaunchEdit;

        public IPipeline Pipeline
        {
            get { return _pipeline; }
            set
            {
                _pipeline = value;

                if (ddPipelines != null)
                    ddPipelines.SelectedItem = value;
            }
        }
        
        public DataFlowPipelineContext<T> Context
        {
            get { return _context; }
            set
            {
                _context = value;
                RefreshPipelineList();
            }
        }

        public void SetContext(IDataFlowPipelineContext context)
        {
            Context = (DataFlowPipelineContext<T>)context;
        }

        public override string Text
        {
            get { return gbPrompt.Text; }
            set { gbPrompt.Text = value; }
        }

        private void RefreshPipelineList()
        {

            var before = ddPipelines.SelectedItem as Pipeline;

            ddPipelines.Items.Clear();
            
            //add pipelines
            var allPipelines = _repository.GetAllObjects<Pipeline>();
            ddPipelines.Items.AddRange(_context == null || cbOnlyShowCompatiblePipelines.Checked == false 
                ? allPipelines.ToArray() //no context/show incompatible enabled so add all pipelines
                : allPipelines.Where(_context.IsAllowable).ToArray()); //only compatible components

            ddPipelines.Items.Add("<<None>>");

            //reselect if it is still there
            if (before != null)
            {
                var toReselect = ddPipelines.Items.OfType<Pipeline>().SingleOrDefault(p => p.ID == before.ID);

                //if we can reselect the users previously selected one
                if (toReselect != null)
                {
                    ddPipelines.SelectedItem = toReselect;
                    return;
                }
            }

            //if there is only one pipeline select it
            ddPipelines.SelectedItem = ddPipelines.Items.OfType<Pipeline>().Count() == 1
                ? (object) ddPipelines.Items.OfType<Pipeline>().Single()
                : "<<None>>";
        }

        public List<object> InitializationObjectsForPreviewPipeline { get; set; }

        
        //IMPORTANT:Do not change this method signature, it is used in reflection (See ArgumentUI.cs for one)
        public PipelineSelectionUI(IDataFlowSource<T> sourceIfExists, IDataFlowDestination<T> destinationIfExists, CatalogueRepository repository)
        {
            _sourceIfExists = sourceIfExists;
            _destinationIfExists = destinationIfExists;
            _repository = repository;
            InitializeComponent();

            //objects that will satisfy (some but not necessarily all) of the IPipelineRequirements of the pipeline source such that it can generate previews of the data for use in configuring the load pipeline (e.g. to figure out dynamic column names)
            InitializationObjectsForPreviewPipeline = new List<object>();

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) //dont connect to database in design mode
                return;

            RefreshPipelineList();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void ddPipelines_SelectedIndexChanged(object sender, EventArgs e)
        {
            Pipeline = ddPipelines.SelectedItem as Pipeline;

            if (Pipeline == null)
                tbDescription.Text = "";
            else
                tbDescription.Text = Pipeline.Description;

            btnEditPipeline.Enabled = Pipeline != null;
            btnDeletePipeline.Enabled = Pipeline != null;
            btnClonePipeline.Enabled = Pipeline != null;

            if(PipelineChanged != null)
                PipelineChanged(this,new EventArgs());
        }

        private void btnEditPipeline_Click(object sender, EventArgs e)
        {
            ShowEditPipelineDialog();
        }

        private void btnCreateNewPipeline_Click(object sender, EventArgs e)
        {
            Pipeline = new Pipeline(_repository, "TO DO:Name this pipeline!" + Guid.NewGuid());
            ddPipelines.Items.Add(Pipeline);
            ddPipelines.SelectedItem = Pipeline;

            ShowEditPipelineDialog();
        }

        private void ShowEditPipelineDialog()
        {
            if (OnBeforeLaunchEdit!= null)
                OnBeforeLaunchEdit(this,new EventArgs());

            //create pipeline UI with NO explicit destination/source (both must be configured within the extraction context by the user)
            var dialog = new ConfigurePipelineUI<T>(Pipeline, _sourceIfExists, _destinationIfExists, Context, InitializationObjectsForPreviewPipeline, _repository);
            dialog.ShowDialog();

            ddPipelines.Items.Remove(Pipeline);

            Pipeline = _repository.GetObjectByID<Pipeline>(Pipeline.ID);
            ddPipelines.Items.Add(Pipeline);
            ddPipelines.SelectedItem = Pipeline;
        }

        private void cbOnlyShowCompatiblePipelines_CheckedChanged(object sender, EventArgs e)
        {
            RefreshPipelineList();
        }

        private void btnDeletePipeline_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to delete " + Pipeline.Name + "? ","Confirm deleting pipeline?",MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
            {
                Pipeline.DeleteInDatabase();
                Pipeline = null;
                PipelineDeleted();
                RefreshPipelineList();
            }
        }

        private void btnClonePipeline_Click(object sender, EventArgs e)
        {
            var p = ddPipelines.SelectedItem as Pipeline;

            if (p != null)
            {
                var clone = p.Clone();
                RefreshPipelineList();
                
                //select the clone
                ddPipelines.SelectedItem = clone;
            }
        }

        
        /// <summary>
        /// Turns the control into a single line ui control
        /// </summary>
        [UsedImplicitly]
        public void CollapseToSingleLineMode()
        {
            this.Height = 28;

            this.Controls.Remove(gbPrompt);

            this.Controls.Add(ddPipelines);
            ddPipelines.Location = new Point(2, 2);
            ddPipelines.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            foreach (var button in new Control[] { btnEditPipeline, btnCreateNewPipeline, btnClonePipeline, btnDeletePipeline, cbOnlyShowCompatiblePipelines })
            {
                this.Controls.Add(button);
                button.Location = new Point(2, 2);
                button.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }

            cbOnlyShowCompatiblePipelines.Text = "";
            cbOnlyShowCompatiblePipelines.Left = Width - cbOnlyShowCompatiblePipelines.Width;

            btnDeletePipeline.Left = cbOnlyShowCompatiblePipelines.Left- btnDeletePipeline.Width;
            btnClonePipeline.Left = btnDeletePipeline.Left - btnClonePipeline.Width;
            btnCreateNewPipeline.Left = btnClonePipeline.Left - btnCreateNewPipeline.Width;
            btnEditPipeline.Left = btnCreateNewPipeline.Left - btnEditPipeline.Width;

            ddPipelines.Width = btnEditPipeline.Left - 2;

        }
    }
}

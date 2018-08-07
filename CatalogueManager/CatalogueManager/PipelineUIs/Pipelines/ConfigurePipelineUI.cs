using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableUIComponents;

using ReusableUIComponents.Progress;

namespace CatalogueManager.PipelineUIs.Pipelines
{
    /// <summary>
    /// Allows you to name, describe and configure a Data Flow Pipeline (IPipeline).  This is a flow of objects (usually DataTables) from a Source through 0 or more Components to a Destination.
    /// Depending on the context the source and/or/neither destination may be fixed.  There will also be zero or more initialization objects which components can consume.  For example if you
    /// are trying to build a pipeline to import a FlatFileToLoad into your database then you might use a DelimitedFlatFileDataFlowSource component to read the file (assuming it wasn't fixed 
    /// width or a database file or anything wierd) and a DataTableUploadDestination to put it into the endpoint.  
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class ConfigurePipelineUI<T> : Form 
    {
        private readonly IPipeline _pipeline;
        private readonly IDataFlowSource<T> _source;
        private readonly IDataFlowDestination<T> _destination;
        private readonly DataFlowPipelineContext<T> _context;

        private readonly DataFlowPipelineEngineFactory<T> _factory;
        private readonly List<object> _initializationObjectsForPreviewPipelineSource;
        private readonly CatalogueRepository _repository;

        private const string NO_COMPONENT = "Unknown";
        
        private PipelineWorkArea<T> _workArea;

        public ConfigurePipelineUI(IPipeline pipeline, IDataFlowSource<T> source, IDataFlowDestination<T> destination, DataFlowPipelineContext<T> context, List<object> initializationObjectsForPreviewPipelineSource, CatalogueRepository repository)
        {
            if (initializationObjectsForPreviewPipelineSource.Any(o => o == null))
                throw new ArgumentException("initializationObjectsForPreviewPipelineSource array cannot contain null elements", "initializationObjectsForPreviewPipelineSource");

            _pipeline = pipeline;
            _source = source;
            _destination = destination;
            _context = context;
            _initializationObjectsForPreviewPipelineSource = initializationObjectsForPreviewPipelineSource;
            _repository = repository;
            InitializeComponent();

            _factory = new DataFlowPipelineEngineFactory<T>(repository.MEF, context)
            {
                ExplicitSource = source,
                ExplicitDestination = destination
            };

            _workArea = new PipelineWorkArea<T>(pipeline, context, repository,initializationObjectsForPreviewPipelineSource.ToArray()) {Dock = DockStyle.Fill};
            panel1.Controls.Add(_workArea);

            tbName.Text = pipeline.Name;
            tbDescription.Text = pipeline.Description;

            RefreshUIFromDatabase();
        }

  
        private void RefreshUIFromDatabase()
        {
            _workArea.SetTo(_factory,_pipeline,_initializationObjectsForPreviewPipelineSource.ToArray());
            try
            {
                lblPreviewStatus.Text = "No Preview Availble";
                lblPreviewStatus.ForeColor = Color.Red;

                InitializeSourceWithPreviewObjects();
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Failed to initialize source with preview objects",e);
                lblPreviewStatus.Text = "Preview Generation Failed";
                lblPreviewStatus.ForeColor = Color.Red;
            }
        }



        private void InitializeSourceWithPreviewObjects()
        {
            //fixed source
            if (_source != null)
            {
                _workArea.SetPreview(_source.TryGetPreview());
                lblPreviewStatus.Text = "Preview Generated successfully";
                lblPreviewStatus.ForeColor = Color.Green;
            }
            else
            {
                //custom source
                if (_initializationObjectsForPreviewPipelineSource == null)
                    return;

                if (!_initializationObjectsForPreviewPipelineSource.Any())
                    return;

                //destination is a pipeline component, factory stamp me out an instance
                var factory = new DataFlowPipelineEngineFactory<T>(_repository.MEF, _context);
                var s = factory.CreateSourceIfExists(_pipeline);

                //now use the stamped out instance for preview generation
                InitializeSourceWithPreviewObjects(s);
            }

        }

        private void InitializeSourceWithPreviewObjects(IDataFlowSource<T> s)
        {
            try
            {
                if (s == null)
                    return;

                _context.PreInitialize(new PopupErrorMessagesEventListener(), s, _initializationObjectsForPreviewPipelineSource.ToArray());
                _workArea.SetPreview(s.TryGetPreview());
                lblPreviewStatus.Text = "Preview Generated successfully";
                lblPreviewStatus.ForeColor = Color.Green;
            }
            catch (Exception e)
            {
                return;//could not generate preview
            }
        }

       private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbName.Text))
                tbName.Text = "NoName";

           if (tbName.Text.StartsWith("TO DO:") || tbName.Text.Equals("NoName"))
               tbName.ForeColor = Color.Red;
           else
               tbName.ForeColor = Color.Black;

           _pipeline.Name = tbName.Text;
            _pipeline.SaveToDatabase();
        }
        
        private void btnOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_pipeline.Description) && string.IsNullOrWhiteSpace(tbDescription.Text))
            {
                if (MessageBox.Show("Are you sure you want to delete the current Description entirely?",
                    "Confirm deleting description?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    _pipeline.Description = null;
                else
                {
                    tbDescription.Text = _pipeline.Description;
                }
            }
            else
                _pipeline.Description = tbDescription.Text;

            _pipeline.SaveToDatabase();
        }

        private void btnRetryPreview_Click(object sender, EventArgs e)
        {
            RefreshUIFromDatabase();
        }
    }
}
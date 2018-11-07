using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Events;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;

namespace CatalogueManager.PipelineUIs.Pipelines
{
    /// <summary>
    /// Reusable component shown by the RDMP whenever it wants you to select a pipeline to achieve a task (See 'A brief overview of what a pipeline is' is UserManual.docx).  The task will
    /// be clearly described at the top of the form, this might be 'loading a flat file into the database to create a new cohort' (the actual description will be more verbose and clear 
    /// though).
    /// 
    /// <para>You should read the task description and select an appropriate pipeline (which will appear in the pipeline diagram along with the input objects this window was launched with).  If
    /// you don't have a pipeline yet you can create a new one (See ConfigurePipelineUI).</para>
    /// 
    /// <para>Input objects are the objects that are provided to accomplish the task (for example a file you are trying to load).  You can usually double click input objects to learn more about
    /// them (e.g. open a file, view a cohort etc).  </para>
    /// 
    /// <para>This may seem like a complicated approach to user interface design but it allows for maximum plugin architecture and freedom to build your own business practices into routine tasks
    /// like cohort committing, data extraction etc.</para>
    /// 
    /// </summary>
    public partial class ConfigureAndExecutePipeline : UserControl
    {
        private readonly PipelineUseCase _useCase;
        private PipelineSelectionUI _pipelineSelectionUI;
        private PipelineDiagram pipelineDiagram1;

        /// <summary>
        /// Fired when the user executes the pipeline (this can happen multiple times if it crashes).
        /// </summary>
        public event PipelineEngineEventHandler PipelineExecutionStarted;

        /// <summary>
        /// Fired when the pipeline finishes execution without throwing an exception.
        /// </summary>
        public event PipelineEngineEventHandler PipelineExecutionFinishedsuccessfully;

        private ForkDataLoadEventListener fork = null;

        readonly List<object> _initializationObjects = new List<object>();

       public ConfigureAndExecutePipeline(PipelineUseCase useCase, IActivateItems activator)
        {
           _useCase = useCase;
           
           InitializeComponent();

           //designer mode
           if(useCase == null && activator == null)
               return;

            rdmpObjectsRibbonUI1.SetIconProvider(activator.CoreIconProvider);

            pipelineDiagram1 = new PipelineDiagram();

            pipelineDiagram1.Dock = DockStyle.Fill;
            panel_pipelineDiagram1.Controls.Add(pipelineDiagram1);

            fork = new ForkDataLoadEventListener(progressUI1);

            var context = useCase.GetContext();

            if(context.GetFlowType() != typeof(DataTable))
                throw new NotSupportedException("Only DataTable flow contexts can be used with this class");

            foreach (var o in useCase.GetInitializationObjects())
                AddInitializationObject(o);

            SetPipelineOptions( activator.RepositoryLocator.CatalogueRepository);

           lblTask.Text = "Task:" + useCase.GetType().Name;
        }
        
        private void AddInitializationObject(object o)
        {
            if(o is DatabaseEntity)
                rdmpObjectsRibbonUI1.Add((DatabaseEntity)o);
            else
                rdmpObjectsRibbonUI1.Add(o);

            _initializationObjects.Add(o);
        }

        private bool _pipelineOptionsSet = false;

        
        public DataFlowPipelineEngineFactory PipelineFactory { get; private set; }
        
        private void SetPipelineOptions(CatalogueRepository repository)
        {
            if (_pipelineOptionsSet)
                throw new Exception("CreateDatabase SetPipelineOptions has already been called, it should only be called once per instance lifetime");

            
            _pipelineOptionsSet = true;

            _pipelineSelectionUI = new PipelineSelectionUI(_useCase, repository)
            {
                Dock = DockStyle.Fill
            };
            _pipelineSelectionUI.PipelineChanged += _pipelineSelectionUI_PipelineChanged;
            _pipelineSelectionUI.PipelineDeleted += () => pipelineDiagram1.Clear();
            
            _pipelineSelectionUI.CollapseToSingleLineMode();

            pPipelineSelection.Controls.Add(_pipelineSelectionUI);
            
            //setup factory
            PipelineFactory = new DataFlowPipelineEngineFactory(_useCase, repository.MEF);

            _pipelineOptionsSet = true;

            RefreshDiagram();
        }

        void _pipelineSelectionUI_PipelineChanged(object sender, EventArgs e)
        {
            RefreshDiagram();

            //repaint entire control so that the arrows are rendered correctly
            Invalidate();
            Refresh();
        }

        
        private void ConfigureAndExecutePipeline_Load(object sender, EventArgs e)
        {
            RefreshDiagram();
        }

        private void RefreshDiagram()
        {
            //not ready to refresh diagram yet
            if (!_pipelineOptionsSet)
                return;

            pipelineDiagram1.SetTo(_pipelineSelectionUI.Pipeline, _useCase);
        }


        private CancellationTokenSource _cancel;

        private void btnExecute_Click(object sender, EventArgs e)
        {
            var pipeline = CreateAndInitializePipeline();

            //if it is already executing
            if (btnExecute.Text == "Stop")
            {
                _cancel.Cancel();//set the cancellation token
                return;
            }

            btnExecute.Text = "Stop";

            _cancel = new CancellationTokenSource();

            //clear any old results
            progressUI1.Clear();
                
            if(PipelineExecutionStarted != null)
                PipelineExecutionStarted(this,new PipelineEngineEventArgs(pipeline));

            progressUI1.ShowRunning(true);

            bool success = false;

            //start a new thread
            Task t = new Task(() =>
            {
                try
                {
                    pipeline.ExecutePipeline(new GracefulCancellationToken(_cancel.Token, _cancel.Token));
                    success = true;
                }
                catch (Exception ex)
                {
                    fork.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Pipeline execution failed", ex));
                }
                
            }
                
                

                );

            t.ContinueWith(x => 
            {
                if (success)
                {
                    //if it successfully got here then Thread has run the engine to completion successfully
                    if (PipelineExecutionFinishedsuccessfully != null)
                        PipelineExecutionFinishedsuccessfully(this, new PipelineEngineEventArgs(pipeline));
                }
                
                progressUI1.ShowRunning(false);

                btnExecute.Text = "Execute"; //make it so user can execute again
            }, TaskScheduler.FromCurrentSynchronizationContext());

            t.Start();
            
           
        }

        
        private void btnPreviewSource_Click(object sender, EventArgs e)
        {
            var pipeline = CreateAndInitializePipeline();
            
            if (pipeline != null)
                try
                {
                    DataTableViewer dtv = new DataTableViewer(((IDataFlowSource<DataTable>) pipeline.SourceObject).TryGetPreview(),"Preview");
                    SingleControlForm.ShowDialog(dtv);
                }
                catch (Exception exception)
                {
                    ExceptionViewer.Show("Preview Generation Failed",exception);
                }
        }

        private IDataFlowPipelineEngine CreateAndInitializePipeline()
        {
            progressUI1.Clear();

            IDataFlowPipelineEngine pipeline = null;
            try
            {
                pipeline = PipelineFactory.Create(_pipelineSelectionUI.Pipeline, fork);
            }
            catch (Exception exception)
            {
                fork.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Could not instantiate pipeline", exception));
                return null;
            }


            try
            {
                pipeline.Initialize(_initializationObjects.ToArray());
            }
            catch (Exception exception)
            {
                fork.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Failed to Initialize pipeline", exception));
                return null;
            }

            return pipeline;
        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tpConfigure_Click(object sender, EventArgs e)
        {

        }

        public void CancelIfRunning()
        {
            if(_cancel != null && !_cancel.IsCancellationRequested)
                _cancel.Cancel();
        }

        public void SetAdditionalProgressListener(IDataLoadEventListener listener)
        {
            fork = new ForkDataLoadEventListener(progressUI1,listener);
        }
    }
}

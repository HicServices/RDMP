// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Repositories;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableLibraryCode.Settings;

namespace Rdmp.UI.PipelineUIs.Pipelines
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
    public partial class ConfigureAndExecutePipelineUI : RDMPUserControl, IPipelineRunner
    {
        private readonly IPipelineUseCase _useCase;
        private PipelineSelectionUI _pipelineSelectionUI;
        private PipelineDiagramUI pipelineDiagram1;

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

       public ConfigureAndExecutePipelineUI(IPipelineUseCase useCase, IActivateItems activator)
        {
           _useCase = useCase;
           
           InitializeComponent();

           //designer mode
           if(useCase == null && activator == null)
               return;

            SetItemActivator(activator);
            progressUI1.ApplyTheme(activator.Theme);

            pipelineDiagram1 = new PipelineDiagramUI();

            pipelineDiagram1.Dock = DockStyle.Fill;
            panel_pipelineDiagram1.Controls.Add(pipelineDiagram1);

            fork = new ForkDataLoadEventListener(progressUI1);

            var context = useCase.GetContext();

            if(context.GetFlowType() != typeof(DataTable))
                throw new NotSupportedException("Only DataTable flow contexts can be used with this class");

            foreach (var o in useCase.GetInitializationObjects())
            {
                var de = o as DatabaseEntity;
                if (o is DatabaseEntity)
                    CommonFunctionality.Add(new ExecuteCommandShow(activator, de, 0, true));
                else
                    CommonFunctionality.Add(o.ToString());

                _initializationObjects.Add(o);
            }

            SetPipelineOptions( activator.RepositoryLocator.CatalogueRepository);

            lblTask.Text = "Task: " + UsefulStuff.PascalCaseStringToHumanReadable(useCase.GetType().Name);
        }

        private bool _pipelineOptionsSet = false;

        
        public DataFlowPipelineEngineFactory PipelineFactory { get; private set; }
        
        private void SetPipelineOptions(ICatalogueRepository repository)
        {
            if (_pipelineOptionsSet)
                throw new Exception("CreateDatabase SetPipelineOptions has already been called, it should only be called once per instance lifetime");

            
            _pipelineOptionsSet = true;

            _pipelineSelectionUI = new PipelineSelectionUI(Activator, _useCase, repository)
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
            Exception exception = null;

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
                    exception = ex;
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

                if(UserSettings.ShowPipelineCompletedPopup)
                {
                    if(success)
                        WideMessageBox.Show("Pipeline Completed","Pipeline execution completed",WideMessageBoxTheme.Help);
                    else
                    {
                        var worst = progressUI1.GetWorst();

                        if(UserSettings.ShowPipelineCompletedPopup)
                            ExceptionViewer.Show("Pipeline crashed",exception ?? worst?.Exception);
                    }
                }

            }, TaskScheduler.FromCurrentSynchronizationContext());

            t.Start();
            
           
        }

        
        private void btnPreviewSource_Click(object sender, EventArgs e)
        {
            var pipeline = CreateAndInitializePipeline();
            
            if (pipeline != null)
                try
                {
                    DataTableViewerUI dtv = new DataTableViewerUI(((IDataFlowSource<DataTable>) pipeline.SourceObject).TryGetPreview(),"Preview");
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

        public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, ICheckNotifier checkNotifier, GracefulCancellationToken token)
        {
            Activator.ShowDialog(new SingleControlForm(this));
            return 0;
        }
    }
}

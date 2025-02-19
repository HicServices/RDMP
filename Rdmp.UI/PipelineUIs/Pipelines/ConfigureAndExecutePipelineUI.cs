// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Events;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.PipelineUIs.Pipelines;

/// <summary>
/// Reusable component shown by the RDMP whenever it wants you to select a pipeline to achieve a task (See 'Pipelines' is UserManual.md).  The task will
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

    private ForkDataLoadEventListener fork;

    private readonly List<object> _initializationObjects = new();

    public ConfigureAndExecutePipelineUI(DialogArgs args, IPipelineUseCase useCase, IActivateItems activator)
    {
        _useCase = useCase;
        InitializeComponent();

        taskDescriptionLabel1.SetupFor(args);

        //designer mode
        if (useCase == null && activator == null)
            return;
        Text = args.WindowTitle;

        SetItemActivator(activator);
        progressUI1.ApplyTheme(activator.Theme);

        pipelineDiagram1 = new PipelineDiagramUI(activator)
        {
            Dock = DockStyle.Fill
        };
        panel_pipelineDiagram1.Controls.Add(pipelineDiagram1);

        fork = new ForkDataLoadEventListener(progressUI1);

        var context = useCase.GetContext();

        if (context.GetFlowType() != typeof(DataTable))
            throw new NotSupportedException("Only DataTable flow contexts can be used with this class");

        foreach (var o in useCase.GetInitializationObjects())
        {
            CommonFunctionality.Add(new ExecuteCommandDescribe(activator, o));

            _initializationObjects.Add(o);
        }

        SetPipelineOptions(activator.RepositoryLocator.CatalogueRepository);
    }

    private bool _pipelineOptionsSet;


    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public DataFlowPipelineEngineFactory PipelineFactory { get; private set; }

    private void SetPipelineOptions(ICatalogueRepository repository)
    {
        if (_pipelineOptionsSet)
            throw new Exception(
                "CreateDatabase SetPipelineOptions has already been called, it should only be called once per instance lifetime");


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
        PipelineFactory = new DataFlowPipelineEngineFactory(_useCase);

        _pipelineOptionsSet = true;

        RefreshDiagram();
    }

    private void _pipelineSelectionUI_PipelineChanged(object sender, EventArgs e)
    {
        RefreshDiagram();

        //repaint entire control so that the arrows are rendered correctly
        Invalidate();
        Refresh();
    }

    private void RefreshDiagram()
    {
        //not ready to refresh diagram yet
        if (!_pipelineOptionsSet)
            return;

        if (_pipelineSelectionUI.Pipeline != null)
        {
            btnPreviewSource.Enabled = true;
            btnExecute.Enabled = true;
            progressUI1.Enabled = true;
        }
        else
        {
            btnPreviewSource.Enabled = false;
            btnExecute.Enabled = false;
            progressUI1.Enabled = false;
        }

        pipelineDiagram1.SetTo(_pipelineSelectionUI.Pipeline, _useCase);
    }


    private CancellationTokenSource _cancel;

    private void btnExecute_Click(object sender, EventArgs e)
    {
        var pipeline = CreateAndInitializePipeline();

        //if it is already executing
        if (btnExecute.Text == "Stop")
        {
            _cancel.Cancel(); //set the cancellation token
            return;
        }

        btnExecute.Text = "Stop";

        _cancel = new CancellationTokenSource();

        //clear any old results
        progressUI1.Clear();

        PipelineExecutionStarted?.Invoke(this, new PipelineEngineEventArgs(pipeline));

        progressUI1.ShowRunning(true);

        var success = false;
        Exception exception = null;

        //start a new thread
        var t = new Task(() =>
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
                //if it successfully got here then Thread has run the engine to completion successfully
                PipelineExecutionFinishedsuccessfully?.Invoke(this, new PipelineEngineEventArgs(pipeline));

            progressUI1.ShowRunning(false);

            btnExecute.Text = "Execute"; //make it so user can execute again

            if (success)
            {
                if (UserSettings.ShowPipelineCompletedPopup)
                    WideMessageBox.Show("Pipeline Completed", "Pipeline execution completed", WideMessageBoxTheme.Help);
                progressUI1.SetSuccess();
            }
            else
            {
                var worst = progressUI1.GetWorst();
                progressUI1.SetFatal();

                if (UserSettings.ShowPipelineCompletedPopup)
                    ExceptionViewer.Show("Pipeline Failed", exception ?? worst?.Exception);
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
                var dtv = new DataTableViewerUI(((IDataFlowSource<DataTable>)pipeline.SourceObject).TryGetPreview(),
                    "Preview");
                SingleControlForm.ShowDialog(dtv);
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("Preview Generation Failed", exception);
            }
    }

    private IDataFlowPipelineEngine CreateAndInitializePipeline()
    {
        progressUI1.Clear();

        IDataFlowPipelineEngine pipeline = null;
        try
        {
            pipeline = PipelineFactory.Create(_pipelineSelectionUI.Pipeline, fork);
            pipeline.Initialize(_initializationObjects.ToArray());
            return pipeline;
        }
        catch (Exception exception)
        {
            fork.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error,
                    $"Could not {(pipeline == null ? "instantiate" : "initialise")} pipeline", exception));
            return null;
        }
    }

    private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void tpConfigure_Click(object sender, EventArgs e)
    {
    }

    public void CancelIfRunning()
    {
        if (_cancel is { IsCancellationRequested: false })
            _cancel.Cancel();
    }

    public void SetAdditionalProgressListener(IDataLoadEventListener listener)
    {
        fork = new ForkDataLoadEventListener(progressUI1, listener);
    }

    public int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token)
    {
        Activator.ShowDialog(new SingleControlForm(this));
        return 0;
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rdmp.Core.CommandExecution.AtomicCommands.Automation;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Progress;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.TransparentHelpSystem;

namespace Rdmp.UI.SimpleControls;

/// <summary>
/// Enables the launching of one of the core RDMP engines (<see cref="RDMPCommandLineOptions"/>) either as a detatched process or as a hosted process (where the
/// UI will show the checking/executing progress messages).  This class ensures that the behaviour is the same between console run rdmp and the UI applications.
/// </summary>
public partial class CheckAndExecuteUI : RDMPUserControl, IConsultableBeforeClosing
{
    //things you have to set for it to work
    public event EventHandler StateChanged;

    public CommandGetterHandler CommandGetter;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ChecksPassed { get; private set; }

    public bool IsExecuting => _runningTask is { IsCompleted: false };

    /// <summary>
    /// Called every time the execution of the runner completes (does not get called if the runner was detached - running
    /// in a seperate process).
    /// </summary>
    public event EventHandler<ExecutionEventArgs> ExecutionFinished;

    private RunnerFactory _factory;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IRunner CurrentRunner { get; private set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool AllowsYesNoToAll
    {
        get => checksUI1.AllowsYesNoToAll;
        set => checksUI1.AllowsYesNoToAll = value;
    }

    public override void SetItemActivator(IActivateItems activator)
    {
        base.SetItemActivator(activator);

        _factory = new RunnerFactory();

        CommonFunctionality.AddToMenu(new ExecuteCommandGenerateRunCommand(activator, Detatch_CommandGetter));
        CommonFunctionality.AddToMenu(new ExecuteCommandRunDetached(activator, Detatch_CommandGetter));

        loadProgressUI1.ApplyTheme(activator.Theme);
    }

    private RDMPCommandLineOptions Detatch_CommandGetter() => CommandGetter(CommandLineActivity.run);

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<HelpStage> HelpStages { get; private set; }

    //constructor
    public CheckAndExecuteUI()
    {
        InitializeComponent();
        ChecksPassed = false;
        SetButtonStates();
        HelpStages = BuildHelpStages();
    }

    private List<HelpStage> BuildHelpStages()
    {
        var stages = new List<HelpStage>
        {
            new(btnRunChecks,
                "Once you are happy with the selections, use this button to run the checks for the selected options."),
            new(btnExecute, "This button will execute the required operation in the RDMP UI.\r\n" +
                            "Results will be shown below.")
        };

        return stages;
    }

    private GracefulCancellationTokenSource _cancellationTokenSource;
    private Task _runningTask;


    private void btnRunChecks_Click(object sender, EventArgs e)
    {
        IRunner runner;

        try
        {
            var command = CommandGetter(CommandLineActivity.check);
            runner = RunnerFactory.CreateRunner(Activator, command);
        }
        catch (Exception ex)
        {
            ragChecks.Fatal(ex);
            return;
        }

        CurrentRunner = runner;

        btnRunChecks.Enabled = false;

        //reset the visualisations
        ragChecks.Reset();
        checksUI1.Clear();

        //ensure the checks are visible over the load
        loadProgressUI1.Visible = false;
        checksUI1.Visible = true;

        //create a to memory that passes the events to checksui since that's the only one that can respond to proposed fixes
        var toMemory = new ToMemoryCheckNotifier(checksUI1);

        Task.Factory.StartNew(() => Check(runner, toMemory)).ContinueWith(
            t =>
            {
                //once Thread completes do this on the main UI Thread

                //find the worst check state
                var worst = toMemory.GetWorst();
                //update the rag smiley to reflect whether it has passed
                ragChecks.OnCheckPerformed(new CheckEventArgs($"Checks resulted in {worst}", worst));
                //update the bit flag
                ChecksPassed = worst <= CheckResult.Warning;

                //enable other buttons now based on the new state
                SetButtonStates();
            }, TaskScheduler.FromCurrentSynchronizationContext());

        _runningTask = null;
        ChecksPassed = true;
    }

    private void Check(IRunner runner, ToMemoryCheckNotifier toMemory)
    {
        try
        {
            runner.Run(Activator.RepositoryLocator, new FromCheckNotifierToDataLoadEventListener(toMemory), toMemory,
                new GracefulCancellationToken());
        }
        catch (Exception e)
        {
            toMemory.OnCheckPerformed(new CheckEventArgs("Entire process crashed", CheckResult.Fail, e));
        }
    }

    private void btnExecute_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource = new GracefulCancellationTokenSource();

        IRunner runner;

        try
        {
            var command = CommandGetter(CommandLineActivity.run);
            runner = RunnerFactory.CreateRunner(Activator, command);
        }
        catch (Exception ex)
        {
            ragChecks.Fatal(ex);
            return;
        }

        CurrentRunner = runner;

        loadProgressUI1.Clear();
        loadProgressUI1.ShowRunning(true);

        var exitCode = 0;

        _runningTask =
            //run the data load in a Thread
            Task.Factory.StartNew(() => { exitCode = Run(runner); });

        _runningTask
            //then on the main UI thread (after load completes with success/error
            .ContinueWith(t =>
                {
                    //reset the system state because the execution has completed
                    ChecksPassed = false;

                    loadProgressUI1.ShowRunning(false);

                    if (exitCode != 0)
                        loadProgressUI1.SetFatal();
                    else
                        loadProgressUI1.SetSuccess();

                    ExecutionFinished?.Invoke(this, new ExecutionEventArgs(exitCode));

                    //adjust the buttons accordingly
                    SetButtonStates();
                }
                , TaskScheduler.FromCurrentSynchronizationContext());

        SetButtonStates();
    }

    private int Run(IRunner runner)
    {
        try
        {
            var exitCode = runner.Run(Activator.RepositoryLocator, loadProgressUI1,
                new FromDataLoadEventListenerToCheckNotifier(loadProgressUI1), _cancellationTokenSource.Token);

            loadProgressUI1.OnNotify(this, new NotifyEventArgs(
                exitCode == 0 ? ProgressEventType.Information : ProgressEventType.Error,
                $"Exit code was {exitCode}"));

            return exitCode;
        }
        catch (Exception ex)
        {
            loadProgressUI1.OnNotify(ProgressUI.GlobalRunError,
                new NotifyEventArgs(ProgressEventType.Error, "Fatal Error", ex));
        }

        return -1;
    }

    private void SetButtonStates()
    {
        var h = StateChanged;
        h?.Invoke(this, EventArgs.Empty);

        if (!ChecksPassed)
        {
            //tell user he must run checks
            if (ragChecks.IsGreen())
                ragChecks.Warning(new Exception("Checks have not been run yet"));

            btnRunChecks.Enabled = true;

            btnExecute.Enabled = false;
            btnAbortLoad.Enabled = false;
            return;
        }

        if (_runningTask != null)
        {
            checksUI1.Visible = false;
            loadProgressUI1.Visible = true;
        }

        //checks have passed is there a load underway already?
        if (_runningTask == null || _runningTask.IsCompleted)
        {
            //no load underway!

            //leave checks enabled and enable execute
            btnRunChecks.Enabled = true;
            btnExecute.Enabled = true;
        }
        else
        {
            //load is underway!
            btnExecute.Enabled = false;
            btnRunChecks.Enabled = false;

            //only thing we can do is abort
            btnAbortLoad.Enabled = true;
        }
    }

    public void GroupBySender(string filter = null)
    {
        loadProgressUI1.GroupBySender(filter);
    }

    private void btnAbortLoad_Click(object sender, EventArgs e)
    {
        _cancellationTokenSource.Abort();
        loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Abort request issued"));
    }

    public void Reset()
    {
        if (!IsExecuting)
        {
            checksUI1.Clear();
            loadProgressUI1.Clear();
            ChecksPassed = false;
            _runningTask = null;
            SetButtonStates();
        }
    }

    public void ConsultAboutClosing(object sender, FormClosingEventArgs formClosingEventArgs)
    {
        if (IsExecuting)
        {
            MessageBox.Show("Control is still executing, please abort first");
            formClosingEventArgs.Cancel = true;
        }
    }
}
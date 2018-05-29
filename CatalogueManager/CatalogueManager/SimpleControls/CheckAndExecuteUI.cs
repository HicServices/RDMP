using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.ItemActivation;
using HIC.Logging;
using RDMPAutomationService.Options;
using RDMPAutomationService.Runners;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;

namespace CatalogueManager.SimpleControls
{
    /// <summary>
    /// Enables the launching of one of the core RDMP engines (<see cref="RDMPCommandLineOptions"/>) either as a detatched process or as a hosted process (where the 
    /// UI will show the checking/executing progress messages).  This class ensures that the behaviour is the same between console run rdmp and the UI applications.
    /// </summary>
    public partial class CheckAndExecuteUI : UserControl
    {
        //things you have to set for it to work
        public event EventHandler StateChanged;

        public CommandGetterHandler CommandGetter;

        public bool ChecksPassed { get; private set; }
        public bool IsExecuting { get { return _runningTask != null && !_runningTask.IsCompleted; } }

        private RunnerFactory _factory;
        private IActivateItems _activator;

        public IRunner CurrentRunner { get; private set; }

        public void SetItemActivator(IActivateItems activator)
        {
            _factory = new RunnerFactory();
            _activator = activator;
            executeInAutomationServerUI1.SetItemActivator(activator);
            
            executeInAutomationServerUI1.CommandGetter = Detatch_CommandGetter;
        }

        private RDMPCommandLineOptions Detatch_CommandGetter()
        {
            return CommandGetter(CommandLineActivity.run);
        }

        //constructor
        public CheckAndExecuteUI()
        {
            InitializeComponent();
            ChecksPassed = false;
            SetButtonStates();
        }

        private GracefulCancellationTokenSource _cancellationTokenSource;
        private Task _runningTask;
        


        private void btnRunChecks_Click(object sender, EventArgs e)
        {
            
            IRunner runner;

            try
            {
                var command = CommandGetter(CommandLineActivity.check);    
                runner = _factory.CreateRunner(command);
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

            Task.Factory.StartNew(() => runner.Run(_activator.RepositoryLocator, new FromCheckNotifierToDataLoadEventListener(toMemory), toMemory, new GracefulCancellationToken())).ContinueWith(
                t=>
                {
                    //once Thread completes do this on the main UI Thread

                    //find the worst check state
                    var worst = toMemory.GetWorst();
                    //update the rag smiley to reflect whether it has passed
                    ragChecks.OnCheckPerformed(new CheckEventArgs("Checks resulted in " + worst ,worst));
                    //update the bit flag
                    ChecksPassed = worst <= CheckResult.Warning;
                
                    //enable other buttons now based on the new state
                    SetButtonStates();

                }, TaskScheduler.FromCurrentSynchronizationContext());

            _runningTask = null;
            ChecksPassed = true;
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource = new GracefulCancellationTokenSource();

            loadProgressUI1.ShowRunning(true);

            IRunner runner;

            try
            {
                var command = CommandGetter(CommandLineActivity.run);
                runner = _factory.CreateRunner(command);
            }
            catch (Exception ex)
            {
                ragChecks.Fatal(ex);
                return;
            }
            CurrentRunner = runner;

            _runningTask =
                //run the data load in a Thread
                Task.Factory.StartNew(() =>

                {
                    try
                    {
                        runner.Run(_activator.RepositoryLocator, loadProgressUI1, new FromDataLoadEventListenerToCheckNotifier(loadProgressUI1), _cancellationTokenSource.Token);
                    }
                    catch (Exception ex)
                    {
                        loadProgressUI1.OnNotify(this,new NotifyEventArgs(ProgressEventType.Error, "Fatal Error",ex));
                    }
                    
                })
                //then on the main UI thread (after load completes with success/error
                .ContinueWith((t) =>
                {
                    //reset the system state because the execution has completed
                    ChecksPassed = false;

                    loadProgressUI1.ShowRunning(false);
                    //adjust the buttons accordingly
                    SetButtonStates();
                }
                , TaskScheduler.FromCurrentSynchronizationContext());

            SetButtonStates();
        }
        
        private void SetButtonStates()
        {
            if (StateChanged != null)
                StateChanged(this,new EventArgs());

            if (!ChecksPassed)
            {
                //tell user he must run checks
                if (ragChecks.IsGreen())
                    ragChecks.Warning(new Exception("Checks have not been run yet"));

                btnRunChecks.Enabled = true;
                
                btnExecute.Enabled = false;
                executeInAutomationServerUI1.Enabled = false;
                btnAbortLoad.Enabled = false;
                return;
            }

            if (_runningTask != null)
            {
                checksUI1.Visible = false;
                loadProgressUI1.Visible = true;
                loadProgressUI1.Clear();
            }
            
            //checks have passed is there a load underway already?
            if (_runningTask == null || _runningTask.IsCompleted)
            {
                //no load underway!

                //leave checks enabled and enable execute
                btnRunChecks.Enabled = true;
                btnExecute.Enabled = true;
                executeInAutomationServerUI1.Enabled = true;
            }
            else
            {
                //load is underway!
                btnExecute.Enabled = false;
                executeInAutomationServerUI1.Enabled = false;
                btnRunChecks.Enabled = false;

                //only thing we can do is abort
                btnAbortLoad.Enabled = true;
            }
        }

        private void btnAbortLoad_Click(object sender, EventArgs e)
        {
            _cancellationTokenSource.Abort();
            loadProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,"Abort request issued"));
        }
    }
}

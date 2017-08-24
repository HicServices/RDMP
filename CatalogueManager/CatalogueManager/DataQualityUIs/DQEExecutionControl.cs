using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.Migration;
using DataQualityEngine.Reports;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace CatalogueManager.DataQualityUIs
{
    /// <summary>
    /// Form for performing Data Quality Engine executions on a chosen Catalogue. Opening the form will trigger a series of pre run checks and once these have successfully completed you
    /// can then begin the execution by clicking the Start Execution button.
    /// 
    /// While the execution is happening you can view the progress on the right hand side.
    /// 
    /// To view the results of the execution Right Click on the relevant catalogue and select View DQE Results.
    /// </summary>
    public partial class DQEExecutionControl : DQEExecutionControl_Design
    {
        private Catalogue _catalogue;
        public Catalogue Catalogue
        {
            get { return _catalogue; }
            private set
            {
                _catalogue = value;
                ReloadUIFromDatabase();
            }
        }
        
        public DQEExecutionControl()
        {
            InitializeComponent();
            dqePreRunCheckerUI1.AllChecksComplete += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                Invoke(new MethodInvoker(AllChecksComplete));
            };
        }

        private void ReloadUIFromDatabase()
        {
            if (_catalogue == null || RepositoryLocator == null)
                return;

            dqePreRunCheckerUI1.Clear();

            RunChecks();
        }

        private void AllChecksComplete()
        {
            EnableExecution();
        }

        private void EnableExecution()
        {
            if (InvokeRequired)
                btnExecute.Invoke((MethodInvoker)(() => btnExecute.Enabled = true));
            else
                btnExecute.Enabled = true;
        }

        private void RunChecks()
        {
            btnExecute.Enabled = false;
            CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, MigrationColumnSet.DataLoadRunField);
            dqePreRunCheckerUI1.StartChecking(report);
        }

        private void ExecuteDQE()
        {
            Thread t = new Thread(() =>
            {
                try
                {
                    CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, MigrationColumnSet.DataLoadRunField);
                    report.GenerateReport(_catalogue, executionProgressUI1, new CancellationTokenSource().Token);
                }
                catch (Exception exception)
                {
                    executionProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, exception.Message, exception));
                }
            });

            executionProgressUI1.Text = "DQE Evaluation of " + _catalogue;
            t.Start();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            executionProgressUI1.Clear();
            executionProgressUI1.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "DQE execution starting..."));

            ExecuteDQE();
        }

        public override void SetDatabaseObject(IActivateItems activator, Catalogue databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            Catalogue = databaseObject;
        }

        public override string GetTabName()
        {
            return "DQE Execution:" + base.GetTabName();
        }

        private void btnRerunChecks_Click(object sender, EventArgs e)
        {
            if (_catalogue == null)
                return;

            ReloadUIFromDatabase();
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DQEExecutionControl_Design, UserControl>))]
    public abstract class DQEExecutionControl_Design : RDMPSingleDatabaseObjectControl<Catalogue>
    {
    }
}

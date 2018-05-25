using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Triggers;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.Migration;
using DataQualityEngine.Reports;
using RDMPAutomationService.Options;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace CatalogueManager.DataQualityUIs
{
    /// <summary>
    /// Form for performing Data Quality Engine executions on a chosen Catalogue. Opening the form will trigger a series of pre run checks and once these have successfully completed you
    /// can then begin the execution by clicking the Start Execution button.
    /// 
    /// <para>While the execution is happening you can view the progress on the right hand side.</para>
    /// 
    /// <para>To view the results of the execution Right Click on the relevant catalogue and select View DQE Results.</para>
    /// </summary>
    public partial class DQEExecutionControl : DQEExecutionControl_Design
    {
        private Catalogue _catalogue;

        public DQEExecutionControl()
        {
            InitializeComponent();
            dqePreRunCheckerUI1.AllChecksComplete += (s, e) =>
            {
                if (IsDisposed || !IsHandleCreated)
                    return;

                Invoke(new MethodInvoker(AllChecksComplete));
            };

            AssociatedCollection = RDMPCollection.Catalogue;
            executeInAutomationServerUI1.CommandGetter = CommandGetter;
        }

        private RDMPCommandLineOptions CommandGetter()
        {
            return new DqeOptions(){Catalogue = _catalogue.ID,Command = CommandLineActivity.run};
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
            CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue, SpecialFieldNames.DataLoadRunID);
            dqePreRunCheckerUI1.StartChecking(report);
        }

        private void ExecuteDQE()
        {
            Thread t = new Thread(() =>
            {
                executionProgressUI1.ShowRunning(true);

                try
                {
                    CatalogueConstraintReport report = new CatalogueConstraintReport(_catalogue,
                        SpecialFieldNames.DataLoadRunID);
                    report.GenerateReport(_catalogue, executionProgressUI1, new CancellationTokenSource().Token);
                }
                catch (Exception exception)
                {
                    executionProgressUI1.OnNotify(this,
                        new NotifyEventArgs(ProgressEventType.Error, exception.Message, exception));
                }
                finally
                {
                    executionProgressUI1.ShowRunning(false);
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
            _catalogue = databaseObject;
            executeInAutomationServerUI1.SetItemActivator(activator);
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

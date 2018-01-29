using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataLoadEngine.DatabaseManagement;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Job;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using HIC.Logging;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.ProcessTasks
{
    /// <summary>
    /// Lets you view/edit an Executable file load task.  This exe will be run at the appropriate time in the data load (with the arguments displayed in the black box).
    /// </summary>
    public partial class ExeProcessTaskUI : ExeProcessTaskUI_Design
    {
        private ProcessTask _processTask;
        private ExecutableRuntimeTask _runtimeTask;
        private Task _runTask;

        public ExeProcessTaskUI()
        {
            InitializeComponent();

            pbFile.Image = CatalogueIcons.Exe;
        }

        public override void SetDatabaseObject(IActivateItems activator, ProcessTask databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);

            _processTask = databaseObject;

            SetupForFile();

        }

        private void SetupForFile()
        {
            lblPath.Text = _processTask.Path;

            lblPath.Text = _processTask.Path;
            ragSmiley1.Left = lblPath.Right;
            btnBrowse.Left = ragSmiley1.Right;

            lblID.Left = btnBrowse.Right;
            tbID.Left = lblID.Right;


            tbID.Text = _processTask.ID.ToString();

            loadStageIconUI1.Setup(_activator.CoreIconProvider, _processTask.LoadStage);
            loadStageIconUI1.Left = tbID.Right + 2;

            GetExecutionCommand();
        }

        private void GetExecutionCommand()
        {
            btnRunExe.Enabled = false;
            try
            {
                var factory = new RuntimeTaskFactory(_activator.RepositoryLocator.CatalogueRepository);

                var lmd = _processTask.LoadMetadata;
                var argsDictionary = new LoadArgsDictionary(lmd, new HICDatabaseConfiguration(lmd).DeployInfo);
                _runtimeTask = (ExecutableRuntimeTask)factory.Create(_processTask, argsDictionary.LoadArgs[_processTask.LoadStage]);
                tbExeCommand.Text = _runtimeTask.ExeFilepath + " " + _runtimeTask.CreateArgString();
                btnRunExe.Enabled = true;
            }
            catch (Exception e)
            {
                ragSmiley1.Fatal(new Exception("Could not assemble ExecutableRuntimeTask or Arguments for exe", e));
            }
        }

        private void btnRunExe_Click(object sender, EventArgs e)
        {
            if (_runTask != null && !_runTask.IsCompleted)
            {
                MessageBox.Show("Exe is still running");
                return;
            }

            btnRunExe.Enabled = false;
            _runTask = new Task(() =>
            {
                try
                {
                    _runtimeTask.Run(new ThrowImmediatelyDataLoadJob(new FromCheckNotifierToDataLoadEventListener(checksUI1)), new GracefulCancellationToken());
                }
                catch (Exception ex)
                {
                    ExceptionViewer.Show(ex);
                }
                finally
                {
                    Invoke(new MethodInvoker(() => btnRunExe.Enabled = true));
                }
            });
            _runTask.Start();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Executables|*.exe";
            dialog.CheckFileExists = true;
            
            //open the browse dialog at the location of the currently specified file
            if (!string.IsNullOrWhiteSpace(_processTask.Path))
            {
                var fi = new FileInfo(_processTask.Path);
                if (fi.Exists && fi.Directory != null)
                    dialog.InitialDirectory = fi.Directory.FullName;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _processTask.Path = dialog.FileName;
                _processTask.SaveToDatabase();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_processTask));
                SetupForFile();
            }
        }
    }
    
    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExeProcessTaskUI_Design, UserControl>))]
    public abstract class ExeProcessTaskUI_Design:RDMPSingleDatabaseObjectControl<ProcessTask>
    {
    }
}

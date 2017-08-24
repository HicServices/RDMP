using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using DataExportLibrary.Repositories;
using HIC.Logging;
using MapsDirectlyToDatabaseTableUI;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableUIComponents;
using ReusableUIComponents.Progress;

using ReusableUIComponents.SingleControlForms;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Allows you to execute an extraction of a project configuration (Generate anonymous project data extractions for researchers).  You should make sure that you have already selected 
    /// the correct datasets, filters, transforms etc to meet the researchers project requirements (and governance approvals) - See ExtractionConfigurationUI and ConfigureDatasetUI.
    /// 
    /// Start by selecting which datasets you want to execute (this can be an iterative process - you can extract half of them overnight and then come back and extract the other half the 
    /// next night).  See ChooseExtractablesUI for how to select datasets.
    /// 
    /// Next you should select/create a new extraction pipeline (See 'A Brief Overview Of What A Pipeline Is' in UserManual.docx).  This will determine the format of the extracted data
    /// (e.g. .CSV or .MDB database file or any other file for which you have a plugin implemented for).
    /// </summary>
    public partial class ExecuteExtractionUI : ExecuteExtractionUI_Design, IConsultableBeforeClosing
    {
        private PipelineSelectionUI<System.Data.DataTable> _pipelineSelectionUI1;
            
        public Project Project { get; set; }
        public int TopX { get; set; }
        private ExtractionConfiguration _configurationToExecute;
        private readonly IExtractableDataSet[] _toExtract;
        
        private bool extractionInProgress = false;
        DataLoadInfo _dataLoadInfo;

        BiDictionary<TabPage, ExecuteDatasetExtractionHostUI> tabPagesDictionary = new BiDictionary<TabPage, ExecuteDatasetExtractionHostUI>();

        public ExecuteExtractionUI()
        {
            InitializeComponent();
        }


        private bool HasConfigurationPreviouslyBeenReleased(ExtractionConfiguration configurationToExecute)
        {
            var previouslyReleasedStuff = configurationToExecute.ReleaseLogEntries;

            if (previouslyReleasedStuff.Any())
                return true;

            return false;
        }


        private void btnCancelAll_Click(object sender, EventArgs e)
        {
            if (datasetsCurrentlyExecuting == 0)
                return;

            foreach (ExecuteDatasetExtractionHostUI host in tabPagesDictionary.Seconds)
                host.Cancel();

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (extractionInProgress)
            {
                MessageBox.Show("Extraction already in process");
                return;
            }

            if (_pipelineSelectionUI1.Pipeline == null)
            {
                MessageBox.Show("No pipeline selected");
                return;
            }

            //get rid of all old tabs
            tabControl1.TabPages.Clear();
            tabPagesDictionary.Clear();

            IExtractCommand[] commands= null;

            try
            {
                //create an entry in the table for each extraction
                commands = chooseExtractablesUI1.GetFinalExtractCommands();
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show(exception);
                return;
            }

            DisableControls(true);
            
            if (!NukeExcelAndWinword())
                return;

            _dataLoadInfo = StartAudit();

            //could not generate audit object so abandon extraction (and renenable the controls)
            if (_dataLoadInfo == null)
            {
                DisableControls(false);
                return;
            }
            
            try
            {
                //the globals we must extract
                var globals = chooseExtractablesUI1.GetGlobalsBundle();

                //if there are globals selected for extraction (including cohort custom tables btw)
                if (globals.Any())
                {
                    //create UI tab for globals
                    var globalsTab = new TabPage("Globals & Custom Data");
                    tabControl1.TabPages.Add(globalsTab);

                    //with a progressUI on it
                    var progressUI = new ProgressUI();
                    progressUI.Dock = DockStyle.Fill;
                    globalsTab.Controls.Add(progressUI);

                    Thread t = new Thread(() =>
                        ExtractionPipelineHost.ExtractGlobalsForDestination(Project, 
                        _configurationToExecute,
                        _pipelineSelectionUI1.Pipeline,
                        globals,
                        progressUI,
                        _dataLoadInfo));

                    t.Start();
                }
                
            }
            catch (Exception exception)
            {
                ExceptionViewer.Show("A problem ocurred trying to create global extractions."+exception.Message,exception);
                return;
            }

            foreach (IExtractCommand command in commands)
            {
                var datasetCommand = command as ExtractDatasetCommand;
                if(datasetCommand != null)
                {
                    if (TopX != -1)
                        ForceTopX(datasetCommand);

                    datasetCommand.IncludeValidation = !cbSkipValidation.Checked;
                }
                
                var host = new ExecuteDatasetExtractionHostUI(command, Project, _dataLoadInfo, _pipelineSelectionUI1.Pipeline);
                host.RepositoryLocator = RepositoryLocator;
                host.Dock = DockStyle.Fill;

                //add a new tab page for this bundle execution
                var tabPage = new TabPage(command.ToString());
                tabPage.Controls.Add(host);
                
                tabControl1.TabPages.Add(tabPage);
                tabPagesDictionary.Add(tabPage,host);
                  
                datasetsCurrentlyExecuting++;
                host.DoExtraction();
                host.Finished += host_Finished;
            }

            if(!_dataLoadInfo.IsClosed)
                _dataLoadInfo.CloseAndMarkComplete();
            
        }

        private void ForceTopX(ExtractDatasetCommand command)
        {
            ((QueryBuilder)command.QueryBuilder).SetLimitationSQL(" TOP " + TopX);
        }

        private void DisableControls(bool disable)
        {
            extractionInProgress = disable;
            btnStart.Enabled = !disable;
            cbIsTest.Enabled = !disable;
            cbIsTest.Enabled = !disable;
        }

        /// <summary>
        /// prompts user to terminate any Excel and Winword executables
        /// </summary>
        /// <returns>True if there weren't any or they were nuked, False if there were some and the user decided not to carry through on nukage</returns>
        private bool NukeExcelAndWinword()
        {
            Process[] processes = Process.GetProcesses();

            List<Process> toNuke = processes.Where(process => process.ProcessName.Equals("EXCEL") || process.ProcessName.Equals("WINWORD")).ToList();

            if (toNuke.Count > 0)
            {
                if (
                    MessageBox.Show(
                        "Found " + toNuke.Count +
                        " processes called EXCEL or WINWORD running on your computer.  Before extraction can take place these must be closed.  Would you like to Terminate them? (Without saving any changes you might have made)",
                        "TERMINATE All Excel and Word processes (Without saving changes)?",
                        MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                {

                    //user might have closed some whilst the YesNo dialog is onscreen.
                    toNuke.ForEach(p => { if (!p.HasExited) p.Kill(); }); 
                    Thread.Sleep(1000);
                    return true;
                }
                else
                    return false;
            }
            else
                return true;
        }

        private int datasetsCurrentlyExecuting = 0;
        
        void host_Finished()
        {
            if (this.InvokeRequired)
            {
                BeginInvoke(new Action(host_Finished));
                return;
            }

            datasetsCurrentlyExecuting--;

            if (datasetsCurrentlyExecuting == 0)
            {
                extractionInProgress = false;
                btnStart.Enabled = true;
                cbIsTest.Enabled = true;
                cbIsTest.Enabled = true;
                
                if (_dataLoadInfo != null && !_dataLoadInfo.IsClosed)
                    _dataLoadInfo.CloseAndMarkComplete();

                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_configurationToExecute));
            }
        }

        
        
        private DataLoadInfo StartAudit()
        {
            DataLoadInfo dataLoadInfo;

            var logManager = _configurationToExecute.GetExplicitLoggingDatabaseServerOrDefault();
            
            try
            {
                //populate DataLoadInfo object (Audit)
                dataLoadInfo = new DataLoadInfo(ExecuteDatasetExtractionSource.AuditTaskName,
                                                     Process.GetCurrentProcess().ProcessName,
                                                     Project.Name + "(ExtractionConfiguration ID=" +
                                                     _configurationToExecute.ID + ")",
                                                     "", cbIsTest.Checked,logManager.Server);
            }
            catch (Exception e)
            {
                ExceptionViewer.Show("Problem occurred trying to create Logging Component:" + e.Message + " (check user has access to " + logManager.Server+ " and that the DataLoadTask '"+ExecuteDatasetExtractionSource.AuditTaskName+"' exists)", e);
                return null;
            }

            return dataLoadInfo;
        }
        

        private void tbTopX_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTopX.Text))
            {
                TopX = -1;
                return;
            }
            try
            {
                TopX = int.Parse(tbTopX.Text);

                if (TopX < 0)
                {
                    TopX = -1;
                    throw new Exception("Cannot be negative");
                }

                tbTopX.ForeColor = Color.Black;
            }
            catch (Exception)
            {
                tbTopX.ForeColor = Color.Red;
            }
        }


        private void lbDatasets_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag != null)
            {
                ExtractableDataSet ds = e.Item.Tag as ExtractableDataSet;

                if (ds != null && ds.DisableExtraction && e.Item.Checked)
                {
                    MessageBox.Show(
                        "Dataset is disabled for extraction (possibly it is a work in progress or in a state of flux)");
                    e.Item.Checked = false;
                    return;
                }
            }
        }

        void PipelineSelectionUI1OnBeforeLaunchEdit(object sender, EventArgs e)
        {
            var datasets = chooseExtractablesUI1.GetCheckedDatasets();

            if (!datasets.Any())
            {
                MessageBox.Show("There are no datasets in this configuration, you cannot edit the pipeline without at least 1 (for preview generation etc)");
                return;
            }


            ExtractableDataSet selected = null;
            
            //unlikely but possible, if there is only 1 don't fire up a dialogue asking him to pick the only one!
            if (datasets.Length == 1)
                selected = (ExtractableDataSet) datasets[0];
            else
            {
                MessageBox.Show(
                    "Some pipeline components require a preview of the data they will receive at runtime, you will now be asked to pick a dataset to use for preview generation","Choose dataset to generate preview with");

                var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(datasets, false, false);

                if (dialog.ShowDialog() == DialogResult.OK)
                    selected = dialog.Selected  as ExtractableDataSet;
            }
            if(selected != null)
            {
                _pipelineSelectionUI1.InitializationObjectsForPreviewPipeline.Clear();
                    
                //create a request for an empty bundle (just the dataset) and just top 100 so that user can preview what is going on in his pipeline
                _pipelineSelectionUI1.InitializationObjectsForPreviewPipeline.Add(new ExtractDatasetCommand(RepositoryLocator,_configurationToExecute, new ExtractableDatasetBundle(selected), " TOP 100 "));
                _pipelineSelectionUI1.InitializationObjectsForPreviewPipeline.Add(DataLoadInfo.Empty);
            }
        }

        private void chooseExtractablesUI1_BundleSelected(object sender, IExtractCommand command)
        {
            var toSelect = tabPagesDictionary.Seconds.SingleOrDefault(ui => ui.ExtractCommand  == command);

            if(toSelect != null)
                tabControl1.SelectTab(tabPagesDictionary.GetBySecond(toSelect));
        }
        
        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            
            //don't accept refresh while executing
            if (extractionInProgress)
                return;

            SetupFor(databaseObject);
            chooseExtractablesUI1.Setup(_configurationToExecute);
        }

        private bool _firstTime = true;
        private void SetupFor(ExtractionConfiguration configuration)
        {
           
            Project = (Project) configuration.Project;
            _configurationToExecute = configuration;

            if (_firstTime)
            {
                this._pipelineSelectionUI1 = new PipelineSelectionUI<DataTable>(null, null, RepositoryLocator.CatalogueRepository);
                this._pipelineSelectionUI1.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
                this._pipelineSelectionUI1.Location = new Point(12, 621);
                this._pipelineSelectionUI1.Name = "_pipelineSelectionUI1";
                this._pipelineSelectionUI1.Dock = DockStyle.Fill;
                this._pipelineSelectionUI1.TabIndex = 25;
                panel1.Controls.Add(_pipelineSelectionUI1);
            }
            _firstTime = false;

            TopX = -1;
            
            if (_configurationToExecute.Cohort_ID == null)
                throw new Exception("There is no cohort associated with this extraction!");

            _pipelineSelectionUI1.Context = ExtractionPipelineHost.Context;
            _pipelineSelectionUI1.OnBeforeLaunchEdit += PipelineSelectionUI1OnBeforeLaunchEdit;

            if (HasConfigurationPreviouslyBeenReleased(_configurationToExecute))
            {
                lblAlreadyReleased.Visible = true;
                this.Enabled = false;//disable entire form
            }
            try
            {
                if (configuration.DefaultPipeline_ID != null)
                    _pipelineSelectionUI1.Pipeline = configuration.DefaultPipeline;
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }

        public void ConsultAboutClosing(object sender, FormClosingEventArgs e)
        {
            if (datasetsCurrentlyExecuting > 0)
            {
                MessageBox.Show("There are currently " + datasetsCurrentlyExecuting + " datasets executing, please abort these before closing");
                e.Cancel = true;
            }
        }

        public override string GetTabName()
        {
            return "Execute:" + base.GetTabName();
        }

        public void SetRequest(ExecuteExtractionUIRequest request)
        {
            if (request.AutoStart)
                btnStart_Click(null, null);
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExecuteExtractionUI_Design, UserControl>))]
    public abstract class ExecuteExtractionUI_Design:RDMPSingleDatabaseObjectControl<ExtractionConfiguration>
    {
    }
}

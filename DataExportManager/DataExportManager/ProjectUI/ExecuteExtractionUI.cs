using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.QueryBuilding;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using RDMPAutomationService.Options;
using RDMPAutomationService.Runners;
using RDMPObjectVisualisation.Pipelines;
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableLibraryCode;
using ReusableUIComponents;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Allows you to execute an extraction of a project configuration (Generate anonymous project data extractions for researchers).  You should make sure that you have already selected 
    /// the correct datasets, filters, transforms etc to meet the researchers project requirements (and governance approvals) - See ExtractionConfigurationUI and ConfigureDatasetUI.
    /// 
    /// <para>Start by selecting which datasets you want to execute (this can be an iterative process - you can extract half of them overnight and then come back and extract the other half the 
    /// next night).  See ChooseExtractablesUI for how to select datasets.</para>
    /// 
    /// <para>Next you should select/create a new extraction pipeline (See 'A Brief Overview Of What A Pipeline Is' in UserManual.docx).  This will determine the format of the extracted data
    /// (e.g. .CSV or .MDB database file or any other file for which you have a plugin implemented for).</para>
    /// </summary>
    public partial class ExecuteExtractionUI : ExecuteExtractionUI_Design
    {
        private IPipelineSelectionUI _pipelineSelectionUI1;
            
        public int TopX { get; set; }
        private ExtractionConfiguration _extractionConfiguration;
        

        private const string Globals = "Globals";
        private IMapsDirectlyToDatabaseTable[] _globals;
        private IExtractableDataSet[] _datasets;
        private Dictionary<IExtractableDataSet, List<IMapsDirectlyToDatabaseTable>> _bundledStuff;

        private const string CoreDatasets = "Core";
        private const string ProjectSpecificDatasets = "Project Specific";

        public ExecuteExtractionUI()
        {
            InitializeComponent();
            AssociatedCollection = RDMPCollection.DataExport;

            checkAndExecuteUI1.CommandGetter = CommandGetter;
            checkAndExecuteUI1.StateChanged += CheckAndExecuteUI1OnStateChanged;
            checkAndExecuteUI1.GroupBySender();

            olvName.ImageGetter = Name_ImageGetter;
            olvState.ImageGetter = State_ImageGetter;
            olvDatasets.ChildrenGetter = ChildrenGetter;
            olvDatasets.CanExpandGetter = CanExpandGetter;
            olvDatasets.HierarchicalCheckboxes = true;

        }

        private void CheckAndExecuteUI1OnStateChanged(object sender, EventArgs eventArgs)
        {
            olvDatasets.RefreshObjects(olvDatasets.Objects.Cast<object>().ToArray());
        }


        private bool CanExpandGetter(object model)
        {
            var e = ChildrenGetter(model);
            return  e != null && e.Cast<object>().Any();
        }

        private IEnumerable ChildrenGetter(object model)
        {
            if (model as string == Globals)
                return _globals;

            if (model as string == CoreDatasets)
                return _datasets.Where(ds => ds.Project_ID == null);

            if (model as string == ProjectSpecificDatasets)
                return _datasets.Where(ds => ds.Project_ID != null);

            var eds = model as IExtractableDataSet;

            if (_bundledStuff != null && eds != null && _bundledStuff.ContainsKey(eds))
                return _bundledStuff[eds];

            return null;
        }

        private object State_ImageGetter(object rowObject)
        {
            var extractionRunner = checkAndExecuteUI1.CurrentRunner as ExtractionRunner;
            var eds = rowObject as ExtractableDataSet;

            if (extractionRunner == null || eds == null)
                return null;

            var state = extractionRunner.GetState(eds);

            if (state == null)
                return null;

            return _activator.CoreIconProvider.GetImage(state);
        }

        private object Name_ImageGetter(object rowobject)
        {
            if (rowobject is string)
                return _activator.CoreIconProvider.GetImage(RDMPConcept.CatalogueFolder);
            
            return _activator.CoreIconProvider.GetImage(rowobject);
        }

        private RDMPCommandLineOptions CommandGetter(CommandLineActivity activityRequested)
        {
            return new ExtractionOptions() { 
                Command = activityRequested,
                ExtractGlobals = olvDatasets.IsChecked(Globals),
                Datasets = _datasets.Where(olvDatasets.IsChecked).Select(ds => ds.ID).ToArray(),
                ExtractionConfiguration = _extractionConfiguration.ID,
                Pipeline = _pipelineSelectionUI1.Pipeline == null?0:_pipelineSelectionUI1.Pipeline.ID
            };
        }

        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            
            _extractionConfiguration = databaseObject;
            
            checkAndExecuteUI1.SetItemActivator(activator);

            olvDatasets.ClearObjects();

            _globals = _extractionConfiguration.GetGlobals();
            _datasets = databaseObject.SelectedDataSets.Select(ds => ds.ExtractableDataSet).ToArray();
            GetBundledStuff();

            olvDatasets.DisableObjects(_globals.Union(_bundledStuff.Values.SelectMany(v=>v)));

            olvDatasets.AddObjects(new object[] { Globals, CoreDatasets, ProjectSpecificDatasets });
            
            
            //don't accept refresh while executing
            if (checkAndExecuteUI1.IsExecuting)
                return;
            
            if (_pipelineSelectionUI1 == null)
            {
                //create a new selection UI (pick an extraction pipeliene UI)
                var useCase = new ExtractionPipelineUseCase(_extractionConfiguration.Project);
                var factory = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, null, useCase);

                _pipelineSelectionUI1 = factory.Create("Extraction Pipeline", DockStyle.Fill, panel1);

                //if the configuration has a default then use that pipeline
                if (_extractionConfiguration.DefaultPipeline_ID != null)
                    _pipelineSelectionUI1.Pipeline = _extractionConfiguration.DefaultPipeline;
            }

            TopX = -1;

            olvDatasets.ExpandAll();
            olvDatasets.CheckAll();
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

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            olvDatasets.ModelFilter = new TextMatchFilter(olvDatasets,tbFilter.Text);
            olvDatasets.UseFiltering = true;
        }

        private void GetBundledStuff()
        {
            _bundledStuff = new Dictionary<IExtractableDataSet, List<IMapsDirectlyToDatabaseTable>>();

            foreach (IExtractableDataSet dataset in _datasets)
            {
                _bundledStuff.Add(dataset, new List<IMapsDirectlyToDatabaseTable>());
                _bundledStuff[dataset].AddRange(dataset.Catalogue.GetAllSupportingDocuments(FetchOptions.ExtractableLocals));
                _bundledStuff[dataset].AddRange(dataset.Catalogue.GetAllSupportingSQLTablesForCatalogue(FetchOptions.ExtractableLocals));
            }
        }
        /*

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

                //if there are globals selected for extraction
                if (globals.Any())
                {

                    var host = new ExecuteDatasetExtractionHostUI(new ExtractGlobalsCommand(RepositoryLocator, Project, _configurationToExecute, globals), Project, _dataLoadInfo, _pipelineSelectionUI1.Pipeline);
                    host.RepositoryLocator = RepositoryLocator;
                    host.Dock = DockStyle.Fill;

                    //add a new tab page for this bundle execution
                    var tabPage = new TabPage("Globals");
                    tabPage.Controls.Add(host);

                    tabControl1.TabPages.Add(tabPage);
                    tabPagesDictionary.Add(tabPage, host);

                    datasetsCurrentlyExecuting++;
                    host.DoExtraction();
                    host.Finished += host_Finished;
                    //////////////////////////////////
                    //create UI tab for globals
                    //var globalsTab = new TabPage("Globals");
                    //tabControl1.TabPages.Add(globalsTab);

                    ////with a progressUI on it
                    //var progressUI = new ProgressUI();
                    //progressUI.Dock = DockStyle.Fill;
                    //globalsTab.Controls.Add(progressUI);

                    //var globalExtractor = new ExtractionPipelineUseCase(Project, ExtractDatasetCommand.EmptyCommand, _pipelineSelectionUI1.Pipeline, _dataLoadInfo);

                    //Thread t = new Thread(() =>
                    //    {
                    //        try
                    //        {
                    //            progressUI.ShowRunning(true);

                    //            globalExtractor.ExtractGlobalsForDestination(Project,
                    //                _configurationToExecute,
                    //                globals,
                    //                progressUI,
                    //                _dataLoadInfo);
                    //        }
                    //        finally
                    //        {
                    //            progressUI.ShowRunning(false);
                    //        }
                    //    }   
                    //);

                    //t.Start();
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
        
        private void chooseExtractablesUI1_BundleSelected(object sender, IExtractCommand command)
        {
            var toSelect = tabPagesDictionary.Seconds.SingleOrDefault(ui => ui.ExtractCommand  == command);

            if(toSelect != null)
                tabControl1.SelectTab(tabPagesDictionary.GetBySecond(toSelect));
        }
        
        

        public override void ConsultAboutClosing(object sender, FormClosingEventArgs e)
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

        public void Start()
        {
            btnStart_Click(null, null);
        }*/
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExecuteExtractionUI_Design, UserControl>))]
    public abstract class ExecuteExtractionUI_Design:RDMPSingleDatabaseObjectControl<ExtractionConfiguration>
    {
    }
}

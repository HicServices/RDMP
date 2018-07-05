using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Ticketing;
using CatalogueLibrary.Repositories.Construction;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents;
using ReusableUIComponents.SqlDialogs;

namespace DataExportManager.DataRelease
{
    public delegate void ReleaseRequested(object sender, ReleasePotential[] datasetReleasePotentials,ReleaseEnvironmentPotential environmentPotential);
    public delegate void ReleasePatchRequested(object sender, ReleasePotential datasetReleasePotential, ReleaseEnvironmentPotential environmentPotential);
    
    /// <summary>
    /// The ultimate end point of the Data Export Manager is the provision of a packaged up Release of all the anonymised datasets for all the cohort(s) (e.g. 'Cases' and 'Controls') in
    /// a research project.  There is no going back once you have sent the package to the researcher, if you have accidentally included the wrong datasets or supplied identifiable data
    /// (e.g. in a free text field) then you are in big trouble.  For this reason the 'Release' process is a tightly controlled sequence which the RDMP undertakes to try to reduce error.
    /// 
    /// <para>In this control you will see all the currently selected datasets in a project configuration and the state of the dataset extraction (from the RDMP's perspective) as well as the 
    /// status of the 'Environment' (Ticketing System).  Right clicking on a dataset will give you options appropriate to it's state (See DataReleaseKeyUI).</para>
    /// 
    /// <para>Extraction of large datasets can take days or weeks and a project extraction is an ongoing exercise.  It is possible that by the time you come to release a project some of the
    /// early datasets have been changed or the files deleted etc.  The status of each extracted dataset (See DataReleaseKeyUI) is shown in the list box.  You can only do an extraction
    /// once all the datasets in the configuration are 'File exists and is current'.</para>
    /// 
    /// <para>In addition to verifying the datasets you can tie the RDMP into your ticketing system.  For example if you have tickets for each project extraction with stages for validation
    /// (so that data analysts can log time against validation and sign off on it etc) then you can setup Data Export Manager when the 'Release' Ticket is at a certain state (e.g. validated).
    /// To configure a ticketing system see TicketingSystemConfigurationUI.</para>
    /// 
    /// <para>If you haven't configured a Ticketing System then you shouldn't have to worry about the Environment State.</para>
    /// </summary>
    public partial class ConfigurationReleasePotentialUI : RDMPUserControl
    {
        /// <summary>
        /// Control will try to resize itself so that it occupies this much space + the height of the list control items 
        /// </summary>
        private const int MaxHeadroom = 200;
        private ExtractionConfiguration _configuration;
        ContextMenuStrip RightClickMenu = new ContextMenuStrip();

        public event ReleasePatchRequested RequestPatchRelease;
        public event ReleaseRequested RequestRelease;
        
        private Project _project = null;

        public ExtractionConfiguration Configuration
        {
            get { return _configuration; }
            private set { _configuration = value; }
        }

        //Constructor
        public ConfigurationReleasePotentialUI()
        {
            InitializeComponent();

            this.tlvDatasets.CellRightClick += (s, e) => { e.MenuStrip = ShowRightClickMenuFor(e.Model, e.Column); }; 

            DoTransparencyProperly.ThisHoversOver(ragSmileyEnvironment,lblConfigurationInvalid);
        }
        
        Thread SetupUIThread = null;

        public void AbortAsyncLoading()
        {
            if(SetupUIThread != null)
                SetupUIThread.Interrupt();
            else 
                return;

            int timeout = 500;
            while(SetupUIThread != null && (timeout-=100)>0 )
                Thread.Sleep(100);
            
            if (timeout <= 0)
                SetupUIThread.Abort();
            
        }

        private void SetupUIAsync(ExtractionConfiguration value)
        {
            try
            {
                int timeout = 10000;

                //wait for window handle to be created
                while (!IsHandleCreated && (timeout-=100) > 0)
                {
                    Thread.Sleep(100);
                    if(IsDisposed)
                        return;
                }

                ISelectedDataSets[] currentlyConfiguredDatasets = Configuration.SelectedDataSets;

                //identify old CumulativeExtractionResults that are no longer active (user extracted a dataset then removed it from the configuration)
                var oldResults = Configuration.CumulativeExtractionResults
                                    .Where(result => currentlyConfiguredDatasets.All(dataset => dataset.ID != result.ExtractableDataSet_ID)).ToArray();

                if (oldResults.Any())
                {
                    string message = "In Configuration " + Configuration + ":" + Environment.NewLine + Environment.NewLine + 
                        "The following CumulativeExtractionResults reflect datasets that were previously extracted under the existing Configuration but are no longer in the CURRENT configuration:";

                    message = oldResults.Aggregate(message, (s, n) => s + Environment.NewLine + n.ExtractableDataSet);

                    if (MessageBox.Show(message, "Delete expired CumulativeExtractionResults for configuration", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        foreach (var result in oldResults)
                            result.DeleteInDatabase();
                    }
                }

                var oldLostSupplemental = Configuration.CumulativeExtractionResults
                                                           .SelectMany(c => c.SupplementalExtractionResults)
                                                           .Union(RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<SupplementalExtractionResults>(Configuration))
                                                           .Where(s => !RepositoryLocator.ArbitraryDatabaseObjectExists(s.RepositoryType, s.ExtractedType, s.ExtractedId))
                                                           .ToList();

                if (oldLostSupplemental.Any())
                {
                    string message = "In Configuration " + Configuration + ":" + Environment.NewLine + Environment.NewLine +
                                     "The following list reflect objects (supporting sql, lookups or documents) " +
                                     "that were previously extracted but have since been deleted:";

                    message = oldLostSupplemental.Aggregate(message, (s, n) => s + Environment.NewLine + n.DestinationDescription);

                    if (MessageBox.Show(message, "Delete expired Extraction Results for configuration?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        foreach (var result in oldLostSupplemental)
                            result.DeleteInDatabase();
                    }
                }


                ////add globals to the globals category
                //Categories[Globals].AddRange(RepositoryLocator.CatalogueRepository.GetAllObjects<SupportingDocument>().Where(d => d.IsGlobal && d.Extractable));

                ////add global SQLs to globals category
                //Categories[Globals].AddRange(RepositoryLocator.CatalogueRepository.GetAllObjects<SupportingSQLTable>().Where(s => s.IsGlobal && s.Extractable));

                //add the bundle
                //Categories[Bundles].AddRange(collection.Datasets);

                // TODO: add Global Release Potentials!

                if (IsDisposed)
                    return;

                //notify UI that data files have been evaluated for this project
                //Invoke((MethodInvoker) GenerateSummaryOfExtractsSoFar);

                tlvDatasets.SetObjects(ReleasePotentials);

                //Can expand dataset bundles and also can expand strings for which the dictionary has a key for that string (i.e. the strings "Custom Tables" etc)
                tlvDatasets.CanExpandGetter = x => x is ReleasePotential;
                tlvDatasets.ChildrenGetter += ChildrenGetter;
                tlvDatasets.Invoke(new MethodInvoker(() => 
                    {
                        tlvDatasets.CheckAll();
                        tlvDatasets.ExpandAll();
                        Height = 19*tlvDatasets.Items.Count + MaxHeadroom;
                    }));

                try
                {
                    //notify environment that environment evaluation is done
                    EnvironmentalPotential = new ReleaseEnvironmentPotential(value);//takes a while

                }
                catch (Exception e)
                {
                    ragSmileyEnvironment.Invoke(new MethodInvoker(() =>
                    {
                        ragSmileyEnvironment.Visible = true;
                        ragSmileyEnvironment.Fatal(e);
                    }));
                    
                    return;
                }

                if(IsDisposed)
                    return;

                Application.DoEvents();

                if(!IsHandleCreated)
                    return;

                Invoke((MethodInvoker)SetupUIAsyncCallback);//callback to UI - on main Thread
            }
            catch (ThreadInterruptedException)
            {
                return;//user gave up
            }
            finally
            {
                SetupUIThread = null; //always remove reference so we can go again
            }
        }

        private IEnumerable ChildrenGetter(object model)
        {
            if (!(model is ReleasePotential)) 
                return null;
            
            var potential = (ReleasePotential) model;
            var results = new List<ResultDetails>();
            results.AddRange(potential.Assessments.Keys.OfType<CumulativeExtractionResults>().Select(c => new ResultDetails()
            {
                Name = c.ExtractableDataSet.Catalogue.ToString(),
                Type = c.ExtractableDataSet.Catalogue.GetType().FullName,
                Releasability = potential.Assessments[c]
            }));
                
            var factory = new ExtractCommandCollectionFactory();
            var collection = factory.Create(RepositoryLocator, Configuration);

            var datasetBundle = collection.Datasets
                .First(x => x.SelectedDataSets.ExtractableDataSet_ID == potential.DataSet.ID)
                .DatasetBundle;
            var notExtracted = datasetBundle.Documents.Select(d => (d as INamed))
                .Union(datasetBundle.SupportingSQL.Select(s => (s as INamed)))
                .Union(datasetBundle.LookupTables.Select(l => (l.TableInfo as INamed)));

            foreach (var item in notExtracted)
            {
                var result = potential.Assessments.Keys.OfType<SupplementalExtractionResults>().FirstOrDefault(a => a.ExtractedId == item.ID);
                if (result != null)
                    results.Add(new ResultDetails()
                    {
                        Name = item.Name,
                        Type = item.GetType().FullName,
                        Releasability = potential.Assessments[result]
                    });
                else
                    results.Add(new ResultDetails()
                    {
                        Name = item.Name,
                        Type = item.GetType().FullName,
                        Releasability = Releaseability.NeverBeenSuccessfullyExecuted
                    });
            }

            return results;
        }

        private class ResultDetails
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public Releaseability Releasability { get; set; }
        }

        private void SetupUIAsyncCallback()
        {
            //assume there is a problem unless we identify otherwise (never let users release datasets if at all possible)
            btnRelease.Enabled = false;

            bool environmentReady = false;

            ragSmileyEnvironment.Visible = true;
            ragSmileyEnvironment.Reset();

            //if environment is ready
            if (EnvironmentalPotential.Assesment == TicketingReleaseabilityEvaluation.Releaseable)
            {
                //environment is READY!
                lblConfigurationInvalid.Text = "Environment Ready";
                lblConfigurationInvalid.ForeColor = Color.Green;
                environmentReady = true;
                
            }
            else if(EnvironmentalPotential.Assesment == TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
            {
                //environment is READY!
                lblConfigurationInvalid.Text = "Environment Ready (But you have no ticketing system)";
                lblConfigurationInvalid.ForeColor = Color.Orange;
                environmentReady = true;
                ragSmileyEnvironment.Warning(new Exception("No Ticketing System Configured"));
            }
            else
            {
                //environment is NOT READY!
                lblConfigurationInvalid.Text = EnvironmentalPotential.Reason;

                ragSmileyEnvironment.Visible = true;
                ragSmileyEnvironment.Fatal(EnvironmentalPotential.Exception);
                lblConfigurationInvalid.ForeColor = Color.DarkRed;
            }

            if(environmentReady)
                //if environment is ready and ALL datasets are also ready (or are Catalogue differences which is allowable) so permit them to release stuff
                if(tlvDatasets.Objects.Cast<object>()
                                      .Where(o => !(o is ReleasePotential))
                                      .Cast<KeyValuePair<IExtractionResults, Releaseability>>()
                                      .All(rp => rp.Value == Releaseability.Releaseable || rp.Value == Releaseability.ColumnDifferencesVsCatalogue))
                    btnRelease.Enabled = true; //you can release
        }
        
        //potential of environment the Configuration is in (Ticketing system, and maybe folders available, writeability etc)
        public ReleaseEnvironmentPotential EnvironmentalPotential;
        
        //potential of each of the datasets in the Configuration
        public List<ReleasePotential> ReleasePotentials = new List<ReleasePotential>();
        private IActivateItems _activator;
        
        protected override bool ProcessKeyPreview(ref Message m)
        {

            PreviewKey p = new PreviewKey(ref m, ModifierKeys);

            
            if (p.IsKeyDownMessage && p.e.KeyCode == Keys.C && p.e.Control)
            {
                if(CopySelectedItemToClipboard())
                    p.Trap(this);
            }

            return base.ProcessKeyPreview(ref m);
        }

        private bool CopySelectedItemToClipboard()
        {
            if (tlvDatasets.CheckedItems.Count == 0)
                return false;

            string toCopy = "";
            foreach (var item in tlvDatasets.CheckedItems)
                toCopy += item + Environment.NewLine;

            Clipboard.SetText(toCopy);
            return true;
        }

        private ContextMenuStrip ShowRightClickMenuFor(object model, OLVColumn column)
        {
            var atomicCommandExecutionFactory = new AtomicCommandUIFactory(_activator);

            //create right click context menu
            RightClickMenu = new ContextMenuStrip();

            var potential = model as ReleasePotential;
            if (potential == null)
                return null;

            if (potential.SqlExtracted != potential.SqlCurrentConfiguration)
                RightClickMenu.Items.Add("View SQL Extracted vs Now", CatalogueIcons.SqlThenVSNow,
                    (sender, args) =>
                        new SQLBeforeAndAfterViewer(potential.SqlExtracted, potential.SqlCurrentConfiguration,
                            "Original Extraction SQL:", "Current Configuration:", "SQL Desync", MessageBoxButtons.OK)
                            .Show());

            if (potential.Assessments[potential.DatasetExtractionResult] == Releaseability.ColumnDifferencesVsCatalogue)
                RightClickMenu.Items.Add("Show Column Changes VS Catalogue", CatalogueIcons.SqlThenVSNow, (sender, args) =>
                    new SQLBeforeAndAfterViewer(
                        string.Join("\r\n",
                            potential.ColumnsThatAreDifferentFromCatalogue.Values.Where(v => v != null)
                                     .Select(val => val.SelectSQL)),
                        string.Join("\r\n",
                            potential.ColumnsThatAreDifferentFromCatalogue.Select(
                                        kvp =>
                                            kvp.Value == null
                                                ? kvp.Key.SelectSQL + "/*Column no longer appears in catalogue*/"
                                                : kvp.Key.SelectSQL)),
                                "Catalogue Version", "Current Configuration Version",
                                "Catalogue Vs DataExportManager Configuration", MessageBoxButtons.OK).Show());

            if (potential.ExtractDirectory != null && potential.ExtractDirectory.Exists)
            {
                RightClickMenu.Items.Add("Open Folder", CatalogueIcons.ExtractionDirectoryNode,
                    (sender, args) => Process.Start(potential.ExtractDirectory.FullName));

                //if (listView1.SelectedItems.Count <= 1)
                //    RightClickMenu.Items.Add("Add file as Patch (Not Recommended)", _activator.CoreIconProvider.GetImage(RDMPConcept.Release, OverlayKind.Problem),
                //        (sender, args) => AddAsPatchContent(tag));
                //get the .Tag element of the selected (right clicked) item (because user isn't doing multi-select)
                //else
                //    RightClickMenu.Items.Add("Add all selected files as Patch (Not Recommended)", _activator.CoreIconProvider.GetImage(RDMPConcept.Release, OverlayKind.Problem),
                //        (sender, args) => AddAllAsPatchContent(
                //            listView1.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag).Cast<ReleasePotential>()
                //            //get all the .Tag elements
                //            ));
            }
            
            RightClickMenu.Items.Add(
                atomicCommandExecutionFactory.CreateMenuItem(new ExecuteCommandActivate(_activator, (ExtractionConfiguration)potential.Configuration)));

            RightClickMenu.Items.Add("Execute Extraction Configuration",
                _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.Execute),
                (sender, args) =>

                    new ExecuteCommandExecuteExtractionConfiguration(_activator).SetTarget((ExtractionConfiguration)potential.Configuration).Execute()

                        );

            return RightClickMenu;
        }

        private void AddAllAsPatchContent(IEnumerable<ReleasePotential> releasePotentials)
        {
            if (ConfirmReleaseEnvironmentAcceptable())
                foreach (ReleasePotential potential in releasePotentials)
                    RequestPatchRelease(this, potential, EnvironmentalPotential);
        }

        private void AddAsPatchContent(ReleasePotential datasetReleasePotential)
        {
            if (ConfirmReleaseEnvironmentAcceptable())
                RequestPatchRelease(this, datasetReleasePotential, EnvironmentalPotential);
        }

        private bool ConfirmReleaseEnvironmentAcceptable()
        {
            if (EnvironmentalPotential == null)
            {
                MessageBox.Show("Please wait till environment has been assessed");
                return false;
            }

            if (EnvironmentalPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable)
                if (MessageBox.Show("Environment is not currently Releaseable, it's current state is '" + EnvironmentalPotential.Assesment + "' ("+EnvironmentalPotential.Reason+") do you wish to perform an Illegal Patch? Your user account will forever be audited as the authorizer of this patch in place of JIRA workflows", "Perform Illegal Patch?", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                    return false;

            return true;
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            //if (RequestRelease != null)
            //    RequestRelease(this, listView1.Items.Cast<ListViewItem>().Select(i => i.Tag).Cast<ReleasePotential>().ToArray(), _environmentalPotential);
            //else
                throw new Exception("Nobody is listening to our event handler (RequestRelease) :(");
        }

        private void lblConfigurationInvalid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(EnvironmentalPotential != null && EnvironmentalPotential.Exception != null)
                ExceptionViewer.Show(EnvironmentalPotential.Exception);
        }

        public void SetConfiguration(IActivateItems activator, ExtractionConfiguration configuration)
        {
            _activator = activator;
            Configuration = configuration;

            PrepareUI();

            olvColumn1.ImageGetter += ImageGetter;
            olvColumn1.AspectGetter += AspectGetter;
            olvColumn2.ImageGetter += StatusGetter;
            olvColumn2.AspectGetter += (x => String.Empty);
        }

        private object StatusGetter(object rowObject)
        {
            if (rowObject is ResultDetails)
                return GetImageFromReleasability(((ResultDetails) rowObject).Releasability);

            return null;
        }

        private object AspectGetter(object rowObject)
        {
            if (rowObject is ReleasePotential)
                return (rowObject as ReleasePotential).DataSet.ToString();

            if (rowObject is ResultDetails)
                return ((ResultDetails)rowObject).Name;
            
            return null;
        }

        private object ImageGetter(object rowObject)
        {
            if (rowObject is ReleasePotential)
                return _activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue);

            if (rowObject is ResultDetails)
                return _activator.CoreIconProvider.GetImage(((ResultDetails) rowObject).Type.Split('.').Last());

            return null;
        }

        private string GetImageFromReleasability(Releaseability value)
        {
            switch (value)
            {
                case Releaseability.NeverBeenSuccessfullyExecuted:
                    return "NeverBeenGenerated";
                case Releaseability.ExtractFilesMissing:
                    return "FileMissing";
                case Releaseability.ExtractionSQLDesynchronisation:
                    return "OutOfSync";
                case Releaseability.CohortDesynchronisation:
                    return "WrongCohort";
                case Releaseability.Releaseable:
                    return "Releaseable";
                case Releaseability.ColumnDifferencesVsCatalogue:
                    return "DifferentFromCatalogue";
                case Releaseability.ExceptionOccurredWhileEvaluatingReleaseability:
                    return "Exception";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PrepareUI()
        {
            if (Configuration != null)
            {
                _project = (Project)Configuration.Project;

                //Deal with crazy newlines and super long configuration description
                lbConfigurationName.Text = "Configuration:" + Configuration + " (ID=" + Configuration.ID + ")";

                if (Configuration.Cohort_ID == null)
                {
                    lbCohortName.Text = "No Cohort Defined for Configuration!";
                    lbCohortName.Tag = null;
                    lbCohortName.ForeColor = Color.Red;
                }
                else
                {
                    try
                    {
                        var cohort = Configuration.Cohort;
                        lbCohortName.Text = "Cohort: " + cohort;
                        lbCohortName.Tag = cohort;
                        lbCohortName.ForeColor = Color.Black;
                    }
                    catch (Exception e)
                    {
                        ExceptionViewer.Show(e);
                        lbCohortName.Text = "Cohort: Error retrieving cohort";
                        lbCohortName.Tag = null;
                        lbCohortName.ForeColor = Color.Red;
                    }
                }

                //reset (since we are being switched to a different ExtractionConfiguration, we don't want holdovers)
                ReleasePotentials.Clear();
                tlvDatasets.Items.Clear();

                SetupUIThread = new Thread(() => SetupUIAsync(Configuration));
                SetupUIThread.Name = "SetupUIAsync ConfigID " + Configuration.ID;
                SetupUIThread.Start();
            }
            else
            {
                throw new NullReferenceException("Configuration null?!");
            }
        }
    }
}

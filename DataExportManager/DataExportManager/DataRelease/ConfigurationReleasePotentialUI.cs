using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Ticketing;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.CommandExecution.AtomicCommands;
using CatalogueManager.CommandExecution.AtomicCommands.UIFactory;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.CommandExecution.AtomicCommands;
using DataExportManager.ProjectUI;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.DataRelease;
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
            private set
            {
                btnRelease.Enabled = false;
                _configuration = value;

                if (value != null)
                {
                    _project = (Project)value.Project;

                    //Deal with crazy newlines and super long configuration description
                    lbConfigurationName.Text = "Configuration:" + value + " (ID=" + value.ID + ")";

                    if (value.Cohort_ID == null)
                    {
                        lbCohortName.Text = "No Cohort Defined for Configuration!";
                        lbCohortName.Tag = null;
                        lbCohortName.ForeColor = Color.Red;
                    }
                    else
                    {
                        try
                        {
                            var cohort = value.Cohort;
                            lbCohortName.Text = "Cohort:" + cohort;
                            lbCohortName.Tag = cohort;
                            lbCohortName.ForeColor = Color.Black;
                        }
                        catch (Exception e)
                        {
                            ExceptionViewer.Show(e);
                            lbCohortName.Text = "Cohort:Error retrieving cohort";
                            lbCohortName.Tag = null;
                            lbCohortName.ForeColor = Color.Red;
                        }
                    }

                    SetupUIThread = new Thread(()=> SetupUIAsync(value));
                    SetupUIThread.Name = "SetupUIAsync ConfigID " + value.ID;
                    SetupUIThread.Start();

                    //reset (incse we are being switched to a different ExtractionConfiguration, we don't want holdovers)
                    ReleasePotentials.Clear();
                    listView1.Items.Clear();

                }
                else
                {
                    throw new NullReferenceException("Configuration null?!");
                }
            }
        }

        //Constructor
        public ConfigurationReleasePotentialUI()
        {
            InitializeComponent();

            this.listView1.MouseClick += listView1_MouseClick;

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
                while (!IsHandleCreated && (timeout-=100)>0)
                {
                    Thread.Sleep(100);
                    if(IsDisposed)
                        return;
                }

                IExtractableDataSet[] currentlyConfiguredDatasets = Configuration.GetAllExtractableDataSets();


                //identify old CumulativeExtractionResults that are no longer active (user extracted a dataset then removed it from the configuration)
                var oldResults = Configuration.CumulativeExtractionResults
                    .Where(
                        result =>
                            currentlyConfiguredDatasets.All(dataset => dataset.ID != result.ExtractableDataSet_ID)).ToArray();

                if (oldResults.Any())
                {
                    string message = "In Configuration:" + Configuration + ":" + Environment.NewLine + Environment.NewLine + 
                        "The following CumulativeExtractionResults reflect datasets that were previously extracted under the existing Configuration but are no longer in the CURRENT configuration:";

                    message = oldResults.Aggregate(message, (s, n) => s + Environment.NewLine + n.ExtractableDataSet);

                    if (
                        MessageBox.Show(message, "Delete expired CumulativeExtractionResults for configuration", MessageBoxButtons.YesNo) ==
                        DialogResult.Yes)
                    {
                        foreach (var result in oldResults)
                            result.DeleteInDatabase();
                    }
                }
                
                //create new ReleaseAssesments
                foreach (ExtractableDataSet dataSet in currentlyConfiguredDatasets)
                    ReleasePotentials.Add(new ReleasePotential(RepositoryLocator,Configuration, dataSet));

                if (IsDisposed)
                    return;

                //notify UI that data files have been evaluated for this project
                Invoke((MethodInvoker) GenerateSummaryOfExtractsSoFar);

                try
                {
                    //notify environment that environment evaluation is done
                    _environmentalPotential = new ReleaseEnvironmentPotential(value);//takes a while

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

        private void SetupUIAsyncCallback()
        {
            //assume there is a problem unless we identify otherwise (never let users release datasets if at all possible)
            btnRelease.Enabled = false;

            bool environmentReady = false;

            ragSmileyEnvironment.Visible = true;
            ragSmileyEnvironment.Reset();

            //if environment is ready
            if (_environmentalPotential.Assesment == TicketingReleaseabilityEvaluation.Releaseable)
            {
                //environment is READY!
                lblConfigurationInvalid.Text = "Environment Ready";
                lblConfigurationInvalid.ForeColor = Color.Green;
                environmentReady = true;
                
            }
            else if(_environmentalPotential.Assesment == TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
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
                lblConfigurationInvalid.Text = _environmentalPotential.Reason;

                ragSmileyEnvironment.Visible = true;
                ragSmileyEnvironment.Fatal(_environmentalPotential.Exception);
                lblConfigurationInvalid.ForeColor = Color.DarkRed;
            }

            if(environmentReady)
                //if environment is ready and ALL datasets are also ready (or are Catalogue differences which is allowable) so permit them to release stuff
                if(listView1.Items.Cast<ListViewItem>().Select(i=>i.Tag).Cast<ReleasePotential>().All(rp=>rp.Assesment == Releaseability.Releaseable || rp.Assesment == Releaseability.ColumnDifferencesVsCatalogue))
                    btnRelease.Enabled = true; //you can release
        }
        
        //potential of environment the Configuration is in (Ticketing system, and maybe folders available, writeability etc)
        private ReleaseEnvironmentPotential _environmentalPotential;
        
        //potential of each of the datasets in the Configuration
        List<ReleasePotential> ReleasePotentials = new List<ReleasePotential>();
        private IActivateItems _activator;
        
        private void GenerateSummaryOfExtractsSoFar()
        {
          
            //create visual representation of these
            foreach (ReleasePotential releasePotential in ReleasePotentials)
            {
                ListViewItem i = new ListViewItem();
                i.SubItems.Add(releasePotential.DataSet.ToString());
                i.Tag = releasePotential;

                switch (releasePotential.Assesment)
                {
                    case Releaseability.NeverBeensuccessfullyExecuted:
                        i.ImageKey = "NeverBeenGenerated";
                        break;

                    case Releaseability.ExtractFilesMissing:
                        i.ImageKey = "FileMissing";
                        i.SubItems.Add(((DateTime)releasePotential.DateOfExtraction).ToString());
                        break;

                    case Releaseability.ExtractionSQLDesynchronisation:
                        i.ImageKey = "OutOfSync";
                        i.SubItems.Add(((DateTime)releasePotential.DateOfExtraction).ToString());
                        break;

                    case Releaseability.CohortDesynchronisation:
                        i.ImageKey = "WrongCohort";
                        i.SubItems.Add(((DateTime)releasePotential.DateOfExtraction).ToString());
                        break;

                    case Releaseability.Releaseable:
                        i.ImageKey = "Releaseable";
                        i.SubItems.Add(((DateTime)releasePotential.DateOfExtraction).ToString());
                        break;

                    case Releaseability.ColumnDifferencesVsCatalogue:
                        i.ImageKey = "DifferentFromCatalogue";
                        i.SubItems.Add(((DateTime)releasePotential.DateOfExtraction).ToString());
                        break;

                    case Releaseability.ExceptionOccurredWhileEvaluatingReleaseability :
                        i.ImageKey = "Exception";
                        i.SubItems.Add(releasePotential.Exception.ToString());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                listView1.Items.Add(i);
            }

            
            //depending on the number of datasets, set the preferred size to this
            if (listView1.Items.Count > 0)
            {
                Size = new Size(this.Size.Width,
                    (listView1.GetItemRect(0).Height * listView1.Items.Count) + MaxHeadroom);

                foreach (ColumnHeader column in listView1.Columns)
                    column.Width = -2; //magical (apparently it resizes to max width of content or header)
            }
            else
                this.Size = new Size(this.Size.Width, MaxHeadroom);
        }
        
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
            if (listView1.SelectedItems.Count == 0)
                return false;


            string toCopy = "";
            foreach (ListViewItem item in listView1.SelectedItems)
                toCopy += ((ReleasePotential) item.Tag).ToString() + Environment.NewLine;

            Clipboard.SetText(toCopy);
            return true;
        }

        void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                ListViewItem item = listView1.GetItemAt(e.Location.X, e.Location.Y);

                if(item == null)
                    return;
                else
                {
                    ShowRightClickMenuFor((ReleasePotential)item.Tag,e.Location);
                }
            }
        }

        private void ShowRightClickMenuFor(ReleasePotential tag, Point point)
        {
            var atomicCommandExecutionFactory = new AtomicCommandUIFactory(_activator.CoreIconProvider);


            //create right click context menu
            RightClickMenu = new ContextMenuStrip();

            if (tag.SqlExtracted != tag.SqlCurrentConfiguration)
                RightClickMenu.Items.Add("View SQL Extracted vs Now", CatalogueIcons.SqlThenVSNow,
                    (sender, args) =>
                        new SQLBeforeAndAfterViewer(tag.SqlExtracted, tag.SqlCurrentConfiguration,
                            "Original Extraction SQL:", "Current Configuration:", "SQL Desync", MessageBoxButtons.OK)
                            .Show());

            if (tag.Assesment == Releaseability.ColumnDifferencesVsCatalogue)
                RightClickMenu.Items.Add("Show Column Changes VS Catalogue", CatalogueIcons.SqlThenVSNow, (sender, args) =>
                    new SQLBeforeAndAfterViewer(
                        string.Join("\r\n",
                            tag.ColumnsThatAreDifferentFromCatalogue.Values.Where(v => v != null)
                                .Select(val => val.SelectSQL)),
                        string.Join("\r\n",
                            tag.ColumnsThatAreDifferentFromCatalogue.Select(
                                kvp =>
                                    kvp.Value == null
                                        ? kvp.Key.SelectSQL + "/*Column no longer appears in catalogue*/"
                                        : kvp.Key.SelectSQL)),
                        "Catalogue Version", "Current Configuration Version",
                        "Catalogue Vs DataExportManager Configuration", MessageBoxButtons.OK).Show());

            if (tag.ExtractDirectory != null && tag.ExtractDirectory.Exists)
            {
                RightClickMenu.Items.Add("Open Folder", CatalogueIcons.ExtractionDirectoryNode,
                    (sender, args) => Process.Start(tag.ExtractDirectory.FullName));

                if (listView1.SelectedItems.Count <= 1)
                    RightClickMenu.Items.Add("Add file as Patch (Not Recommended)", _activator.CoreIconProvider.GetImage(RDMPConcept.Release,OverlayKind.Problem),
                        (sender, args) => AddAsPatchContent(tag));
                        //get the .Tag element of the selected (right clicked) item (because user isn't doing multi-select)
                else
                    RightClickMenu.Items.Add("Add all selected files as Patch (Not Recommended)", _activator.CoreIconProvider.GetImage(RDMPConcept.Release, OverlayKind.Problem),
                        (sender, args) => AddAllAsPatchContent(
                            listView1.SelectedItems.Cast<ListViewItem>().Select(i => i.Tag).Cast<ReleasePotential>()
                            //get all the .Tag elements
                            ));
            }


            RightClickMenu.Items.Add(
                atomicCommandExecutionFactory.CreateMenuItem(new ExecuteCommandActivate(_activator,(ExtractionConfiguration) tag.Configuration)));

            RightClickMenu.Items.Add("Execute Extraction Configuration",
                _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractionConfiguration, OverlayKind.Execute),
                (sender, args) =>

                    new ExecuteCommandExecuteExtractionConfiguration(_activator).SetTarget((ExtractionConfiguration) tag.Configuration).Execute()

                        );

            RightClickMenu.Show(listView1, point);

        }

        private void 
            AddAllAsPatchContent(IEnumerable<ReleasePotential> releasePotentials)
        {
            if (ConfirmReleaseEnvironmentAcceptable())
                foreach (ReleasePotential potential in releasePotentials)
                    RequestPatchRelease(this, potential, _environmentalPotential);
        }

        private void AddAsPatchContent(ReleasePotential datasetReleasePotential)
        {
            if (ConfirmReleaseEnvironmentAcceptable())
                RequestPatchRelease(this, datasetReleasePotential, _environmentalPotential);
        }

        private bool ConfirmReleaseEnvironmentAcceptable()
        {
            if (_environmentalPotential == null)
            {
                MessageBox.Show("Please wait till environment has been assessed");
                return false;
            }

            if (_environmentalPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable)
                if (MessageBox.Show("Environment is not currently Releaseable, it's current state is '" + _environmentalPotential.Assesment + "' ("+_environmentalPotential.Reason+") do you wish to perform an Illegal Patch? Your user account will forever be audited as the authorizer of this patch in place of JIRA workflows", "Perform Illegal Patch?", MessageBoxButtons.YesNoCancel) != DialogResult.Yes)
                    return false;

            return true;
        }

        private void btnRelease_Click(object sender, EventArgs e)
        {
            if (RequestRelease != null)
                RequestRelease(this, listView1.Items.Cast<ListViewItem>().Select(i => i.Tag).Cast<ReleasePotential>().ToArray(), _environmentalPotential);
            else
                throw new Exception("Nobody is listening to our event handler (RequestRelease) :(");
        }

        private void lblConfigurationInvalid_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(_environmentalPotential != null && _environmentalPotential.Exception != null)
                ExceptionViewer.Show(_environmentalPotential.Exception);
        }

        public void SetConfiguration(IActivateItems activator, ExtractionConfiguration configuration)
        {
            _activator = activator;
            Configuration = configuration;

        }
    }
}

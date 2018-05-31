using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Ticketing;
using CatalogueManager.Collections;
using CatalogueManager.ItemActivation;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportManager.Icons.IconProvision;
using DataExportManager.ProjectUI;
using DataExportLibrary;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease;
using RDMPObjectVisualisation.Pipelines;
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableUIComponents;

namespace DataExportManager.DataRelease
{
    /// <summary>
    /// Lists all the active (unreleased) configurations in a project extraction and allows you to start a data release with one or more of them.  Each configuration is hosted in a
    /// ConfigurationReleasePotentialUI (See ConfigurationReleasePotentialUI) which shows whether it is in a releasable state and allows you to add it to the Release.
    /// 
    /// <para> Once you have selected all the configurations you want to release click Release.</para>
    /// </summary>
    public partial class DataReleaseUI : DataReleaseUI_Design
    {
        private Project _project;

        private IPipelineSelectionUI _pipelineUI;
        private IExtractionConfiguration[] _unreleasedConfigurations;

        public Project Project
        {
            get { return _project; }
            private set
            {
                _project = value;

                SetupUIForProject(value);
            }
        }

        public FileInfo[] FinalFiles
        {
            get
            {
                List<FileInfo> toReturn = new List<FileInfo>();

                //foreach (var kvp in doReleaseAndAuditUI1.ConfigurationsForRelease)
                //    foreach (ReleasePotential potential in kvp.Value)
                //        toReturn.AddRange(potential.ExtractDirectory.GetFiles());

                return toReturn.ToArray();
            }
        }

        public DataReleaseUI()
        {
            InitializeComponent();

            AssociatedCollection = RDMPCollection.DataExport;
        }

        private void SetupUIForProject(Project project)
        {
            lblLoading.Visible = true;
            lblLoading.Refresh();

            foreach (Control c in flowLayoutPanel1.Controls)
            {
                var ui = c as ConfigurationReleasePotentialUI;
                if (ui != null)
                    ui.AbortAsyncLoading();
            }

            flowLayoutPanel1.Controls.Clear();

            if (project == null)
            {
                //clear other stuff here 
                flowLayoutPanel1.Controls.Add(new Label() {Text = "No Project Selected"});
            }
            else
            {
                //show all unreleased configurations
                _unreleasedConfigurations = project.ExtractionConfigurations.Where(c => !c.IsReleased).ToArray();

                //for each configuration defined in the project
                for (int index = 0; index < _unreleasedConfigurations.Length; index++)
                {
                    //create a UI that shows the datasets in it and their statuses and lets the user generate releases
                    IExtractionConfiguration configuration = _unreleasedConfigurations[index];

                    var configurationReleasePotentialUI = new ConfigurationReleasePotentialUI()
                    {
                        Width = flowLayoutPanel1.Width - 30
                    };

                    configurationReleasePotentialUI.SetConfiguration((IActivateItems) _activator, (ExtractionConfiguration) configuration);
                    configurationReleasePotentialUI.RepositoryLocator = RepositoryLocator;
                    configurationReleasePotentialUI.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    configurationReleasePotentialUI.RequestRelease += ConfigurationReleasePotentialUIOnRequestRelease;
                    configurationReleasePotentialUI.RequestPatchRelease += configurationReleasePotentialUI_RequestPatchRelease;

                    flowLayoutPanel1.Controls.Add(configurationReleasePotentialUI);
                }
            }

            lblLoading.Visible = false;
            SetupPipeline();
        }

        private void configurationReleasePotentialUI_RequestPatchRelease(object sender, ReleasePotential datasetReleasePotential, ReleaseEnvironmentPotential environmentPotential)
        {
            //doReleaseAndAuditUI1.AddPatchRelease(datasetReleasePotential, environmentPotential);
        }

        private void ConfigurationReleasePotentialUIOnRequestRelease(object sender, ReleasePotential[] datasetReleasePotentials, ReleaseEnvironmentPotential environmentPotential)
        {
            if (!datasetReleasePotentials.All(p =>
            {
                var dsReleasability = p.Assessments[p.DatasetExtractionResult];
                return dsReleasability == Releaseability.Releaseable ||
                       dsReleasability == Releaseability.ColumnDifferencesVsCatalogue;
            }))
                throw new Exception("Attempt made to release one or more datasets that are not assessed as being Releaseable (or ColumnDifferencesVsCatalogue)");

            if (environmentPotential.Assesment != TicketingReleaseabilityEvaluation.Releaseable && environmentPotential.Assesment != TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
                throw new Exception("Ticketing system decided that Environment was not ready for release");

            //doReleaseAndAuditUI1.AddToRelease(datasetReleasePotentials, environmentPotential);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            SetupUIForProject(Project);
        }
        
        private void DataReleaseManagementUI_Load(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Resize(object sender, EventArgs e)
        {
            foreach (Control c in flowLayoutPanel1.Controls)
                c.Width = flowLayoutPanel1.Width - 30; //allow scroll bar visibility
        }

        public override void SetDatabaseObject(IActivateItems activator, Project databaseObject)
        {
            base.SetDatabaseObject(activator, databaseObject);
            Project = databaseObject;
        }

        public override string GetTabName()
        {
            return "Release: " + base.GetTabName();
        }
        
        private void SetupPipeline()
        {
            if (_pipelineUI == null)
            {
                var releaseData = GetReleaseData();
                releaseData.IsDesignTime = true;
                var context = new ReleaseUseCase(_project, releaseData);
                _pipelineUI = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, null, context).Create("Release", DockStyle.Fill, pnlPipeline);
            }
        }

        private ReleaseData GetReleaseData()
        {
            return new ReleaseData
            {
                ConfigurationsForRelease = flowLayoutPanel1
                                                    .Controls
                                                    .Cast<ConfigurationReleasePotentialUI>()
                                                    .ToDictionary(crp => (IExtractionConfiguration)crp.Configuration, crp => crp.ReleasePotentials),
                EnvironmentPotential = flowLayoutPanel1
                                                    .Controls
                                                    .Cast<ConfigurationReleasePotentialUI>()
                                                    .Select(crp => crp.EnvironmentalPotential).FirstOrDefault(),
                ReleaseState = ReleaseState.Nothing
            };
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<DataReleaseUI_Design, UserControl>))]
    public abstract class DataReleaseUI_Design : RDMPSingleDatabaseObjectControl<Project>
    {

    }
}


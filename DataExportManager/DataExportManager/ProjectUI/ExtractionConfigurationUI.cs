using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.QueryBuilding.Parameters;
using CatalogueManager;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs;
using CatalogueManager.ExtractionUIs.FilterUIs.ParameterUIs.Options;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.SimpleControls;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.CohortCreationPipeline;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.Repositories;
using HIC.Logging;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;

using ReusableUIComponents.ChecksUI;

namespace DataExportManager.ProjectUI
{
    /// <summary>
    /// Allows you to choose the cohort and selected datasets for a project extraction configuration.  This will involve joining the selected datasets against the selected cohort (and
    /// substituting the private identifiers for project specific anonymous release identifiers) as well as applying an configured filters (See ConfigureDatasetUI).
    /// 
    /// You can have multiple active configurations in a project, for example you might extract 'Prescribing', 'Biochemistry' and 'Demography' for the cohort 'CasesForProject123' and
    /// only datasets 'Biochemistry' and 'Demography' for the cohort 'ControlsForProject123'.
    /// 
    /// Cohorts will only appear in the dropdown if the cohort manager database lists them as being associated with the Project (to help reduce the possibility of extracting the wrong
    /// cohort - which would be catastrophic!)
    /// 
    /// On the left you can see a list of all the currently extractable datasets (See DataSetManagementUI) that are not already part of the extraction configuration.  You can add these
    /// datasets to the configuration by selecting them and pressing the '>' button.  Once a dataset is included in the extraction you must choose which columns the researcher will receive
    /// and whether there are any dataset filters (e.g. is the researchers only allowed prescribing records for 'Propranolol').  For instructions on how to adjust the columns/filters see
    /// ConfigureDatasetUI
    /// </summary>
    public partial class ExtractionConfigurationUI : ExtractionConfigurationUI_Design, ISaveableUI
    {
        private ExtractionConfiguration _extractionConfiguration;
        private PipelineSelectionUI<DataTable> _extractionPipelineSelectionUI;

        private PipelineSelectionUI<DataTable> _cohortRefreshingPipelineSelectionUI;

        public ExtractionConfigurationUI()
        {
            InitializeComponent();
            
            tcRequest.Title = "Request Ticket";
            tcRequest.TicketTextChanged += tcRequest_TicketTextChanged;

            tcRelease.Title = "Release Ticket";
            tcRelease.TicketTextChanged += tcRelease_TicketTextChanged;

            cbxCohortIdentificationConfiguration.PropertySelector = sel => sel.Cast<CohortIdentificationConfiguration>().Select(cic=> cic == null? "<<None>>":cic.Name);
        }
        
        public ExtractionConfiguration ExtractionConfiguration
        {
            get { return _extractionConfiguration; }
            set
            {
                _extractionConfiguration = value;

                tbUsername.Text = ExtractionConfiguration.Username ?? "";
                tbCreated.Text = ExtractionConfiguration.dtCreated.ToString();
                tcRelease.TicketText = ExtractionConfiguration.ReleaseTicket;
                tcRequest.TicketText = ExtractionConfiguration.RequestTicket;
                tbID.Text = ExtractionConfiguration.ID.ToString();
                tbDescription.Text = ExtractionConfiguration.Description;
            }
        }
        
        void tcRequest_TicketTextChanged(object sender, EventArgs e)
        {
            if (ExtractionConfiguration == null)
                return;

            //don't change if it is already that
            if (ExtractionConfiguration.RequestTicket != null && ExtractionConfiguration.RequestTicket.Equals(tcRequest.TicketText))
                return;
            
            ExtractionConfiguration.RequestTicket = tcRequest.TicketText;

            ExtractionConfiguration.SaveToDatabase();
        }

        void tcRelease_TicketTextChanged(object sender, EventArgs e)
        {
            if (ExtractionConfiguration == null)
                return;

            //don't change if it is already that
            if (ExtractionConfiguration.ReleaseTicket != null && ExtractionConfiguration.ReleaseTicket.Equals(tcRelease.TicketText))
                return;

            ExtractionConfiguration.ReleaseTicket = tcRelease.TicketText;
            ExtractionConfiguration.SaveToDatabase();
        }


        private void btnConfigureGlobalParameters_Click(object sender, EventArgs e)
        {
            var parameterManager = new ParameterManager();


            foreach (var extractableDataSet in ExtractionConfiguration.GetAllExtractableDataSets())
            {
                var rootFilterContainer = ExtractionConfiguration.GetFilterContainerFor(extractableDataSet);
                var allFilters = SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(rootFilterContainer).ToList();
                parameterManager.AddParametersFor(allFilters);//query level
            }

            foreach (var p in ExtractionConfiguration.GlobalExtractionFilterParameters)
                parameterManager.AddGlobalParameter(p);

            ParameterCollectionUI.ShowAsDialog(new ParameterCollectionUIOptions(ConfigureExtractionConfigurationGlobalParametersUseCase, ExtractionConfiguration, ParameterLevel.Global, parameterManager),true);

        }

        private void ExtractionConfigurationUI_Load(object sender, EventArgs e)
        {

        }

        private void tcRequest_Load(object sender, EventArgs e)
        {

        }

        private const string ConfigureExtractionConfigurationGlobalParametersUseCase
            = @"You are trying to perform a data extraction of one or more datasets against a cohort.  It is likely that your datasets contain filters (e.g. 'only records from Tayside').  These filters may contain duplicate parameters (e.g. if you have 5 datasets each filtered by healthboard each with a parameter called @healthboard).  This dialog lets you configure a single 'overriding' master copy at the ExtractionConfiguration level which will allow you to change all copies at once in one place.  You will also see two global parameters the system generates automatically when doing extractions these are @CohortDefinitionID and @ProjectNumber";

        private bool _bLoading = false;
        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _bLoading = true;
            ExtractionConfiguration = databaseObject;

            SetupPipelineSelection();

            SetupCohortIdentificationConfiguration();

            SetPreviewObjectsForCohortRefresh();

            pbCic.Image = activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration,OverlayKind.Link);
            
            objectSaverButton1.SetupFor(databaseObject,activator.RefreshBus);
            
            _bLoading = false;
            
        }

        private void SetPreviewObjectsForCohortRefresh()
        {
            _cohortRefreshingPipelineSelectionUI.InitializationObjectsForPreviewPipeline.Clear();
            _cohortRefreshingPipelineSelectionUI.InitializationObjectsForPreviewPipeline.Add(CohortCreationRequest.Empty);
            _cohortRefreshingPipelineSelectionUI.InitializationObjectsForPreviewPipeline.Add(ExtractionConfiguration.CohortIdentificationConfiguration);
        }


        private void SetupCohortIdentificationConfiguration()
        {
            cbxCohortIdentificationConfiguration.DataSource = _activator.CoreChildProvider.AllCohortIdentificationConfigurations;
            cbxCohortIdentificationConfiguration.SelectedItem = ExtractionConfiguration.CohortIdentificationConfiguration;
        }

        private void SetupPipelineSelection()
        {
            //already set i tup
            if (_extractionPipelineSelectionUI != null)
                return;

            _extractionPipelineSelectionUI = new PipelineSelectionUI<DataTable>(null, null, _activator.RepositoryLocator.CatalogueRepository);
            _extractionPipelineSelectionUI.Context = ExtractionPipelineHost.Context;
            _extractionPipelineSelectionUI.Dock = DockStyle.Fill;
            _extractionPipelineSelectionUI.PipelineChanged += ExtractionPipelineSelectionUIOnPipelineChanged;
            pChooseExtractionPipeline.Controls.Add(_extractionPipelineSelectionUI);

            _extractionPipelineSelectionUI.InitializationObjectsForPreviewPipeline.Add(ExtractDatasetCommand.EmptyCommand);
            _extractionPipelineSelectionUI.InitializationObjectsForPreviewPipeline.Add(DataLoadInfo.Empty);
            try
            {
                _extractionPipelineSelectionUI.Pipeline = ExtractionConfiguration.DefaultPipeline;
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
            _extractionPipelineSelectionUI.Text = "Extraction Pipeline";

            
            _cohortRefreshingPipelineSelectionUI = new PipelineSelectionUI<DataTable>(null, null, _activator.RepositoryLocator.CatalogueRepository);
            _cohortRefreshingPipelineSelectionUI.Context = CohortCreationRequest.Context;
            _cohortRefreshingPipelineSelectionUI.Dock = DockStyle.Fill;
            _cohortRefreshingPipelineSelectionUI.PipelineChanged += CohortRefreshingPipelineSelectionUIOnPipelineChanged;
            pChooseCohortRefreshPipeline.Controls.Add(_cohortRefreshingPipelineSelectionUI);

            try
            {
                _cohortRefreshingPipelineSelectionUI.Pipeline = ExtractionConfiguration.CohortRefreshPipeline;
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
            _cohortRefreshingPipelineSelectionUI.Text = "Cohort Refresh Pipeline";
        }

        private void CohortRefreshingPipelineSelectionUIOnPipelineChanged(object sender, EventArgs eventArgs)
        {
            var newValue = _cohortRefreshingPipelineSelectionUI.Pipeline != null ? _cohortRefreshingPipelineSelectionUI.Pipeline.ID : (int?)null;

            if (newValue == ExtractionConfiguration.CohortRefreshPipeline_ID)
                return;

            ExtractionConfiguration.CohortRefreshPipeline_ID = newValue;
        }

        private void ExtractionPipelineSelectionUIOnPipelineChanged(object sender, EventArgs e)
        {
            var newValue = _extractionPipelineSelectionUI.Pipeline != null ? _extractionPipelineSelectionUI.Pipeline.ID : (int?)null;

            if (newValue == ExtractionConfiguration.DefaultPipeline_ID)
                return;

            ExtractionConfiguration.DefaultPipeline_ID = newValue;


        }

        public ObjectSaverButton GetObjectSaverButton()
        {
            return objectSaverButton1;
        }

        private void cbxCohortIdentificationConfiguration_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(_bLoading)
                return;

            var cic = cbxCohortIdentificationConfiguration.SelectedItem as CohortIdentificationConfiguration;

            if (cic == null)
                ExtractionConfiguration.CohortIdentificationConfiguration_ID = null;
            else
                ExtractionConfiguration.CohortIdentificationConfiguration_ID = cic.ID;
        }

        private void btnClearCic_Click(object sender, EventArgs e)
        {
            cbxCohortIdentificationConfiguration.SelectedItem = null;
        }

        private void tbDescription_TextChanged(object sender, EventArgs e)
        {
            ExtractionConfiguration.Description = tbDescription.Text;
        }
    }

    [TypeDescriptionProvider(typeof(AbstractControlDescriptionProvider<ExtractionConfigurationUI_Design, UserControl>))]
    public abstract class ExtractionConfigurationUI_Design : RDMPSingleDatabaseObjectControl<ExtractionConfiguration>
    {
    }
}

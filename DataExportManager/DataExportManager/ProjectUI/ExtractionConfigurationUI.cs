using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.QueryBuilding.Parameters;
using CatalogueLibrary.Repositories;
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
using RDMPObjectVisualisation.Pipelines.PluginPipelineUsers;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

using ReusableUIComponents.ChecksUI;
using ReusableUIComponents.Icons.IconProvision;
using ReusableUIComponents.TransparentHelpSystem;

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
        private IPipelineSelectionUI _extractionPipelineSelectionUI;

        private IPipelineSelectionUI _cohortRefreshingPipelineSelectionUI;

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


        private void ExtractionConfigurationUI_Load(object sender, EventArgs e)
        {

        }

        private void tcRequest_Load(object sender, EventArgs e)
        {

        }
        
        private bool _bLoading = false;
        
        public override void SetDatabaseObject(IActivateItems activator, ExtractionConfiguration databaseObject)
        {
            base.SetDatabaseObject(activator,databaseObject);
            _bLoading = true;
            ExtractionConfiguration = databaseObject;

            SetupCohortIdentificationConfiguration();

            SetupPipelineSelectionExtraction();
            SetupPipelineSelectionCohortRefresh();
            
            pbCic.Image = activator.CoreIconProvider.GetImage(RDMPConcept.CohortIdentificationConfiguration,OverlayKind.Link);
            
            objectSaverButton1.SetupFor(databaseObject,activator.RefreshBus);
            
            _bLoading = false;
            
        }
        
        private void SetupCohortIdentificationConfiguration()
        {
            cbxCohortIdentificationConfiguration.DataSource = _activator.CoreChildProvider.AllCohortIdentificationConfigurations;
            cbxCohortIdentificationConfiguration.SelectedItem = ExtractionConfiguration.CohortIdentificationConfiguration;
        }

        private void SetupPipelineSelectionCohortRefresh()
        {
            ragSmiley1Refresh.Reset();

            if (_cohortRefreshingPipelineSelectionUI != null)
                return;
            try
            {
                //the use case is extracting a dataset
                var useCase = new CohortCreationRequest(ExtractionConfiguration);

                //the user is DefaultPipeline_ID field of ExtractionConfiguration
                var user = new PipelineUser(typeof(ExtractionConfiguration).GetProperty("CohortRefreshPipeline_ID"), ExtractionConfiguration);

                //create the UI for this situation
                var factory = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, user, useCase);
                _cohortRefreshingPipelineSelectionUI = factory.Create("Cohort Refresh Pipeline", DockStyle.Fill,pChooseCohortRefreshPipeline);
                _cohortRefreshingPipelineSelectionUI.Pipeline = ExtractionConfiguration.CohortRefreshPipeline;
                _cohortRefreshingPipelineSelectionUI.PipelineChanged += _cohortRefreshingPipelineSelectionUI_PipelineChanged;
                _cohortRefreshingPipelineSelectionUI.CollapseToSingleLineMode();
            }
            catch (Exception e)
            {
                ragSmiley1Refresh.Fatal(e);
            }
        }

        void _cohortRefreshingPipelineSelectionUI_PipelineChanged(object sender, EventArgs e)
        {
            ragSmiley1Refresh.Reset();
            try
            {
                new CohortCreationRequest(ExtractionConfiguration).GetEngine(_cohortRefreshingPipelineSelectionUI.Pipeline,new ThrowImmediatelyDataLoadEventListener());
            }
            catch (Exception ex)
            {
                ragSmiley1Refresh.Fatal(ex);
            }
        }

        private void SetupPipelineSelectionExtraction()
        {
            //already set i tup
            if (_extractionPipelineSelectionUI != null)
                return;

            //the use case is extracting a dataset
            var useCase = new ExtractionPipelineHost();

            //the user is DefaultPipeline_ID field of ExtractionConfiguration
            var user = new PipelineUser(typeof(ExtractionConfiguration).GetProperty("DefaultPipeline_ID"),ExtractionConfiguration);

            //create the UI for this situation
            var factory = new PipelineSelectionUIFactory(_activator.RepositoryLocator.CatalogueRepository, user, useCase);
            _extractionPipelineSelectionUI = factory.Create("Extraction Pipeline",DockStyle.Fill,pChooseExtractionPipeline);
            _extractionPipelineSelectionUI.CollapseToSingleLineMode();
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

            SetupPipelineSelectionCohortRefresh();
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

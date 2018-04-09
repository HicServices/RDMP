using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Events;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportManager.ProjectUI;
using DataExportLibrary;
using DataExportLibrary.CohortCreationPipeline.Destinations;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using HIC.Logging;
using HIC.Logging.Listeners;

using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

using DataTable = System.Data.DataTable;
using Point = System.Drawing.Point;

namespace DataExportManager.CohortUI.ImportCustomData
{
    /// <summary>
    /// Wraps ConfigureAndExecutePipeline.  Lets you upload a flat file as custom data for a cohort.  Custom data is a project specific dataset for use with only one the specific cohort, 
    /// for example if a researcher had collected questionnaire data about 1000 participants in his cohort and wanted it supplied in anonymous form along with his project extract.  
    /// 
    /// <para>The custom table must contain the private patient identifier column (or will must by the end of the pipeline execution).  The custom table will be maintained in the Cohort 
    /// database and anonymised during extractions.  The columns of the table will also be available for use as addendums to existing datasets (e.g. if your custom data has a field 
    /// 'DateConsentedToStudy' then you could add this on the end of all Prescription dataset rows as a new column).</para>
    /// 
    /// <para>You must select an appropriate pipeline or create a new one.  The pipeline has a fixed destination which you cannot change (CustomCohortDataDestination) but you can put in any 
    /// 'Middle' components that change the DataTable during execution (e.g. to replace a third party identifier with your own patient identifiers).  You must also select an appropriate
    /// source pipeline component for your file type.  When building pipelines remember that they should be as reusable as possible and have a lifetime that should last forever so make 
    /// sure to describe what it does properly and try to reduce any implementation specific logic (e.g. explicitly named columns that relate to this one time load).</para>
    /// 
    /// </summary>
    public partial class ImportCustomDataFileUI : RDMPUserControl
    {
        public DataFlowPipelineContext<DataTable> _context;

        private readonly IActivateItems _activator;
        private readonly ExtractableCohort _cohort;

        private CustomCohortDataDestination _destination;
        private IExternalDatabaseServer _loggingServer;

        public ImportCustomDataFileUI(IActivateItems activator, ExtractableCohort cohort, params object[] initializationObjects)
        {
            _context = new DataFlowPipelineContextFactory<DataTable>().Create(
                PipelineUsage.FixedDestination |
                PipelineUsage.LogsToTableLoadInfo |
                PipelineUsage.LoadsSingleTableInfo);
            
            _context.MustHaveSource = typeof (IDataFlowSource<DataTable>);

            InitializeComponent();


            if (VisualStudioDesignMode || initializationObjects == null)
                return;

            _activator = activator;
            _cohort = cohort;
            
            _destination = new CustomCohortDataDestination();

            var cataRepository = ((DataExportRepository) cohort.Repository).CatalogueRepository;

            //tell the hosted control about what we are trying to do
            configureAndExecutePipeline1.AddInitializationObject(_cohort);

            foreach (object initializationObject in initializationObjects)
                configureAndExecutePipeline1.AddInitializationObject(initializationObject);
            
            configureAndExecutePipeline1.SetPipelineOptions(null, _destination, _context, cataRepository);
            configureAndExecutePipeline1.PipelineExecutionFinishedsuccessfully += configureAndExecutePipeline1_PipelineExecutionFinishedsuccessfully;
            configureAndExecutePipeline1.PipelineExecutionStarted += configureAndExecutePipeline1_PipelineExecutionStarted;
            var defaults = new ServerDefaults(cataRepository);
            _loggingServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID);

        }

        private void configureAndExecutePipeline1_PipelineExecutionStarted(object sender, PipelineEngineEventArgs args)
        {
            //only create the logging audit record once the user hits the go button (and again if he keeps hammering go again)
            if (_loggingServer != null)
            {
                var logManager = new LogManager(_loggingServer);

                logManager.CreateNewLoggingTaskIfNotExists(ExtractableCohort.CohortLoggingTask);
                var listener = new ToLoggingDatabaseDataLoadEventListener(this, logManager, ExtractableCohort.CohortLoggingTask, "Custom Data For Cohort " + _cohort);
                configureAndExecutePipeline1.SetAdditionalProgressListener(listener);
            }
        }

        void configureAndExecutePipeline1_PipelineExecutionFinishedsuccessfully(object sender, CatalogueLibrary.DataFlowPipeline.Events.PipelineEngineEventArgs args)
        {
            MessageBox.Show("Pipeline reports it has successfully completed, inspect Progress tab for any warnings and confirm you are happy with the number of records loaded then close this form");

            tryAgain:

            TypeTextOrCancelDialog dialog = new TypeTextOrCancelDialog("Describe file imported","Describe the file you just imported",9000);

            if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.ResultText))
            {
                _cohort.AppendToAuditLog("Custom File Imported" + Environment.NewLine + dialog.ResultText);
                _cohort.CreateCustomColumnsIfCustomTableExists();
                _activator.RefreshBus.Publish(this,new RefreshObjectEventArgs(_cohort));
            }
            else
            {
                MessageBox.Show(
                    "This is not optional, you must provide a description of the file you just joined to your cohort");
                goto tryAgain;
            }
        }

        
    }
}

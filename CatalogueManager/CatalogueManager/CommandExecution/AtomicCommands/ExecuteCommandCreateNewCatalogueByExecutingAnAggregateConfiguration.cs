using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.CommandExecution.AtomicCommands;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.DataHelper;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.SimpleDialogs.ForwardEngineering;
using DataExportLibrary.CohortCreationPipeline.Sources;
using DataExportLibrary.Data.DataTables;
using DataLoadEngine.DataFlowPipeline.Destinations;
using RDMPObjectVisualisation.Pipelines;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableLibraryCode.Progress;
using ReusableUIComponents;

namespace CatalogueManager.CommandExecution.AtomicCommands
{
    public class ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration : BasicUICommandExecution,IAtomicCommandWithTarget
    {
        private AggregateConfiguration _aggregateConfiguration;
        private ExtractableCohort _cohort;
        private DiscoveredTable _table;
        private Project _projectSpecific;

        public ExecuteCommandCreateNewCatalogueByExecutingAnAggregateConfiguration(IActivateItems activator) : base(activator)
        {
            
        }

        public override string GetCommandHelp()
        {
            return "Executes an existing cohort set, patient index table or graph and stores the results in a new table (which is imported as a new dataset)";
        }

        public override void Execute()
        {
            base.Execute();

            if (_aggregateConfiguration == null)
                _aggregateConfiguration = SelectOne<AggregateConfiguration>(Activator.RepositoryLocator.CatalogueRepository);

            if(_aggregateConfiguration == null)
                return;

            IDataFlowSource<DataTable> fixedSource = null;

            if (_aggregateConfiguration.IsJoinablePatientIndexTable())
            {
                var dr = MessageBox.Show("Would you like to constrain the records to only those in a committed cohort?","Cohort Records Only", MessageBoxButtons.YesNoCancel);

                if(dr == DialogResult.Cancel)
                    return;

                if (dr == DialogResult.Yes)
                {
                    _cohort = SelectOne<ExtractableCohort>(Activator.RepositoryLocator.DataExportRepository);
                    
                    if(_cohort == null)
                        return;
                }

                if(_cohort != null)
                {
                    var src = new PatientIndexTableSource();
                    src.PreInitialize(_aggregateConfiguration, new ThrowImmediatelyDataLoadEventListener());
                    src.PreInitialize(_cohort, new ThrowImmediatelyDataLoadEventListener());
                    fixedSource = src;

                    var externalData = _cohort.GetExternalData();
                    if(externalData != null)
                    {
                        var projNumber = externalData.ExternalProjectNumber;
                        var projs = Activator.RepositoryLocator.DataExportRepository.GetAllObjects<Project>().Where(p=>p.ProjectNumber == projNumber).ToArray();
                        if (projs.Length == 1)
                            _projectSpecific = projs[0];
                    }
                }
            }

            if(fixedSource == null)
            {
                var src = new AggregateConfigurationTableSource();
                src.PreInitialize(_aggregateConfiguration,new ThrowImmediatelyDataLoadEventListener());
                fixedSource = src;
            }

            _table = SelectTable(true,"Choose destination table name");

            if (_table == null)
                return;

            ((AggregateConfigurationTableSource)fixedSource).TableName = _table.GetRuntimeName();
            
            var ui = new ConfigureAndExecutePipeline();
            ui.AddInitializationObject(_aggregateConfiguration);
            if(_cohort != null)
                ui.AddInitializationObject(_cohort);


            ui.AddInitializationObject(_table.Database);

            var contextFactory = new DataFlowPipelineContextFactory<DataTable>();
            var context = contextFactory.Create(PipelineUsage.FixedSource);
            context.MustHaveDestination = typeof (DataTableUploadDestination);

            ui.SetPipelineOptions(fixedSource,null,context,Activator.RepositoryLocator.CatalogueRepository);

            ui.PipelineExecutionFinishedsuccessfully += ui_PipelineExecutionFinishedsuccessfully;

            Activator.ShowWindow(ui, true);
        }

        void ui_PipelineExecutionFinishedsuccessfully(object sender, CatalogueLibrary.DataFlowPipeline.Events.PipelineEngineEventArgs args)
        {
            if(!_table.Exists())
                throw new Exception("Pipeline execute succesfully but the expected table '" + _table +"' did not exist");
            
            var importer = new TableInfoImporter(Activator.RepositoryLocator.CatalogueRepository, _table);
            
            var createCatalogue = new ConfigureCatalogueExtractabilityUI(Activator, importer, "Execution of '" + _aggregateConfiguration + "' (AggregateConfiguration ID =" + _aggregateConfiguration.ID+")",_projectSpecific);
            createCatalogue.ShowDialog();
        }


        public Image GetImage(IIconProvider iconProvider)
        {
            return iconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Execute);
        }

        public IAtomicCommandWithTarget SetTarget(DatabaseEntity target)
        {
            var configuration = target as AggregateConfiguration;
            if (configuration != null)
                _aggregateConfiguration = configuration;

            var cohort = target as ExtractableCohort;
            if (cohort != null)
                _cohort = cohort;

            var project = target as Project;
            if (project != null)
                _projectSpecific = project;

            return this;
        }
    }
}
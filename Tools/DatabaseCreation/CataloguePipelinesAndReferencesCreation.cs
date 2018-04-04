using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using DataExportLibrary.CohortCreationPipeline.Destinations;
using DataExportLibrary.CohortCreationPipeline.Sources;
using DataExportLibrary.DataRelease.ReleasePipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataLoadEngine.DataFlowPipeline.Destinations;
using LoadModules.Generic.DataFlowSources;
using RDMPStartup;
using ReusableLibraryCode.Checks;

namespace DatabaseCreation
{
    /// <summary>
    /// Creates default pipelines required for basic functionality in RDMP.  These are templates that work but can be expanded upon / modified by the user.  For
    /// example the user might want to add a ColumnBlacklister to the default export pipeline to prevent sensitive fields being extracted etc.
    /// 
    /// </summary>
    public class CataloguePipelinesAndReferencesCreation
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;
        private readonly SqlConnectionStringBuilder _logging;
        private readonly SqlConnectionStringBuilder _dqe;
        private ExternalDatabaseServer _edsLogging;

        public CataloguePipelinesAndReferencesCreation(IRDMPPlatformRepositoryServiceLocator repositoryLocator, SqlConnectionStringBuilder logging, SqlConnectionStringBuilder dqe)
        {
            _repositoryLocator = repositoryLocator;
            _logging = logging;
            _dqe = dqe;
        }

        private void DoStartup()
        {
            var startup = new RDMPStartup.Startup(_repositoryLocator);
            startup.DoStartup(new IgnoreAllErrorsCheckNotifier());
        }
        private void CreateServers()
        {
            var defaults = new ServerDefaults(_repositoryLocator.CatalogueRepository);

            _edsLogging = new ExternalDatabaseServer(_repositoryLocator.CatalogueRepository, "Logging", typeof(HIC.Logging.Database.Class1).Assembly);

            _edsLogging.Server = _logging.DataSource;
            _edsLogging.Database = _logging.InitialCatalog;
            _edsLogging.SaveToDatabase();
            defaults.SetDefault(ServerDefaults.PermissableDefaults.LiveLoggingServer_ID, _edsLogging);
            Console.WriteLine("Successfully configured default logging server");

            var edsDQE = new ExternalDatabaseServer(_repositoryLocator.CatalogueRepository, "DQE", typeof(DataQualityEngine.Database.Class1).Assembly);
            edsDQE.Server = _dqe.DataSource;
            edsDQE.Database = _dqe.InitialCatalog;
            edsDQE.SaveToDatabase();
            defaults.SetDefault(ServerDefaults.PermissableDefaults.DQE, edsDQE);
            Console.WriteLine("Successfully configured default dqe server");

            var edsRAW = new ExternalDatabaseServer(_repositoryLocator.CatalogueRepository, "RAW Server", null);
            edsRAW.Server = _dqe.DataSource;
            edsRAW.SaveToDatabase();
            defaults.SetDefault(ServerDefaults.PermissableDefaults.RAWDataLoadServer, edsRAW);
            Console.WriteLine("Successfully configured RAW server");
        }

        private void CreatePipelines()
        {
            var bulkInsertCsvPipe = CreatePipeline("BULK INSERT:CSV Import File", typeof(DelimitedFlatFileDataFlowSource), typeof(DataTableUploadDestination));
            CreatePipeline("BULK INSERT:Excel File", typeof(ExcelDataFlowSource), typeof(DataTableUploadDestination));

            SetCSVSourceDelimiterToComma(bulkInsertCsvPipe);

            var d = (PipelineComponentArgument)bulkInsertCsvPipe.Destination.GetAllArguments().Single(a => a.Name.Equals("LoggingServer"));
            d.SetValue(_edsLogging);
            d.SaveToDatabase();

            var createCohortFromCSV = CreatePipeline("CREATE COHORT:From CSV File",typeof (DelimitedFlatFileDataFlowSource), typeof (BasicCohortDestination));
            SetCSVSourceDelimiterToComma(createCohortFromCSV);

            CreatePipeline("CREATE COHORT:By Executing Cohort Identification Configuration",typeof (CohortIdentificationConfigurationSource), typeof (BasicCohortDestination));

            var customDataCsvPipe = CreatePipeline("IMPORT COHORT CUSTOM DATA:From CSV File", typeof (DelimitedFlatFileDataFlowSource), null);
            SetCSVSourceDelimiterToComma(customDataCsvPipe);

            CreatePipeline("IMPORT COHORT CUSTOM DATA: From PatientIndexTable", typeof (PatientIndexTableSource), null);

            CreatePipeline("DATA EXPORT:To CSV", typeof (ExecuteDatasetExtractionSource), typeof (ExecuteDatasetExtractionFlatFileDestination));

            CreatePipeline("RELEASE PROJECT:To Directory", null, typeof (BasicDataReleaseDestination));
        }

        private void SetCSVSourceDelimiterToComma(Pipeline pipe)
        {
            var s = (PipelineComponentArgument)pipe.Source.GetAllArguments().Single(a => a.Name.Equals("Separator"));
            s.SetValue(",");
            s.SaveToDatabase();
        }

        private Pipeline CreatePipeline(string nameOfPipe, Type sourceType, Type destinationTypeIfAny, params Type[] componentTypes)
        {
            if (componentTypes == null || componentTypes.Length == 0)
                return CreatePipeline(nameOfPipe, sourceType, destinationTypeIfAny);

            var pipeline = CreatePipeline(nameOfPipe, sourceType, destinationTypeIfAny);

            int i = 1;
            foreach (var componentType in componentTypes)
            {
                var component = new PipelineComponent(_repositoryLocator.CatalogueRepository, pipeline, componentType, i++);
                component.CreateArgumentsForClassIfNotExists(componentType);
                component.Pipeline_ID = pipeline.ID;
            }

            return pipeline;
        }

        private Pipeline CreatePipeline(string nameOfPipe, Type sourceType, Type destinationTypeIfAny)
        {
            var pipe = new Pipeline(_repositoryLocator.CatalogueRepository, nameOfPipe);

            if (sourceType != null)
            {
                var source = new PipelineComponent(_repositoryLocator.CatalogueRepository, pipe, sourceType, 0);
                source.CreateArgumentsForClassIfNotExists(sourceType);
                pipe.SourcePipelineComponent_ID = source.ID;    
            }
            
            if (destinationTypeIfAny != null)
            {
                var destination = new PipelineComponent(_repositoryLocator.CatalogueRepository, pipe,destinationTypeIfAny, 100);
                destination.CreateArgumentsForClassIfNotExists(destinationTypeIfAny);
                pipe.DestinationPipelineComponent_ID = destination.ID;
            }

            pipe.SaveToDatabase();

            return pipe;
        }

        public void Create()
        {

            DoStartup();
            CreateServers();
            CreatePipelines();
        }
    }
}

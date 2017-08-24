using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataHelper;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Data.LinkCreators;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.ExtractionTime.Commands;
using DataExportLibrary.ExtractionTime.ExtractionPipeline;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Destinations;
using DataExportLibrary.ExtractionTime.ExtractionPipeline.Sources;
using DataExportLibrary.ExtractionTime.UserPicks;
using HIC.Logging;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataExportLibrary.Tests.DataExtraction
{
    public class TestsRequiringAnExtractionConfiguration : TestsRequiringACohort
    {
        protected Catalogue _catalogue;
        protected TableInfo _tableInfo;
        protected ExtractableDataSet _extractableDataSet;
        protected Project _project;
        protected ExtractionConfiguration _configuration;
        protected ExtractionInformation[] _extractionInformations;
        protected List<IColumn> _extractableColumns = new List<IColumn>();
        protected ExtractDatasetCommand _request;

        private readonly string _testDatabaseName = TestDatabaseNames.GetConsistentName("ExtractionConfiguration");

        protected bool AllowEmptyExtractions = false;
        protected SelectedDataSets _selectedDataSet;

        [TestFixtureSetUp]
        protected override void SetUp()
        {
            base.SetUp();

            SetupCatalogueConfigurationEtc();

            SetupDataExport();

            _configuration.Cohort_ID = _extractableCohort.ID;
            _configuration.SaveToDatabase();


            _request = new ExtractDatasetCommand(RepositoryLocator,_configuration, _extractableCohort, new ExtractableDatasetBundle(_extractableDataSet),
                _extractableColumns, new HICProjectSalt(_project), "",
                new ExtractionDirectory(@"C:\temp\", _configuration));
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            //delete all columns
            foreach (var selectedCol in _extractableColumns)
                ((ExtractableColumn)selectedCol).DeleteInDatabase();

            //unselect the dataset
            _configuration.SelectedDataSets.Single().DeleteInDatabase();

            _configuration.DeleteInDatabase();
            _extractableDataSet.DeleteInDatabase();
            _project.DeleteInDatabase();
         
            _catalogue.DeleteInDatabase();
            var credentials = _tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            _tableInfo.DeleteInDatabase();

            if(credentials != null)
                credentials.DeleteInDatabase();
        }

        private void SetupDataExport()
        {
            _extractableDataSet = new ExtractableDataSet(DataExportRepository, _catalogue);

            _project = new Project(DataExportRepository, _testDatabaseName);
            _project.ProjectNumber = 1;

            Directory.CreateDirectory(@"C:\temp\");
            _project.ExtractionDirectory = @"C:\temp\";
            
            _project.SaveToDatabase();

            _configuration = new ExtractionConfiguration(DataExportRepository, _project);
            
            //select the dataset for extraction under this configuration
            _selectedDataSet = new SelectedDataSets(RepositoryLocator.DataExportRepository,_configuration,_extractableDataSet,null);

            //select all the columns for extraction
            foreach (var toSelect in _extractionInformations)
            {
                var col = new ExtractableColumn(DataExportRepository, _extractableDataSet, _configuration, toSelect, toSelect.Order, toSelect.SelectSQL);

                if (col.GetRuntimeName().Equals("PrivateID"))
                    col.IsExtractionIdentifier = true;

                col.SaveToDatabase();
                
                _extractableColumns.Add(col);
                
            }
        }

        private void SetupCatalogueConfigurationEtc()
        {
            SqlConnection con = new SqlConnection(DatabaseICanCreateRandomTablesIn.ConnectionString);
            con.Open();

            SqlCommand cmdCreateTestTable = new SqlCommand(@"
if exists (select 1 from sys.tables where Name = 'TestTable')
begin
drop table TestTable
end
CREATE TABLE TestTable (PrivateID varchar(10),Result int )", con);
            cmdCreateTestTable.ExecuteNonQuery();

            SqlCommand cmdInsert = new SqlCommand("INSERT INTO TestTable VALUES('"+_cohortKeysGenerated.Keys.First()+"',1);",con);
            cmdInsert.ExecuteNonQuery();
            
            con.Close();

            TableInfoImporter importer = new TableInfoImporter(RepositoryLocator.CatalogueRepository, DatabaseICanCreateRandomTablesIn.DataSource,DatabaseICanCreateRandomTablesIn.InitialCatalog, "TestTable",DatabaseType.MicrosoftSQLServer,DatabaseICanCreateRandomTablesIn.UserID,DatabaseICanCreateRandomTablesIn.Password);

            TableInfo t;
            ColumnInfo[] cs;
            importer.DoImport(out t, out cs);

            var forwardEngineer = new ForwardEngineerCatalogue(t,cs,true);
            _tableInfo = t;

            Catalogue catalogue;
            CatalogueItem[] cataItems;
            ExtractionInformation[] extractionInformations;

            forwardEngineer.ExecuteForwardEngineering(out catalogue,out cataItems,out extractionInformations);
            _extractionInformations = extractionInformations;

            ExtractionInformation _privateID = extractionInformations.First(e => e.GetRuntimeName().Equals("PrivateID"));
            _privateID.IsExtractionIdentifier = true;
            _privateID.SaveToDatabase();

            _catalogue = catalogue;
        }

        protected void Execute(out ExtractionPipelineHost pipelineHost, out IExecuteDatasetExtractionDestination results)
        {

            DataLoadInfo d = new DataLoadInfo("Internal", _testDatabaseName, "IgnoreMe", "", true, new DiscoveredServer(UnitTestLoggingConnectionString));

            Pipeline pipeline = null;
            
            try
            {
                pipeline = SetupPipeline();
                pipelineHost = new ExtractionPipelineHost(_request,CatalogueRepository.MEF, pipeline, d);

                pipelineHost.Execute(new ThrowImmediatelyEventsListener());

                Assert.IsNotEmpty(pipelineHost.Source.Request.QueryBuilder.SQL);

                Assert.IsFalse(pipelineHost.Crashed);
            }
            finally
            {
                if(pipeline != null)
                    pipeline.DeleteInDatabase();
            }

            results =  pipelineHost.Destination;
        }

        protected virtual Pipeline SetupPipeline()
        {
            var repository = RepositoryLocator.CatalogueRepository;
            var pipeline = new Pipeline(repository, "Empty extraction pipeline");

            var component = new PipelineComponent(repository, pipeline, typeof(ExecuteDatasetExtractionFlatFileDestination), 0, "Destination");
            var arguments = component.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionFlatFileDestination>().ToArray();
            
            if(arguments.Length != 2)
                throw new Exception("Expected only 2 arguments for type ExecuteDatasetExtractionFlatFileDestination, did somebody add another [DemandsInitialization]? if so handle it below");

            arguments.Single(a => a.Name.Equals("DateFormat")).SetValue("yyyy-MM-dd");
            arguments.Single(a => a.Name.Equals("DateFormat")).SaveToDatabase();

            arguments.Single(a => a.Name.Equals("FlatFileType")).SetValue(ExecuteExtractionToFlatFileType.CSV);
            arguments.Single(a => a.Name.Equals("FlatFileType")).SaveToDatabase();


            var component2 = new PipelineComponent(repository, pipeline, typeof(ExecuteDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionSource>().ToArray();

            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(AllowEmptyExtractions);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

                
            //configure the component as the destination
            pipeline.DestinationPipelineComponent_ID = component.ID;
            pipeline.SourcePipelineComponent_ID = component2.ID;
            pipeline.SaveToDatabase();

            return pipeline;
        }
    }


    public class ThrowImmediatelyEventsListener : IDataLoadEventListener
    {
        public void OnNotify(object sender, NotifyEventArgs e)
        {

            if (e.Exception != null)
                throw e.Exception;

            if(e.ProgressEventType == ProgressEventType.Error)
                throw new Exception(e.Message);

        }

        public void OnProgress(object sender, ProgressEventArgs e)
        {
           
        }
    }
}

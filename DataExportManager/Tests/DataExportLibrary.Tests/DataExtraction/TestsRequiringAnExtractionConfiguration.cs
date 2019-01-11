using System;
using System.Collections.Generic;
using System.Data;
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
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using FAnsi.Discovery;
using FAnsi.Discovery.TypeTranslation;
using HIC.Logging;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using Tests.Common;

namespace DataExportLibrary.Tests.DataExtraction
{
    [TestFixture]
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
        protected ColumnInfo[] _columnInfos;

        [OneTimeSetUp]
        protected override void SetUp()
        {
            base.SetUp();

            SetupCatalogueConfigurationEtc();

            SetupDataExport();

            _configuration.Cohort_ID = _extractableCohort.ID;
            _configuration.SaveToDatabase();


            _request = new ExtractDatasetCommand(RepositoryLocator,_configuration, _extractableCohort, new ExtractableDatasetBundle(_extractableDataSet),
                _extractableColumns, new HICProjectSalt(_project),
                new ExtractionDirectory(@"C:\temp\", _configuration));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            RunBlitzDatabases(RepositoryLocator);
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
            DataTable dt = new DataTable();
            dt.Columns.Add("PrivateID");
            dt.Columns.Add("Name");
            dt.Columns.Add("DateOfBirth");

            dt.Rows.Add(new object[] {_cohortKeysGenerated.Keys.First(), "Dave", "2001-01-01"});

            var tbl = DiscoveredDatabaseICanCreateRandomTablesIn.CreateTable("TestTable", dt, new[] { new DatabaseColumnRequest("Name",new DatabaseTypeRequest(typeof(string),50))});
            
            CatalogueItem[] cataItems;
            _catalogue = Import(tbl, out _tableInfo, out _columnInfos, out cataItems,out _extractionInformations);
            
            ExtractionInformation _privateID = _extractionInformations.First(e => e.GetRuntimeName().Equals("PrivateID"));
            _privateID.IsExtractionIdentifier = true;
            _privateID.SaveToDatabase();
            
        }

        protected void Execute(out ExtractionPipelineUseCase pipelineUseCase, out IExecuteDatasetExtractionDestination results)
        {

            DataLoadInfo d = new DataLoadInfo("Internal", _testDatabaseName, "IgnoreMe", "", true, new DiscoveredServer(UnitTestLoggingConnectionString));

            Pipeline pipeline = null;
            
            try
            {
                pipeline = SetupPipeline();
                pipelineUseCase = new ExtractionPipelineUseCase(_request.Configuration.Project, _request, pipeline, d);

                pipelineUseCase.Execute(new ThrowImmediatelyDataLoadEventListener());

                Assert.IsNotEmpty(pipelineUseCase.Source.Request.QueryBuilder.SQL);

                Assert.IsTrue(pipelineUseCase.ExtractCommand.State == ExtractCommandState.Completed);
            }
            finally
            {
                if(pipeline != null)
                    pipeline.DeleteInDatabase();
            }

            results =  pipelineUseCase.Destination;
        }

        protected virtual Pipeline SetupPipeline()
        {
            var repository = RepositoryLocator.CatalogueRepository;
            var pipeline = new Pipeline(repository, "Empty extraction pipeline");

            var component = new PipelineComponent(repository, pipeline, typeof(ExecuteDatasetExtractionFlatFileDestination), 0, "Destination");
            var arguments = component.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionFlatFileDestination>().ToArray();
            
            if(arguments.Length < 3)
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
}

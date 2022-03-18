// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.CohortCommitting;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CohortCommitting.Pipeline.Destinations;
using Rdmp.Core.CohortCommitting.Pipeline.Sources;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.Progress;
using Tests.Common;
using Rdmp.Core.CommandExecution;

namespace Rdmp.Core.Tests.DataExport.TableValuedFunctionTests
{
    public class EndToEndTableValuedFunction:DatabaseTests
    {
        private ExtractionInformation _nonTvfExtractionIdentifier;
        private ICatalogue _nonTvfCatalogue;
        private ITableInfo _nonTvfTableInfo;
        private ExternalCohortTable _externalCohortTable;
        private ICatalogue _tvfCatalogue;
        private ITableInfo _tvfTableInfo;

        //the cohort database
        private DiscoveredDatabase _discoveredCohortDatabase;
        //the data database (with the tvf in it)
        private DiscoveredDatabase _database;

        private CohortIdentificationConfiguration _cic;
        private Project _project;
        private Pipeline _pipe;
        private AggregateConfiguration _aggregate;
        private AggregateConfiguration _cicAggregate;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            _database = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        }

        [Test]
        public void EndToEndTest()
        {
            var cohortDatabaseNameWillBe = TestDatabaseNames.GetConsistentName("TbvCohort");
            _discoveredCohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(cohortDatabaseNameWillBe);

            //cleanup
            if(_discoveredCohortDatabase.Exists())
                _discoveredCohortDatabase.Drop();

            //create a normal catalogue
            CreateANormalCatalogue();
            
            //create a cohort database using wizard
            CreateNewCohortDatabaseWizard cohortDatabaseWizard = new CreateNewCohortDatabaseWizard(_discoveredCohortDatabase,CatalogueRepository,DataExportRepository,false);
            
            _externalCohortTable = cohortDatabaseWizard.CreateDatabase(
                new PrivateIdentifierPrototype(_nonTvfExtractionIdentifier)
                ,new ThrowImmediatelyCheckNotifier());

            //create a table valued function
            CreateTvfCatalogue(cohortDatabaseNameWillBe);
            
            //Test 1 
            TestThatQueryBuilderWithoutParametersBeingSetThrowsQueryBuildingException();

            PopulateCohortDatabaseWithRecordsFromNonTvfCatalogue();
            
            //Test 2 
            TestWithParameterValueThatRowsAreReturned();

            //Test 3 
            TestUsingTvfForAggregates();

            //Test 4 
            TestAddingTvfToCIC();

            //Test 5
            TestDataExportOfTvf();

            //tear down
            DataExportRepository.GetAllObjects<ExtractableCohort>().Single().DeleteInDatabase();
            _externalCohortTable.DeleteInDatabase();

            _database.ExpectTable("NonTVFTable").Drop();
            _database.ExpectTableValuedFunction("GetTopXRandom").Drop();

            //delete global parameter
            ((AnyTableSqlParameter)_aggregate.GetAllParameters().Single()).DeleteInDatabase();
            //delete aggregate
            _aggregate.DeleteInDatabase();

            ((AnyTableSqlParameter)_cicAggregate.GetAllParameters().Single()).DeleteInDatabase();
            //delete aggregate
            _cicAggregate.DeleteInDatabase();

            //get rid of the cohort identification configuration
            _cic.DeleteInDatabase();
            _pipe.DeleteInDatabase();
            
            //get rid of the cohort database
            _discoveredCohortDatabase.Drop();
            
            _nonTvfCatalogue.DeleteInDatabase();
            _nonTvfTableInfo.DeleteInDatabase();

            _tvfCatalogue.DeleteInDatabase();
            _tvfTableInfo.DeleteInDatabase();
        }


        private void PopulateCohortDatabaseWithRecordsFromNonTvfCatalogue()
        {
            //create a cohort identification configuration (identifies people from datasets using set operations - see CohortManager)
            _cic = new CohortIdentificationConfiguration(CatalogueRepository, "TbvfCIC");
            _cic.CreateRootContainerIfNotExists();
            
            //turn the catalogue _nonTvfCatalogue into a cohort set and add it to the root container
            var newAggregate = _cic.CreateNewEmptyConfigurationForCatalogue(_nonTvfCatalogue,(s,e)=> { throw new Exception("Did not expect there to be more than 1!"); });

            var root = _cic.RootCohortAggregateContainer;
            root.AddChild(newAggregate,0);

            //create a pipeline for executing this CIC and turning it into a cohort
            _pipe = new Pipeline(CatalogueRepository, "CREATE COHORT:By Executing CIC");
            
            var source = new PipelineComponent(CatalogueRepository, _pipe,typeof (CohortIdentificationConfigurationSource), 0, "CIC Source");
            
            _project = new Project(DataExportRepository, "TvfProject");
            _project.ProjectNumber = 12;
            _project.ExtractionDirectory = TestContext.CurrentContext.TestDirectory;
            _project.SaveToDatabase();

            var destination = new PipelineComponent(CatalogueRepository, _pipe, typeof(BasicCohortDestination), 1, "Destination");

            _pipe.SourcePipelineComponent_ID = source.ID;
            _pipe.DestinationPipelineComponent_ID = destination.ID;
            _pipe.SaveToDatabase();

            //create pipeline arguments 
            source.CreateArgumentsForClassIfNotExists<CohortIdentificationConfigurationSource>();
            destination.CreateArgumentsForClassIfNotExists<BasicCohortDestination>();

            //create pipeline initialization objects
            var request = new CohortCreationRequest(_project, new CohortDefinition(null, "MyFirstCohortForTvfTest", 1, 12, _externalCohortTable), new ThrowImmediatelyActivator(RepositoryLocator), "Here goes nothing");
            request.CohortIdentificationConfiguration = _cic;
            var engine = request.GetEngine(_pipe,new ThrowImmediatelyDataLoadEventListener());
            engine.ExecutePipeline(new GracefulCancellationToken());
        }

        private void CreateTvfCatalogue(string cohortDatabaseName)
        {
            var svr = _database.Server;
            using (var con = svr.GetConnection())
            {
                con.Open();

                //create the newID view
                svr.GetCommand("create view getNewID as select newid() as new_id", con).ExecuteNonQuery();

                var sql = string.Format(
                @"create function GetTopXRandom (@numberOfRecords int)
RETURNS @retTable TABLE
( 
chi varchar(10),
definitionID int
)
AS
BEGIN

while(@numberOfRecords >0)
begin
insert into @retTable select top 1 chi,cohortDefinition_id from {0}..Cohort order by (select new_id from getNewID)
set @numberOfRecords = @numberOfRecords - 1
end
return
end
",cohortDatabaseName);

                svr.GetCommand(sql, con).ExecuteNonQuery();
            }

            var tblvf = _database.ExpectTableValuedFunction("GetTopXRandom");

            var importer = new TableValuedFunctionImporter(CatalogueRepository, tblvf);
            importer.DoImport(out var tbl,out var cols);

            var engineer = new ForwardEngineerCatalogue(tbl, cols, true);
            engineer.ExecuteForwardEngineering(out var cata, out var cis, out var eis);

            Assert.AreEqual("chi", eis[0].GetRuntimeName());
            eis[0].IsExtractionIdentifier = true;
            eis[0].SaveToDatabase();
            
            _tvfCatalogue = cata;
            _tvfTableInfo = tbl;


        }

        private void CreateANormalCatalogue()
        {
            var svr = _database.Server;
            using (var con = svr.GetConnection())
            {
                con.Open();
                svr.GetCommand("CREATE TABLE NonTVFTable ( chi varchar(10))",con).ExecuteNonQuery();
                svr.GetCommand("INSERT INTO NonTVFTable VALUES ('0101010101')", con).ExecuteNonQuery();
                svr.GetCommand("INSERT INTO NonTVFTable VALUES ('0202020202')", con).ExecuteNonQuery();
                svr.GetCommand("INSERT INTO NonTVFTable VALUES ('0303030303')", con).ExecuteNonQuery();
            }

            var importer = new TableInfoImporter(CatalogueRepository, svr.Name,
                _database.GetRuntimeName(), "NonTVFTable",
                DatabaseType.MicrosoftSQLServer,_database.Server.ExplicitUsernameIfAny,_database.Server.ExplicitPasswordIfAny);

            importer.DoImport(out var tbl,out var cols);

            var engineer = new ForwardEngineerCatalogue(tbl, cols, true);
            engineer.ExecuteForwardEngineering(out var cata,out var cis, out var eis);
            
            _nonTvfExtractionIdentifier  = eis.Single();
            _nonTvfExtractionIdentifier.IsExtractionIdentifier = true;
            _nonTvfExtractionIdentifier.SaveToDatabase();
            
            _nonTvfCatalogue = cata;
            _nonTvfTableInfo = tbl;
        }

        private void TestThatQueryBuilderWithoutParametersBeingSetThrowsQueryBuildingException()
        {
            //we should have problems reading from the table valued function 
            var qb = new QueryBuilder("", "");

            //table valued function should have 2 fields (chi and definitionID)
            Assert.AreEqual(2, _tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Count());

            qb.AddColumnRange(_tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            var ex = Assert.Throws<QueryBuildingException>(() => Console.WriteLine(qb.SQL));
            Assert.AreEqual("No Value defined for Parameter @numberOfRecords", ex.Message);
        }

        private void TestWithParameterValueThatRowsAreReturned()
        {
            var  p = _tvfTableInfo.GetAllParameters().Single();
            p.Value = "5";
            p.SaveToDatabase();

            var qb = new QueryBuilder("", "");
            qb.AddColumnRange(_tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

            var sql = qb.SQL;

            var db = DataAccessPortal.GetInstance().ExpectDatabase(_tvfTableInfo, DataAccessContext.InternalDataProcessing);
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = db.Server.GetCommand(sql, con).ExecuteReader();

                int rowsReturned = 0;

                while (r.Read())
                {
                    rowsReturned++;
                    Assert.NotNull(r["chi"]);
                    Assert.NotNull(r["definitionID"]);
                }

                Assert.AreEqual(rowsReturned,5);
            }
        }

        private void TestUsingTvfForAggregates()
        {


            _aggregate = new AggregateConfiguration(CatalogueRepository, _tvfCatalogue,"tvfAggregate");

            var ei = _tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any).Single(e => !e.IsExtractionIdentifier);
            _aggregate.AddDimension(ei);

            //change the parameter to 10
            var p = _tvfTableInfo.GetAllParameters().Single();
            p.Value = "10";
            p.SaveToDatabase();

            var qb = _aggregate.GetQueryBuilder();
            
            //Query should be something like :
            /*
             * DECLARE @numberOfRecords AS int;
             * SET @numberOfRecords=10;
             * tvfAggregate
             * SELECT 
             * GetTopXRandom.[definitionID],
             * count(*)
             * FROM 
             * [TestDbName_ScratchArea]..GetTopXRandom(@numberOfRecords) AS GetTopXRandom
             * group by 
             * GetTopXRandom.[definitionID]
             * order by 
             * GetTopXRandom.[definitionID] 
             * 
             * --Since we only imported 1 cohort we should have 1 row and the count should be the number we requested 
             * 
             * */

            var sql = qb.SQL;
            
            var db = DataAccessPortal.GetInstance().ExpectDatabase(_tvfTableInfo, DataAccessContext.InternalDataProcessing);
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = db.Server.GetCommand(sql, con).ExecuteReader();

                Assert.IsTrue(r.Read());

                Assert.AreEqual(r[1],10);

                Assert.IsFalse(r.Read());
            }

            //create a global overriding parameter on the aggregate
            var global = new AnyTableSqlParameter(CatalogueRepository, _aggregate, "DECLARE @numberOfRecords AS int;");
            global.Value = "1";
            global.SaveToDatabase();


            //refresh the SQL
            sql = _aggregate.GetQueryBuilder().SQL;

            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = db.Server.GetCommand(sql, con).ExecuteReader();

                Assert.IsTrue(r.Read());

                Assert.AreEqual(r[1], 1);//should now only have 1 record being retrned and counted when executing

                Assert.IsFalse(r.Read());
            }
        }

        private void TestAddingTvfToCIC()
        {
            var root = _cic.RootCohortAggregateContainer;
            root.Operation = SetOperation.EXCEPT;
            root.SaveToDatabase();

            //declare a global parameter of 1 on the aggregate
            _cicAggregate = _cic.ImportAggregateConfigurationAsIdentifierList(_aggregate, (s, e) => { return null; });
            
            //it should have imported the global parameter as part of the import right?
            Assert.AreEqual(1,_cicAggregate.GetAllParameters().Count());

            //add the new cic to the container
            root.AddChild(_cicAggregate,2);

            //So container is:
            // EXCEPT 
                //People in _nonTvfCatalogue (3)
                //People in _tvfCatalogue (with @numberOfRecords = 1) (1)

            //result should be 2
            var qb = new CohortQueryBuilder(_cic,null);

            var sql = qb.SQL;

            var db = DataAccessPortal.GetInstance().ExpectDatabase(_tvfTableInfo, DataAccessContext.InternalDataProcessing);
            using (var con = db.Server.GetConnection())
            {
                con.Open();
                var r = db.Server.GetCommand(sql, con).ExecuteReader();

                //2 chi numbers should be returned
                Assert.IsTrue(r.Read());
                Assert.IsTrue(r.Read());

                Assert.IsFalse(r.Read());
            }
        }

        private void TestDataExportOfTvf()
        {
            var config = new ExtractionConfiguration(DataExportRepository, _project);
            config.Cohort_ID = DataExportRepository.GetAllObjects<ExtractableCohort>().Single().ID;
            config.SaveToDatabase();

            var tvfExtractable = new ExtractableDataSet(DataExportRepository, _tvfCatalogue);

            var selected = new SelectedDataSets(DataExportRepository, config, tvfExtractable, null);

            //make all columns part of the extraction
            foreach (ExtractionInformation e in _tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any))
                config.AddColumnToExtraction(tvfExtractable, e);
            
            //the default value should be 10
            Assert.AreEqual("10",_tvfTableInfo.GetAllParameters().Single().Value);

            //configure an extraction specific global of 1 so that only 1 chi number is fetched (which will be in the cohort)
            var globalP = new GlobalExtractionFilterParameter(DataExportRepository, config, "DECLARE @numberOfRecords AS int;");
            globalP.Value = "1";
            globalP.SaveToDatabase();
            
            var extractionCommand = new ExtractDatasetCommand(config, new ExtractableDatasetBundle(tvfExtractable));

            var source = new ExecuteDatasetExtractionSource();

            source.PreInitialize(extractionCommand, new ThrowImmediatelyDataLoadEventListener());

            var dt = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            
            Assert.AreEqual(1,dt.Rows.Count);

            Assert.AreEqual("ReleaseId",dt.Columns[0].ColumnName);

            //should be a guid
            Assert.IsTrue(dt.Rows[0][0].ToString().Length>10);
            Assert.IsTrue(dt.Rows[0][0].ToString().Contains("-"));

            selected.DeleteInDatabase();
            globalP.DeleteInDatabase();
            config.DeleteInDatabase();

            tvfExtractable.DeleteInDatabase();

        }
    }
}

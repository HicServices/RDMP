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
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataExport.TableValuedFunctionTests;

public class EndToEndTableValuedFunction : DatabaseTests
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

        _database = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
    }

    [Test]
    public void EndToEndTest()
    {
        var cohortDatabaseNameWillBe = TestDatabaseNames.GetConsistentName("TbvCohort");
        _discoveredCohortDatabase =
            DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(cohortDatabaseNameWillBe);

        //cleanup
        if (_discoveredCohortDatabase.Exists())
            _discoveredCohortDatabase.Drop();

        //create a normal catalogue
        CreateANormalCatalogue();

        //create a cohort database using wizard
        var cohortDatabaseWizard = new CreateNewCohortDatabaseWizard(_discoveredCohortDatabase, CatalogueRepository,
            DataExportRepository, false);

        _externalCohortTable = cohortDatabaseWizard.CreateDatabase(
            new PrivateIdentifierPrototype(_nonTvfExtractionIdentifier)
            , ThrowImmediatelyCheckNotifier.Quiet);

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
        var newAggregate = _cic.CreateNewEmptyConfigurationForCatalogue(_nonTvfCatalogue,
            (s, e) => throw new Exception("Did not expect there to be more than 1!"));

        var root = _cic.RootCohortAggregateContainer;
        root.AddChild(newAggregate, 0);

        //create a pipeline for executing this CIC and turning it into a cohort
        _pipe = new Pipeline(CatalogueRepository, "CREATE COHORT:By Executing CIC");

        var source = new PipelineComponent(CatalogueRepository, _pipe, typeof(CohortIdentificationConfigurationSource),
            0, "CIC Source");

        _project = new Project(DataExportRepository, "TvfProject")
        {
            ProjectNumber = 12,
            ExtractionDirectory = TestContext.CurrentContext.TestDirectory
        };
        _project.SaveToDatabase();

        var destination =
            new PipelineComponent(CatalogueRepository, _pipe, typeof(BasicCohortDestination), 1, "Destination");

        _pipe.SourcePipelineComponent_ID = source.ID;
        _pipe.DestinationPipelineComponent_ID = destination.ID;
        _pipe.SaveToDatabase();

        //create pipeline arguments
        source.CreateArgumentsForClassIfNotExists<CohortIdentificationConfigurationSource>();
        destination.CreateArgumentsForClassIfNotExists<BasicCohortDestination>();

        //create pipeline initialization objects
        var request = new CohortCreationRequest(_project,
            new CohortDefinition(null, "MyFirstCohortForTvfTest", 1, 12, _externalCohortTable), DataExportRepository,
            "Here goes nothing")
        {
            CohortIdentificationConfiguration = _cic
        };
        var engine = request.GetEngine(_pipe, ThrowImmediatelyDataLoadEventListener.Quiet);
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

            var sql = $@"create function GetTopXRandom (@numberOfRecords int)
RETURNS @retTable TABLE
( 
chi varchar(10),
definitionID int
)
AS
BEGIN

while(@numberOfRecords >0)
begin
insert into @retTable select top 1 chi,cohortDefinition_id from {cohortDatabaseName}..Cohort order by (select new_id from getNewID)
set @numberOfRecords = @numberOfRecords - 1
end
return
end
";

            svr.GetCommand(sql, con).ExecuteNonQuery();
        }

        var tblvf = _database.ExpectTableValuedFunction("GetTopXRandom");

        var importer = new TableValuedFunctionImporter(CatalogueRepository, tblvf);
        importer.DoImport(out var tbl, out var cols);

        var engineer = new ForwardEngineerCatalogue(tbl, cols);
        engineer.ExecuteForwardEngineering(out var cata, out _, out var eis);

        Assert.That(eis[0].GetRuntimeName(), Is.EqualTo("chi"));
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
            svr.GetCommand("CREATE TABLE NonTVFTable ( chi varchar(10))", con).ExecuteNonQuery();
            svr.GetCommand("INSERT INTO NonTVFTable VALUES ('0101010101')", con).ExecuteNonQuery();
            svr.GetCommand("INSERT INTO NonTVFTable VALUES ('0202020202')", con).ExecuteNonQuery();
            svr.GetCommand("INSERT INTO NonTVFTable VALUES ('0303030303')", con).ExecuteNonQuery();
        }

        var importer = new TableInfoImporter(CatalogueRepository, svr.Name,
            _database.GetRuntimeName(), "NonTVFTable",
            DatabaseType.MicrosoftSQLServer, _database.Server.ExplicitUsernameIfAny,
            _database.Server.ExplicitPasswordIfAny);

        importer.DoImport(out var tbl, out var cols);

        var engineer = new ForwardEngineerCatalogue(tbl, cols);
        engineer.ExecuteForwardEngineering(out var cata, out _, out var eis);

        _nonTvfExtractionIdentifier = eis.Single();
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
        Assert.That(_tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any), Has.Length.EqualTo(2));

        qb.AddColumnRange(_tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

        var ex = Assert.Throws<QueryBuildingException>(() => Console.WriteLine(qb.SQL));
        Assert.That(ex.Message, Is.EqualTo("No Value defined for Parameter @numberOfRecords"));
    }

    private void TestWithParameterValueThatRowsAreReturned()
    {
        var p = _tvfTableInfo.GetAllParameters().Single();
        p.Value = "5";
        p.SaveToDatabase();

        var qb = new QueryBuilder("", "");
        qb.AddColumnRange(_tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any));

        var sql = qb.SQL;

        var db = DataAccessPortal.ExpectDatabase(_tvfTableInfo, DataAccessContext.InternalDataProcessing);
        using var con = db.Server.GetConnection();
        con.Open();
        var r = db.Server.GetCommand(sql, con).ExecuteReader();

        var rowsReturned = 0;

        while (r.Read())
        {
            rowsReturned++;
            Assert.Multiple(() =>
            {
                Assert.That(r["chi"], Is.Not.Null);
                Assert.That(r["definitionID"], Is.Not.Null);
            });
        }

        Assert.That(rowsReturned, Is.EqualTo(5));
    }

    private void TestUsingTvfForAggregates()
    {
        _aggregate = new AggregateConfiguration(CatalogueRepository, _tvfCatalogue, "tvfAggregate");

        var ei = _tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .Single(e => !e.IsExtractionIdentifier);
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

        var db = DataAccessPortal.ExpectDatabase(_tvfTableInfo, DataAccessContext.InternalDataProcessing);
        using (var con = db.Server.GetConnection())
        {
            con.Open();
            var r = db.Server.GetCommand(sql, con).ExecuteReader();

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());

                Assert.That(r[1], Is.EqualTo(10));
            });

            Assert.That(r.Read(), Is.False);
        }

        //create a global overriding parameter on the aggregate
        var global = new AnyTableSqlParameter(CatalogueRepository, _aggregate, "DECLARE @numberOfRecords AS int;")
        {
            Value = "1"
        };
        global.SaveToDatabase();


        //refresh the SQL
        sql = _aggregate.GetQueryBuilder().SQL;

        using (var con = db.Server.GetConnection())
        {
            con.Open();
            var r = db.Server.GetCommand(sql, con).ExecuteReader();

            Assert.Multiple(() =>
            {
                Assert.That(r.Read());

                Assert.That(r[1],
                    Is.EqualTo(1)); //should now only have 1 record being returned and counted when executing
            });

            Assert.That(r.Read(), Is.False);
        }
    }

    private void TestAddingTvfToCIC()
    {
        var root = _cic.RootCohortAggregateContainer;
        root.Operation = SetOperation.EXCEPT;
        root.SaveToDatabase();

        //declare a global parameter of 1 on the aggregate
        _cicAggregate = _cic.ImportAggregateConfigurationAsIdentifierList(_aggregate, (s, e) => null);

        //it should have imported the global parameter as part of the import right?
        Assert.That(_cicAggregate.GetAllParameters(), Has.Length.EqualTo(1));

        //add the new cic to the container
        root.AddChild(_cicAggregate, 2);

        //So container is:
        // EXCEPT
        //People in _nonTvfCatalogue (3)
        //People in _tvfCatalogue (with @numberOfRecords = 1) (1)

        //result should be 2
        var qb = new CohortQueryBuilder(_cic, null);

        var sql = qb.SQL;

        var db = DataAccessPortal.ExpectDatabase(_tvfTableInfo, DataAccessContext.InternalDataProcessing);
        using var con = db.Server.GetConnection();
        con.Open();
        var r = db.Server.GetCommand(sql, con).ExecuteReader();

        //2 chi numbers should be returned
        Assert.That(r.Read());
        Assert.That(r.Read());

        Assert.That(r.Read(), Is.False);
    }

    private void TestDataExportOfTvf()
    {
        var config = new ExtractionConfiguration(DataExportRepository, _project)
        {
            Cohort_ID = DataExportRepository.GetAllObjects<ExtractableCohort>().Single().ID
        };
        config.SaveToDatabase();

        var tvfExtractable = new ExtractableDataSet(DataExportRepository, _tvfCatalogue);

        var selected = new SelectedDataSets(DataExportRepository, config, tvfExtractable, null);

        //make all columns part of the extraction
        foreach (var e in _tvfCatalogue.GetAllExtractionInformation(ExtractionCategory.Any))
            config.AddColumnToExtraction(tvfExtractable, e);

        //the default value should be 10
        Assert.That(_tvfTableInfo.GetAllParameters().Single().Value, Is.EqualTo("10"));

        //configure an extraction specific global of 1 so that only 1 chi number is fetched (which will be in the cohort)
        var globalP =
            new GlobalExtractionFilterParameter(DataExportRepository, config, "DECLARE @numberOfRecords AS int;")
            {
                Value = "1"
            };
        globalP.SaveToDatabase();

        var extractionCommand = new ExtractDatasetCommand(config, new ExtractableDatasetBundle(tvfExtractable));

        var source = new ExecuteDatasetExtractionSource();

        source.PreInitialize(extractionCommand, ThrowImmediatelyDataLoadEventListener.Quiet);

        var dt = source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows, Has.Count.EqualTo(1));

            Assert.That(dt.Columns[0].ColumnName, Is.EqualTo("ReleaseId"));
        });

        //should be a guid
        Assert.That(dt.Rows[0][0].ToString(), Has.Length.GreaterThan(10));
        Assert.That(dt.Rows[0][0].ToString(), Does.Contain('-'));

        selected.DeleteInDatabase();
        globalP.DeleteInDatabase();
        config.DeleteInDatabase();

        tvfExtractable.DeleteInDatabase();
    }
}
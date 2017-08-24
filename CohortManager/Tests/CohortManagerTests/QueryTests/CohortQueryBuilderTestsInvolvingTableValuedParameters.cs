using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibraryTests.Integration.TableValuedFunctionTests;
using CohortManagerLibrary.QueryBuilding;
using NUnit.Framework;
using Tests.Common;

namespace CohortManagerTests.QueryTests
{
    public class CohortQueryBuilderTestsInvolvingTableValuedParameters:DatabaseTests
    {
        private TestableTableValuedFunction _function = new TestableTableValuedFunction();
        [SetUp]
        public void CreateFunction()
        {
            _function.Create(DatabaseICanCreateRandomTablesIn, CatalogueRepository);
        }

        [Test]
        public void CohortGenerationDifferingTableValuedParametersTest()
        {
            //In this example we have 2 configurations which both target the same table valued function but which must have different parameter values
            var config1 = new AggregateConfiguration(CatalogueRepository,_function.Cata, "CohortGenerationDifferingTableValuedParametersTest_1");
            config1.CountSQL = null;
            config1.SaveToDatabase();

            var config2 = new AggregateConfiguration(CatalogueRepository,_function.Cata, "CohortGenerationDifferingTableValuedParametersTest_2");
            config2.CountSQL = null;
            config2.SaveToDatabase();
            
            var cic = new CohortIdentificationConfiguration(CatalogueRepository,"CohortGenerationDifferingTableValuedParametersTest");

            try
            {
                //make the string column the extraction identifier
                _function.ExtractionInformations[1].IsExtractionIdentifier = true;
                _function.ExtractionInformations[1].SaveToDatabase();
                
                //add the extraction identtifier as the only dimension one ach of the aggregate configurations that we will use for the cohort identification query
                new AggregateDimension(CatalogueRepository,_function.ExtractionInformations[1], config1);
                new AggregateDimension(CatalogueRepository,_function.ExtractionInformations[1], config2);

                Assert.IsNull(cic.RootCohortAggregateContainer_ID);
                
                //create a root container for it
                CohortAggregateContainer container = new CohortAggregateContainer(CatalogueRepository,SetOperation.INTERSECT);

                //set the container as the root container for the cohort identification task object
                cic.RootCohortAggregateContainer_ID = container.ID;
                cic.SaveToDatabase();

                //put both the aggregates into the container
                container.AddChild(config1, 0);
                container.AddChild(config2, 1);

                CohortQueryBuilder builder = new CohortQueryBuilder(cic);
                Assert.AreEqual(@"DECLARE @startNumber AS int;
SET @startNumber=5;
DECLARE @stopNumber AS int;
SET @stopNumber=10;
DECLARE @name AS varchar(50);
SET @name='fish';

(
	/*CohortGenerationDifferingTableValuedParametersTest_1*/
	SELECT distinct
	MyAwesomeFunction.[Name]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction

	INTERSECT

	/*CohortGenerationDifferingTableValuedParametersTest_2*/
	SELECT distinct
	MyAwesomeFunction.[Name]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction
)
", builder.SQL);

                //now override JUST @name
                var param1 = new AnyTableSqlParameter(CatalogueRepository,config1, "DECLARE @name AS varchar(50)");
                param1.Value = "'lobster'";
                param1.SaveToDatabase();
                
                var param2 = new AnyTableSqlParameter(CatalogueRepository,config2, "DECLARE @name AS varchar(50)");
                param2.Value = "'monkey'";
                param2.SaveToDatabase();

                CohortQueryBuilder builder2 = new CohortQueryBuilder(cic);

                Assert.AreEqual(@"DECLARE @startNumber AS int;
SET @startNumber=5;
DECLARE @stopNumber AS int;
SET @stopNumber=10;
DECLARE @name AS varchar(50);
SET @name='lobster';
DECLARE @name_2 AS varchar(50);
SET @name_2='monkey';

(
	/*CohortGenerationDifferingTableValuedParametersTest_1*/
	SELECT distinct
	MyAwesomeFunction.[Name]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name) AS MyAwesomeFunction

	INTERSECT

	/*CohortGenerationDifferingTableValuedParametersTest_2*/
	SELECT distinct
	MyAwesomeFunction.[Name]
	FROM 
	["+TestDatabaseNames.Prefix+@"ScratchArea]..MyAwesomeFunction(@startNumber,@stopNumber,@name_2) AS MyAwesomeFunction
)
", builder2.SQL);
            }
            finally
            {
                cic.DeleteInDatabase();
                config1.DeleteInDatabase();
                config2.DeleteInDatabase();
                
            }
        }

        [TearDown]
        public void Destroy()
        {
            _function.Destroy();
        }

    }
}
using System;
using System.Linq;
using System.Text.RegularExpressions;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.Cohort;
using Diagnostics.TestData;
using NUnit.Framework;
using Tests.Common;

namespace CohortManagerTests
{
    public class CohortIdentificationTests : DatabaseTests
    {

        protected BulkTestsData testData;
        protected AggregateConfiguration aggregate1;
        protected AggregateConfiguration aggregate2;
        protected AggregateConfiguration aggregate3;
        protected CohortIdentificationConfiguration cohortIdentificationConfiguration;
        protected CohortAggregateContainer rootcontainer;
        protected CohortAggregateContainer container1;

        [SetUp]
        public void SetupTestData()
        {
            testData = new BulkTestsData(CatalogueRepository,DatabaseICanCreateRandomTablesIn, 100);
            testData.SetupTestData();

            testData.ImportAsCatalogue();
           
            aggregate1 =
                new AggregateConfiguration(CatalogueRepository,testData.catalogue, "UnitTestAggregate1");
            aggregate1.CountSQL = null;
            aggregate1.SaveToDatabase();

            new AggregateDimension(CatalogueRepository,testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate1);

            aggregate2 =
                new AggregateConfiguration(CatalogueRepository,testData.catalogue, "UnitTestAggregate2");
            aggregate2.CountSQL = null;
            aggregate2.SaveToDatabase();

            new AggregateDimension(CatalogueRepository,testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate2);

            aggregate3 =
                new AggregateConfiguration(CatalogueRepository, testData.catalogue, "UnitTestAggregate3");
            aggregate3.CountSQL = null;
            aggregate3.SaveToDatabase();

            new AggregateDimension(CatalogueRepository,testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate3);

            cohortIdentificationConfiguration = new CohortIdentificationConfiguration (CatalogueRepository,"UnitTestIdentification");

            rootcontainer = new CohortAggregateContainer(CatalogueRepository,SetOperation.EXCEPT);
            container1 = new CohortAggregateContainer(CatalogueRepository,SetOperation.UNION);

            cohortIdentificationConfiguration.RootCohortAggregateContainer_ID = rootcontainer.ID;
            cohortIdentificationConfiguration.SaveToDatabase();
        }

        [TearDown]
        public void Cleanup()
        {

            container1.DeleteInDatabase();

            if (aggregate1 != null)
                aggregate1.DeleteInDatabase();
            
            if (aggregate2 != null)
                aggregate2.DeleteInDatabase();

            if (aggregate3 != null)
                aggregate3.DeleteInDatabase();


            if (cohortIdentificationConfiguration != null)
                cohortIdentificationConfiguration.DeleteInDatabase();
            
            if (testData != null)
                testData.DeleteCatalogue();
        }

        [TestFixtureTearDown]
        public void AfterAllTests()
        {
            testData.Destroy();
        }
    }
}
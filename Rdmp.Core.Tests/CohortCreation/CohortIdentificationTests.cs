// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Repositories;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.CohortCreation
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
            SetupTestData(CatalogueRepository);
        }

        public void SetupTestData(ICatalogueRepository repository)
        {
            testData = new BulkTestsData(repository, DiscoveredDatabaseICanCreateRandomTablesIn, 100);
            testData.SetupTestData();

            testData.ImportAsCatalogue();

            aggregate1 =
                new AggregateConfiguration(repository, testData.catalogue, "UnitTestAggregate1");
            aggregate1.CountSQL = null;
            aggregate1.SaveToDatabase();

            new AggregateDimension(repository, testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate1);

            aggregate2 =
                new AggregateConfiguration(repository, testData.catalogue, "UnitTestAggregate2");

            aggregate2.CountSQL = null;
            aggregate2.SaveToDatabase();

            new AggregateDimension(repository, testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate2);

            aggregate3 =
                new AggregateConfiguration(repository, testData.catalogue, "UnitTestAggregate3");
            aggregate3.CountSQL = null;
            aggregate3.SaveToDatabase();

            new AggregateDimension(repository, testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("chi")), aggregate3);

            cohortIdentificationConfiguration = new CohortIdentificationConfiguration(repository, "UnitTestIdentification");

            rootcontainer = new CohortAggregateContainer(repository, SetOperation.EXCEPT);
            container1 = new CohortAggregateContainer(repository, SetOperation.UNION);

            cohortIdentificationConfiguration.RootCohortAggregateContainer_ID = rootcontainer.ID;
            cohortIdentificationConfiguration.SaveToDatabase();

            cohortIdentificationConfiguration.EnsureNamingConvention(aggregate1);
            cohortIdentificationConfiguration.EnsureNamingConvention(aggregate2);
            cohortIdentificationConfiguration.EnsureNamingConvention(aggregate3);
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

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            testData.Destroy();
        }
    }
}
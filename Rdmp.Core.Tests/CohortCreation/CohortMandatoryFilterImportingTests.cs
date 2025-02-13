// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

namespace Rdmp.Core.Tests.CohortCreation;

public class CohortMandatoryFilterImportingTests : CohortIdentificationTests
{
    [Test]
    public void NoMandatoryFilters()
    {
        var importedAggregate =
            cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);
        var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

        //Must have a root container
        Assert.That(importedAggregateFilterContainer, Is.Null);

        importedAggregate.DeleteInDatabase();
    }

    [Test]
    public void ImportCatalogueWithMandatoryFilter()
    {
        var filter = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[0])
        {
            IsMandatory = true,
            WhereSQL = "There Be Dragons"
        };
        filter.SaveToDatabase();

        //ensure that it is picked SetUp
        var mandatoryFilters = testData.catalogue.GetAllMandatoryFilters();
        Assert.That(mandatoryFilters, Has.Length.EqualTo(1));
        Assert.That(mandatoryFilters[0], Is.EqualTo(filter));

        AggregateConfiguration importedAggregate = null;

        try
        {
            importedAggregate =
                cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);

            Assert.That(importedAggregate.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges));

            var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

            //Must have a root container
            Assert.That(importedAggregateFilterContainer, Is.Not.Null);

            //With an AND operation
            Assert.That(importedAggregateFilterContainer.Operation, Is.EqualTo(FilterContainerOperation.AND));

            var importedFilters = importedAggregateFilterContainer.GetFilters();
            Assert.That(importedFilters, Has.Length.EqualTo(1));

            //they are not the same object
            Assert.That(importedFilters[0], Is.Not.EqualTo(filter));
            Assert.Multiple(() =>
            {
                //the deployed filter knows its parent it was cloned from
                Assert.That(importedFilters[0].ClonedFromExtractionFilter_ID, Is.EqualTo(filter.ID));
                //the WHERE SQL of the filters should be the same
                Assert.That(importedFilters[0].WhereSQL, Is.EqualTo(filter.WhereSQL));
            });
        }
        finally
        {
            filter.DeleteInDatabase();

            if (importedAggregate != null)
            {
                importedAggregate.RootFilterContainer.DeleteInDatabase();
                importedAggregate.DeleteInDatabase();
            }
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ImportCatalogueWithSingleFilterThatHasAParameter(bool createAGlobalOverrideBeforeHand)
    {
        const string parameterSQL = "DECLARE @dragonCount as varchar(100)";

        var filter = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[0])
        {
            IsMandatory = true,
            WhereSQL = "There Be Dragons AND @dragonCount = 1"
        };
        filter.SaveToDatabase();

        //Should result in the creation of a parameter
        new ParameterCreator(new ExtractionFilterFactory(testData.extractionInformations[0]), null, null)
            .CreateAll(filter, null);

        var filterParameters = filter.ExtractionFilterParameters.ToArray();
        Assert.That(filterParameters, Has.Length.EqualTo(1));

        filterParameters[0].ParameterSQL = parameterSQL;
        filterParameters[0].Value = "'No More than 300 Dragons Please'";
        filterParameters[0].SaveToDatabase();

        AnyTableSqlParameter global = null;

        if (createAGlobalOverrideBeforeHand)
        {
            global = new AnyTableSqlParameter(CatalogueRepository, cohortIdentificationConfiguration, parameterSQL)
            {
                Value = "'At Least 1000 Dragons'"
            };
            global.SaveToDatabase();
        }

        //ensure that it is picked SetUp
        var mandatoryFilters = testData.catalogue.GetAllMandatoryFilters();
        Assert.That(mandatoryFilters, Has.Length.EqualTo(1));
        Assert.That(mandatoryFilters[0], Is.EqualTo(filter));


        AggregateConfiguration importedAggregate = null;

        try
        {
            importedAggregate =
                cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);
            var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

            //Must have a root container
            Assert.That(importedAggregateFilterContainer, Is.Not.Null);

            //With an AND operation
            Assert.That(importedAggregateFilterContainer.Operation, Is.EqualTo(FilterContainerOperation.AND));

            var importedFilters = importedAggregateFilterContainer.GetFilters();
            Assert.That(importedFilters, Has.Length.EqualTo(1));

            //Because the configuration already has a parameter with the same declaration it should not bother to import the parameter from the underlying filter
            if (createAGlobalOverrideBeforeHand)
            {
                Assert.That(importedFilters[0].GetAllParameters(), Is.Empty);
            }
            else
            {
                //Because there is no global we should be creating a clone of the parameter too
                var paramClones = importedFilters[0].GetAllParameters();
                Assert.That(paramClones, Has.Length.EqualTo(1));

                //clone should have same SQL and Value
                Assert.That(paramClones[0].ParameterSQL, Is.EqualTo(parameterSQL));
                Assert.Multiple(() =>
                {
                    Assert.That(paramClones[0].ParameterSQL, Is.EqualTo(filterParameters[0].ParameterSQL));
                    Assert.That(paramClones[0].Value, Is.EqualTo(filterParameters[0].Value));

                    //but not be the same object in database
                    Assert.That(paramClones[0], Is.Not.EqualTo(filterParameters[0]));
                });
            }
        }
        finally
        {
            global?.DeleteInDatabase();

            filter.DeleteInDatabase();

            if (importedAggregate != null)
            {
                importedAggregate.RootFilterContainer.DeleteInDatabase();
                importedAggregate.DeleteInDatabase();
            }
        }
    }

    [Test]
    public void ImportCatalogueWithMultipleMandatoryFilters()
    {
        //First mandatory
        var filter1 = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[0])
        {
            IsMandatory = true,
            WhereSQL = "There Be Dragons"
        };
        filter1.SaveToDatabase();

        //Second mandatory
        var filter2 = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[1])
        {
            IsMandatory = true,
            WhereSQL = "And Months"
        };
        filter2.SaveToDatabase();

        //Then one that is not mandatory
        var filter3 = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[2])
        {
            IsMandatory = false,
            WhereSQL = "But Can Also Be Flies"
        };
        filter3.SaveToDatabase();

        //ensure that both are picked SetUp as mandatory filters by catalogue
        var mandatoryFilters = testData.catalogue.GetAllMandatoryFilters();
        Assert.That(mandatoryFilters, Has.Length.EqualTo(2));

        AggregateConfiguration importedAggregate = null;

        try
        {
            //import the Catalogue
            importedAggregate =
                cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);
            var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

            //Must have a root container
            Assert.That(importedAggregateFilterContainer, Is.Not.Null);

            //the AND container should be there
            Assert.That(importedAggregateFilterContainer.Operation, Is.EqualTo(FilterContainerOperation.AND));

            //the filters should both be there (See above test for WHERE SQL, ID etc checking)
            var importedFilters = importedAggregateFilterContainer.GetFilters();
            Assert.That(importedFilters, Has.Length.EqualTo(2));
        }
        finally
        {
            filter1.DeleteInDatabase();
            filter2.DeleteInDatabase();
            filter3.DeleteInDatabase();

            if (importedAggregate != null)
            {
                importedAggregate.RootFilterContainer.DeleteInDatabase();
                importedAggregate.DeleteInDatabase();
            }
        }
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;

namespace Rdmp.Core.Tests.CohortCreation;

public class CohortMandatoryFilterImportingTests : CohortIdentificationTests
{
    [Test]
    public void NoMandatoryFilters()
    {
        var importedAggregate = cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);
        var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

        //Must have a root container
        Assert.IsNull(importedAggregateFilterContainer);

        importedAggregate.DeleteInDatabase();
    }

    [Test]
    public void ImportCatalogueWithMandatoryFilter()
    {
        var filter = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[0]);
        filter.IsMandatory = true;
        filter.WhereSQL = "There Be Dragons";
        filter.SaveToDatabase();

        //ensure that it is picked SetUp
        var mandatoryFilters = testData.catalogue.GetAllMandatoryFilters();
        Assert.AreEqual(1, mandatoryFilters.Length);
        Assert.AreEqual(filter, mandatoryFilters[0]);

        AggregateConfiguration importedAggregate = null;
            
        try
        {
                
            importedAggregate = cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue,null);

            Assert.AreEqual(ChangeDescription.NoChanges, importedAggregate.HasLocalChanges().Evaluation);

            var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

            //Must have a root container
            Assert.IsNotNull(importedAggregateFilterContainer);

            //With an AND operation
            Assert.AreEqual(FilterContainerOperation.AND,importedAggregateFilterContainer.Operation);

            var importedFilters = importedAggregateFilterContainer.GetFilters();
            Assert.AreEqual(1, importedFilters.Length);
            
            //they are not the same object
            Assert.AreNotEqual(filter, importedFilters[0]);
            //the deployed filter knows its parent it was cloned from
            Assert.AreEqual(filter.ID, importedFilters[0].ClonedFromExtractionFilter_ID);
            //the WHERE SQL of the filters should be the same
            Assert.AreEqual(filter.WhereSQL, importedFilters[0].WhereSQL);
                
        }
        finally
        {
            filter.DeleteInDatabase();

            if(importedAggregate != null)
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
        string parameterSQL = "DECLARE @dragonCount as varchar(100)";

        var filter = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[0]);
        filter.IsMandatory = true;
        filter.WhereSQL = "There Be Dragons AND @dragonCount = 1";
        filter.SaveToDatabase();

        //Should result in the creation of a parameter
        new ParameterCreator(new ExtractionFilterFactory(testData.extractionInformations[0]),null,null).CreateAll(filter,null);

        var filterParameters = filter.ExtractionFilterParameters.ToArray();
        Assert.AreEqual(1, filterParameters.Length);

        filterParameters[0].ParameterSQL = parameterSQL;
        filterParameters[0].Value = "'No More than 300 Dragons Please'";
        filterParameters[0].SaveToDatabase();

        AnyTableSqlParameter global = null;

        if (createAGlobalOverrideBeforeHand)
        {    
            global = new AnyTableSqlParameter(CatalogueRepository, cohortIdentificationConfiguration,parameterSQL);
            global.Value = "'At Least 1000 Dragons'";
            global.SaveToDatabase();
        }

        //ensure that it is picked SetUp
        var mandatoryFilters = testData.catalogue.GetAllMandatoryFilters();
        Assert.AreEqual(1, mandatoryFilters.Length);
        Assert.AreEqual(filter, mandatoryFilters[0]);


        AggregateConfiguration importedAggregate = null;

        try
        {
            importedAggregate = cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);
            var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

            //Must have a root container
            Assert.IsNotNull(importedAggregateFilterContainer);

            //With an AND operation
            Assert.AreEqual(FilterContainerOperation.AND, importedAggregateFilterContainer.Operation);

            var importedFilters = importedAggregateFilterContainer.GetFilters();
            Assert.AreEqual(1, importedFilters.Length);

            //Because the configuration already has a parameter with the same declaration it should not bother to import the parameter from the underlying filter
            if(createAGlobalOverrideBeforeHand)
                Assert.AreEqual(0,importedFilters[0].GetAllParameters().Length);
            else
            {
                //Because there is no global we should be creating a clone of the parameter too
                var paramClones = importedFilters[0].GetAllParameters();
                Assert.AreEqual(1, paramClones.Length);

                //clone should have same SQL and Value
                Assert.AreEqual(parameterSQL,paramClones[0].ParameterSQL);
                Assert.AreEqual(filterParameters[0].ParameterSQL, paramClones[0].ParameterSQL);
                Assert.AreEqual(filterParameters[0].Value, paramClones[0].Value);

                //but not be the same object in database
                Assert.AreNotEqual(filterParameters[0], paramClones[0]);
            }

        }
        finally
        {
            if(global != null)
                global.DeleteInDatabase();

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
        var filter1 = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[0]);
        filter1.IsMandatory = true;
        filter1.WhereSQL = "There Be Dragons";
        filter1.SaveToDatabase();

        //Second mandatory
        var filter2 = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[1]);
        filter2.IsMandatory = true;
        filter2.WhereSQL = "And Months";
        filter2.SaveToDatabase();

        //Then one that is not mandatory
        var filter3 = new ExtractionFilter(CatalogueRepository, "MyMandatoryFilter", testData.extractionInformations[2]);
        filter3.IsMandatory = false;
        filter3.WhereSQL = "But Can Also Be Flies";
        filter3.SaveToDatabase();

        //ensure that both are picked SetUp as mandatory filters by catalogue
        var mandatoryFilters = testData.catalogue.GetAllMandatoryFilters();
        Assert.AreEqual(2, mandatoryFilters.Length);

        AggregateConfiguration importedAggregate = null;

        try
        {
            //import the Catalogue               
            importedAggregate = cohortIdentificationConfiguration.CreateNewEmptyConfigurationForCatalogue(testData.catalogue, null);
            var importedAggregateFilterContainer = importedAggregate.RootFilterContainer;

            //Must have a root container
            Assert.IsNotNull(importedAggregateFilterContainer);

            //the AND container should be there
            Assert.AreEqual(FilterContainerOperation.AND, importedAggregateFilterContainer.Operation);

            //the filters should both be there (See above test for WHERE SQL, ID etc checking)
            var importedFilters = importedAggregateFilterContainer.GetFilters();
            Assert.AreEqual(2, importedFilters.Length);
        }
        finally
        {
            filter1.DeleteInDatabase();
            filter2.DeleteInDatabase();
            filter3.DeleteInDatabase();

            if(importedAggregate != null)
            {
                importedAggregate.RootFilterContainer.DeleteInDatabase();
                importedAggregate.DeleteInDatabase();
            }
        }
    }
}
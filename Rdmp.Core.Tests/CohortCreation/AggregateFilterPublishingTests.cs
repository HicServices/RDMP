// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Tests.CohortCreation;

public class AggregateFilterPublishingTests : CohortIdentificationTests
{
    private AggregateFilter _filter;
    private AggregateFilterContainer _container;

    private ExtractionInformation _chiExtractionInformation;

    [OneTimeSetUp]
    protected override void SetUp()
    {
        base.SetUp();

        aggregate1.RootFilterContainer_ID =
            new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.AND).ID;
        aggregate1.SaveToDatabase();

        _chiExtractionInformation = aggregate1.AggregateDimensions.Single().ExtractionInformation;

        _container = (AggregateFilterContainer)aggregate1.RootFilterContainer;

        _filter = new AggregateFilter(CatalogueRepository, "folk", _container);
    }

    [Test]
    public void NotPopulated_Description()
    {
        var ex = Assert.Throws<Exception>(() =>
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_container,
                _filter, null));
        Assert.AreEqual("Cannot clone filter called 'folk' because:There is no description", ex?.Message);
    }

    [Test]
    public void NotPopulated_DescriptionTooShort()
    {
        _filter.Description = "fish";
        var ex = Assert.Throws<Exception>(() =>
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_container,
                _filter, null));
        Assert.AreEqual(
            "Cannot clone filter called 'folk' because:Description is not long enough (minimum length is 20 characters)",
            ex?.Message);
    }

    [Test]
    public void NotPopulated_WhereSQLNotSet()
    {
        _filter.Description = "fish swim in the sea and make people happy to be";
        var ex = Assert.Throws<Exception>(() =>
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_container,
                _filter, null));
        Assert.AreEqual("Cannot clone filter called 'folk' because:WhereSQL is not populated", ex?.Message);
    }

    /// <summary>
    /// Check parameters can be created without a comment
    /// </summary>
    [Test]
    public void NotPopulated_ParameterNoComment()
    {
        _filter.Description = "fish swim in the sea and make people happy to be";
        _filter.WhereSQL = "LovelyCoconuts = @coconutCount";
        _filter.SaveToDatabase();
        new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(_filter, null);

        var importedFilter =
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(null, _filter,
                null);
        Assert.AreEqual("folk", importedFilter.Name);
    }


    [Test]
    public void NotPopulated_ParameterNotSet()
    {
        _filter.Description = "fish swim in the sea and make people happy to be";
        _filter.WhereSQL = "LovelyCoconuts = @coconutCount";

        new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(_filter, null);
        var parameter = _filter.GetAllParameters().Single();
        parameter.Comment = "It's coconut time!";
        parameter.Value = null; //clear its value
        parameter.SaveToDatabase();

        var ex = Assert.Throws<Exception>(() =>
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_container,
                _filter, null));
        Assert.AreEqual(
            "Cannot clone filter called 'folk' because:Parameter '@coconutCount' was rejected :There is no value/default value listed",
            ex?.Message);
    }

    [Test]
    public void ShortcutFiltersWork_ProperlyReplicatesParentAndHasFK()
    {
        _filter.WhereSQL = "folk=1";
        _filter.SaveToDatabase();

        var sql = new CohortQueryBuilder(aggregate1, null, null).SQL;

        Console.WriteLine(sql);
        Assert.IsTrue(sql.Contains("folk=1"));


        var shortcutAggregate =
            new AggregateConfiguration(CatalogueRepository, testData.catalogue, "UnitTestShortcutAggregate");

        _ = new AggregateDimension(CatalogueRepository,
            testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("sex")), shortcutAggregate);

        //before it is a shortcut it has no filters
        Assert.IsFalse(shortcutAggregate.GetQueryBuilder().SQL.Contains("WHERE"));

        //make it a shortcut 
        shortcutAggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = aggregate1.ID;
        shortcutAggregate.SaveToDatabase();

        var sqlShortcut = shortcutAggregate.GetQueryBuilder().SQL;

        //shortcut should have its own dimensions
        Assert.IsTrue(sqlShortcut.Contains("[sex]"));
        Assert.IsFalse(sqlShortcut.Contains("[chi]"));

        //but should have a REFERENCE (not a clone!) to aggregate 1's filters
        Assert.IsTrue(sqlShortcut.Contains("folk=1"));

        //make sure it is a reference by changing the original
        _filter.WhereSQL = "folk=2";
        _filter.SaveToDatabase();
        Assert.IsTrue(shortcutAggregate.GetQueryBuilder().SQL.Contains("folk=2"));

        //shouldn't work because of the dependency of the child - should give a foreign key error
        if (CatalogueRepository is TableRepository) Assert.Throws<SqlException>(aggregate1.DeleteInDatabase);

        //delete the child
        shortcutAggregate.DeleteInDatabase();

        aggregate1.DeleteInDatabase();
        aggregate1 = null;
    }

    [Test]
    public void ShortcutFilters_AlreadyHasFilter()
    {
        Assert.IsNotNull(aggregate1.RootFilterContainer_ID);
        var ex = Assert.Throws<NotSupportedException>(() =>
            aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = -500); //not ok
        Assert.AreEqual(
            "Cannot set OverrideFiltersByUsingParentAggregateConfigurationInstead_ID because this AggregateConfiguration already has a filter container set (if you were to be a shortcut and also have a filter tree set it would be very confusing)",
            ex?.Message);
    }

    [Test]
    public void ShortcutFilters_AlreadyHasFilter_ButSettingItToNull()
    {
        Assert.IsNotNull(aggregate1.RootFilterContainer_ID);
        Assert.DoesNotThrow(
            () => { aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null; }); // is ok
    }


    [Test]
    public void ShortcutFilters_DoesNotHaveFilter_SetOne()
    {
        aggregate1.RootFilterContainer_ID = null;
        Assert.DoesNotThrow(() =>
        {
            aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null;
        }); // is ok
        Assert.DoesNotThrow(() =>
        {
            aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = -19;
        }); // is ok
        var ex = Assert.Throws<NotSupportedException>(() => aggregate1.RootFilterContainer_ID = 123);
        Assert.AreEqual(
            "This AggregateConfiguration has a shortcut to another AggregateConfiguration's Filters (its OverrideFiltersByUsingParentAggregateConfigurationInstead_ID is -19) which means it cannot be assigned its own RootFilterContainerID",
            ex?.Message);
    }

    [Test]
    public void CloneWorks_AllPropertiesMatchIncludingParameters()
    {
        _filter.Description = "fish swim in the sea and make people happy to be";
        _filter.WhereSQL = "LovelyCoconuts = @coconutCount";

        new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(_filter, null);
        _filter.SaveToDatabase();

        Assert.IsNull(_filter.ClonedFromExtractionFilter_ID);

        var parameter = _filter.GetAllParameters().Single();
        parameter.ParameterSQL = "Declare @coconutCount int";
        parameter.Comment = "It's coconut time!";
        parameter.Value = "3";
        parameter.SaveToDatabase();

        var newMaster =
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(null, _filter,
                null);

        //we should now be a clone of the master we just created
        Assert.AreEqual(_filter.ClonedFromExtractionFilter_ID, newMaster.ID);
        Assert.IsTrue(newMaster.Description.StartsWith(_filter.Description)); //it adds some addendum stuff onto it
        Assert.AreEqual(_filter.WhereSQL, newMaster.WhereSQL);

        Assert.AreEqual(_filter.GetAllParameters().Single().ParameterName,
            newMaster.GetAllParameters().Single().ParameterName);
        Assert.AreEqual(_filter.GetAllParameters().Single().ParameterSQL,
            newMaster.GetAllParameters().Single().ParameterSQL);
        Assert.AreEqual(_filter.GetAllParameters().Single().Value, newMaster.GetAllParameters().Single().Value);
    }
}
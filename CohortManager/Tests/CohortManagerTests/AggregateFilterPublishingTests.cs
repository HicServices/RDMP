using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.FilterImporting;
using CatalogueLibrary.FilterImporting.Construction;
using CohortManagerLibrary.QueryBuilding;
using NUnit.Framework;
using Tests.Common;

namespace CohortManagerTests
{
    public class AggregateFilterPublishingTests:CohortIdentificationTests
    {

        private AggregateFilter _filter;
        private AggregateFilterContainer _container;

        private ExtractionInformation _chiExtractionInformation;
        

        [SetUp]
        public void CreateAFilter()
        {
            aggregate1.RootFilterContainer_ID = new AggregateFilterContainer(CatalogueRepository,FilterContainerOperation.AND).ID;
            aggregate1.SaveToDatabase();

            _chiExtractionInformation = aggregate1.AggregateDimensions.Single().ExtractionInformation;

            _container = aggregate1.RootFilterContainer;

            _filter = new AggregateFilter(CatalogueRepository,"folk", _container);
        }

        [TearDown]
        public void DeleteFilter()
        {
            _container.DeleteInDatabase();//cascades to filter
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Cannot clone filter called 'folk' because:There is no description")]
        public void NotPopulated_Description()
        {
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_filter, null);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Cannot clone filter called 'folk' because:Description is not long enough (minimum length is 20 characters)")]
        public void NotPopulated_DescriptionTooShort()
        {
            _filter.Description = "fish";
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_filter, null);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Cannot clone filter called 'folk' because:WhereSQL is not populated")]
        public void NotPopulated_WhereSQLNotSet()
        {
            _filter.Description = "fish swim in the sea and make people happy to be";
            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_filter, null);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Cannot clone filter called 'folk' because:Parameter '@coconutCount' was rejected :There is no description comment")]
        public void NotPopulated_ParameterNotCreated()
        {
            _filter.Description = "fish swim in the sea and make people happy to be";
            _filter.WhereSQL = "LovelyCoconuts = @coconutCount";
            _filter.SaveToDatabase();
            new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(_filter, null);

            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_filter, null);
        }


        [Test]
        [ExpectedException(ExpectedMessage = "Cannot clone filter called 'folk' because:Parameter '@coconutCount' was rejected :There is no value/default value listed")]
        public void NotPopulated_ParameterNotSet()
        {
            _filter.Description = "fish swim in the sea and make people happy to be";
            _filter.WhereSQL = "LovelyCoconuts = @coconutCount";
            
            new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(_filter, null);
            var parameter = _filter.GetAllParameters().Single();
            parameter.Comment = "It's coconut time!";
            parameter.Value = null;//clear it's value
            parameter.SaveToDatabase();

            new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation),null).ImportFilter(_filter,null);
        }

        [Test]
        public void ShortcutFiltersWork_ProperlyReplicatesParentAndHasFK()
        {
            _filter.WhereSQL = "folk=1";
            _filter.SaveToDatabase();

            string sql = new CohortQueryBuilder(aggregate1, null).SQL;

            Console.WriteLine(sql);
            Assert.IsTrue(sql.Contains("folk=1"));


            var shortcutAggregate =
                new AggregateConfiguration(CatalogueRepository,testData.catalogue, "UnitTestShortcutAggregate");

            new AggregateDimension(CatalogueRepository,testData.extractionInformations.Single(e => e.GetRuntimeName().Equals("sex")), shortcutAggregate);

            //before it is a shortcut it has no filters
            Assert.IsFalse(shortcutAggregate.GetQueryBuilder().SQL.Contains("WHERE"));

            //make it a shortcut 
            shortcutAggregate.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = aggregate1.ID;
            shortcutAggregate.SaveToDatabase();

            string sqlShortcut = shortcutAggregate.GetQueryBuilder().SQL;
            
            //shortcut should have it's own dimensions
            Assert.IsTrue(sqlShortcut.Contains("[sex]"));
            Assert.IsFalse(sqlShortcut.Contains("[chi]"));
            
            //but should have a REFERENCE (not a clone!) to aggregate 1's filters
            Assert.IsTrue(sqlShortcut.Contains("folk=1"));

            //make sure it is a reference by changing the original
            _filter.WhereSQL = "folk=2";
            _filter.SaveToDatabase();
            Assert.IsTrue(shortcutAggregate.GetQueryBuilder().SQL.Contains("folk=2"));

            //shouldnt work because of the dependency of the child - should give a foreign key error
            Assert.Throws<SqlException>(aggregate1.DeleteInDatabase);

            //delete the child
            shortcutAggregate.DeleteInDatabase();

            aggregate1.DeleteInDatabase();
            aggregate1 = null;
        }

        [Test]
        [ExpectedException(ExpectedMessage= "Cannot set OverrideFiltersByUsingParentAggregateConfigurationInstead_ID because this AggregateConfiguration already has a filter container set (if you were to be a shortcut and also have a filter tree set it would be very confusing)")]
        public void ShortcutFilters_AlreadyHasFilter()
        {
            Assert.IsNotNull(aggregate1.RootFilterContainer_ID);
            aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = -500;//not ok
        }
        [Test]
        public void ShortcutFilters_AlreadyHasFilter_ButSettingItToNull()
        {
            Assert.IsNotNull(aggregate1.RootFilterContainer_ID);
            Assert.DoesNotThrow(
                () => { aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null; }); // is ok
        }


        [Test]
        [ExpectedException(ExpectedMessage = "This AggregateConfiguration has a shortcut to another AggregateConfiguration's Filters (It's OverrideFiltersByUsingParentAggregateConfigurationInstead_ID is -19) which means it cannot be assigned it's own RootFilterContainerID")]
        public void ShortcutFilters_DoesNotHaveFilter_SetOne()
        {
            aggregate1.RootFilterContainer_ID = null;
            Assert.DoesNotThrow(() => { aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = null; }); // is ok
            Assert.DoesNotThrow(() => { aggregate1.OverrideFiltersByUsingParentAggregateConfigurationInstead_ID = -19; }); // is ok
            aggregate1.RootFilterContainer_ID = 123;
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
            
            var newMaster = new FilterImporter(new ExtractionFilterFactory(_chiExtractionInformation), null).ImportFilter(_filter, null);

            //we should now be a clone of the master we just created
            Assert.AreEqual(_filter.ClonedFromExtractionFilter_ID,newMaster.ID);
            Assert.IsTrue(newMaster.Description.StartsWith(_filter.Description)); //it adds some addendum stuff onto it
            Assert.AreEqual(_filter.WhereSQL,newMaster.WhereSQL);

            Assert.AreEqual(_filter.GetAllParameters().Single().ParameterName, newMaster.GetAllParameters().Single().ParameterName);
            Assert.AreEqual(_filter.GetAllParameters().Single().ParameterSQL, newMaster.GetAllParameters().Single().ParameterSQL);
            Assert.AreEqual(_filter.GetAllParameters().Single().Value, newMaster.GetAllParameters().Single().Value);

        }
    }
}

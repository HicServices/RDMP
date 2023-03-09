// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.Curation.FilterImporting.Construction;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation
{
    public class CohortContainerAndCloningTests : CohortIdentificationTests
    {
        [Test]
        public void AggregateOrdering_ExplicitSetting_CorrectOrder()
        {
            try
            {
                //set the order so that 2 comes before 1
                rootcontainer.AddChild(aggregate2, 1);
                rootcontainer.AddChild(aggregate1, 5);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            try
            {
                Assert.AreEqual(1, aggregate2.Order);
                Assert.AreEqual(5, aggregate1.Order);

                Assert.AreEqual(rootcontainer.GetAggregateConfigurations()[0].ID, aggregate2.ID);
                Assert.AreEqual(rootcontainer.GetAggregateConfigurations()[1].ID, aggregate1.ID);
            }
            finally
            {
                rootcontainer.RemoveChild(aggregate1);
                rootcontainer.RemoveChild(aggregate2);
            }
        }

        [Test]
        public void CloneChild_NamingCorrectNewObject()
        {

            //should not follow naming convention
            aggregate1.Name = "fish";
            Assert.IsFalse(cohortIdentificationConfiguration.IsValidNamedConfiguration(aggregate1));

            //add a clone using aggregate1 as a template 
            var clone = cohortIdentificationConfiguration.ImportAggregateConfigurationAsIdentifierList(aggregate1,null);
            //add the clone
            rootcontainer.AddChild(clone, 0);

            try
            {
                //there should be 1 child
                AggregateConfiguration[] aggregateConfigurations = rootcontainer.GetAggregateConfigurations();
                Assert.AreEqual(1, aggregateConfigurations.Length);

                //child should follow naming convention
                Assert.IsTrue(cohortIdentificationConfiguration.IsValidNamedConfiguration(aggregateConfigurations[0]));

                //clone should have a different ID - also it was created after so should be higher ID
                Assert.Greater(aggregateConfigurations[0].ID, aggregate1.ID);

            }
            finally
            {
                aggregate1.RevertToDatabaseState();

                rootcontainer.RemoveChild(clone);

                if (clone.RootFilterContainer != null)
                    clone.RootFilterContainer.DeleteInDatabase();

                clone.DeleteInDatabase();
            }
        }
        [Test]
        public void CloneChildWithFilter_IDsDifferent()
        {
            //aggregate 1 is now a normal non cohort aggregate
            var container = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.OR);
            aggregate1.CountSQL = "count(*)";
            aggregate1.RootFilterContainer_ID = container.ID;
            aggregate1.SaveToDatabase();

            //with filters
            var filter = new AggregateFilter(CatalogueRepository, "MyFilter", container);
            filter.WhereSQL = "sex=@sex";

            //and parameters
            new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(filter, null);
            filter.SaveToDatabase();

            var param = (AggregateFilterParameter)filter.GetAllParameters().Single();
            param.Value = "'M'";
            param.SaveToDatabase();

            //we are importing this graph aggregate as a new cohort identification aggregate
            var clone = cohortIdentificationConfiguration.ImportAggregateConfigurationAsIdentifierList(aggregate1, null);

            //since its a cohort aggregate it should be identical to the origin Aggregate except it has a different ID and no count SQL
            Assert.AreEqual(clone.CountSQL,null);

            //get the original sql
            var aggregateSql = aggregate1.GetQueryBuilder().SQL;

            try
            {
                Assert.AreNotEqual(clone.ID, aggregate1.ID);
                Assert.AreNotEqual(clone.RootFilterContainer_ID, aggregate1.RootFilterContainer_ID);


                var cloneContainer = clone.RootFilterContainer;
                var cloneFilter = cloneContainer.GetFilters().Single();

                Assert.AreNotEqual(cloneContainer.ID, container.ID);
                Assert.AreNotEqual(cloneFilter.ID, filter.ID);

                var cloneParameter = (AggregateFilterParameter)cloneFilter.GetAllParameters().Single();
                Assert.AreNotEqual(cloneParameter.ID, param.ID);

                //it has a different ID and is part of an aggregate filter container (It is presumed to be involved with cohort identification cohortIdentificationConfiguration) which means it will be called cic_X_
                string cohortAggregateSql = new CohortQueryBuilder(clone,null,null).SQL;


//the basic aggregate has the filter, parameter and group by
                Assert.AreEqual(CollapseWhitespace(
                    string.Format(
@"DECLARE @sex AS varchar(50);
SET @sex='M';
/*cic_{0}_UnitTestAggregate1*/
SELECT 
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi],
count(*)
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
WHERE
(
/*MyFilter*/
sex=@sex
)

group by 
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
order by 
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]",cohortIdentificationConfiguration.ID)),CollapseWhitespace(aggregateSql));

//the expected differences are
//1. should not have the count
//2. should not have the group by
//3. should be marked with the cic comment with the ID matching the CohortIdentificationConfiguration.ID
//4. should have a distinct on the identifier column

                Assert.AreEqual(
@"DECLARE @sex AS varchar(50);
SET @sex='M';
/*cic_"+cohortIdentificationConfiguration.ID+@"_UnitTestAggregate1*/
SELECT
distinct
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData].[chi]
FROM 
["+TestDatabaseNames.Prefix+@"ScratchArea].[dbo].[BulkData]
WHERE
(
/*MyFilter*/
sex=@sex
)", cohortAggregateSql);


                clone.RootFilterContainer.DeleteInDatabase();
                container.DeleteInDatabase();
            }
            finally
            {
                clone.DeleteInDatabase();
            }
        }


        [Test]
        public void CohortIdentificationConfiguration_CloneEntirely()
        {
            //set the order so that 2 comes before 1
            rootcontainer.AddChild(aggregate1, 5);

            rootcontainer.AddChild(container1);
            container1.AddChild(aggregate2,1);
            container1.AddChild(aggregate3,2);


            //create a filter too
            var container = new AggregateFilterContainer(CatalogueRepository, FilterContainerOperation.OR);

            aggregate1.RootFilterContainer_ID = container.ID;
            aggregate1.SaveToDatabase();

            var filter = new AggregateFilter(CatalogueRepository, "MyFilter", container);
            filter.WhereSQL = "sex=@sex";
            new ParameterCreator(new AggregateFilterFactory(CatalogueRepository), null, null).CreateAll(filter,null);
            filter.SaveToDatabase();

            //with a parameter too
            var param = (AggregateFilterParameter)filter.GetAllParameters().Single();
            param.Value = "'M'";
            param.SaveToDatabase();

            cohortIdentificationConfiguration.RootCohortAggregateContainer_ID = rootcontainer.ID;
            cohortIdentificationConfiguration.SaveToDatabase();

            try
            {
                var clone = cohortIdentificationConfiguration.CreateClone(new ThrowImmediatelyCheckNotifier());

                //the objects should be different
                Assert.AreNotEqual(cohortIdentificationConfiguration.ID, clone.ID);
                Assert.IsTrue(clone.Name.EndsWith("(Clone)"));

                Assert.AreNotEqual(clone.RootCohortAggregateContainer_ID, cohortIdentificationConfiguration.RootCohortAggregateContainer_ID);
                Assert.IsNotNull(clone.RootCohortAggregateContainer_ID);

                var beforeSQL = new CohortQueryBuilder(cohortIdentificationConfiguration,null).SQL;
                var cloneSQL = new CohortQueryBuilder(clone,null).SQL;

                beforeSQL = Regex.Replace(beforeSQL, "cic_[0-9]+_", "");
                cloneSQL = Regex.Replace(cloneSQL, "cic_[0-9]+_", "");

                //the SQL should be the same for them 
                Assert.AreEqual(beforeSQL,cloneSQL);

                var containerClone = clone.RootCohortAggregateContainer.GetAllAggregateConfigurationsRecursively()
                    .Where(a => a.RootFilterContainer_ID != null)
                    .Select(ag => ag.RootFilterContainer).Single();

                Assert.AreNotEqual(container, containerClone);
                
                //cleanup phase
                clone.DeleteInDatabase();
                containerClone.DeleteInDatabase();

            }
            finally
            {
                rootcontainer.RemoveChild(aggregate1);
                container1.RemoveChild(aggregate2);
                container1.RemoveChild(aggregate3);

                filter.DeleteInDatabase();
                container.DeleteInDatabase();
            }
        }

    }
}

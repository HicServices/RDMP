using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using NUnit.Framework;
using Tests.Common;

namespace CohortManagerTests
{
    public class SimpleCohortIdentificationTests:DatabaseTests
    {
        [Test]
        public void CreateNewCohortIdentificationConfiguration_SaveAndReload()
        {
            var config = new CohortIdentificationConfiguration(CatalogueRepository, "franky");
            
            try
            {
                Assert.IsTrue(config.StillExists());
                Assert.AreEqual("franky",config.Name);
            
                config.Description = "Hi there";
                config.SaveToDatabase();


                CohortIdentificationConfiguration config2 = CatalogueRepository.GetObjectByID<CohortIdentificationConfiguration>(config.ID);
                Assert.AreEqual("Hi there", config2.Description);
            }
            finally 
            {
                config.DeleteInDatabase();
                Assert.IsFalse(config.StillExists());
            }
        }

        [Test]
        public void ContainerCreate()
        {
            var container = new CohortAggregateContainer(CatalogueRepository,SetOperation.UNION);

            try
            {
                Assert.AreEqual(SetOperation.UNION,container.Operation);

                container.Operation = SetOperation.INTERSECT;
                container.SaveToDatabase();

                var container2 = CatalogueRepository.GetObjectByID<CohortAggregateContainer>(container.ID);
                Assert.AreEqual(SetOperation.INTERSECT, container2.Operation);
            }
            finally
            {
                container.DeleteInDatabase();
            }
        }


        [Test]
        public void Container_Subcontainering()
        {
            var container = new CohortAggregateContainer(CatalogueRepository,SetOperation.UNION);
            
            var container2 = new CohortAggregateContainer(CatalogueRepository,SetOperation.INTERSECT);
            try
            {
                Assert.AreEqual(0, container.GetSubContainers().Length);

              
                Assert.AreEqual(0, container.GetSubContainers().Length);

                //set container to parent
                container.AddChild(container2);

                //container 1 should now contain container 2
                Assert.AreEqual(1, container.GetSubContainers().Length);
                Assert.Contains(container2, container.GetSubContainers());

                //container 2 should not have any children
                Assert.AreEqual(0, container2.GetSubContainers().Length);
            }
            finally
            {
                container.DeleteInDatabase();

                //container 2 was contained within container 1 so should have also been deleted
                Assert.Throws<KeyNotFoundException>(
                    () => CatalogueRepository.GetObjectByID<CohortAggregateContainer>(container2.ID));
            }
        }
    }
}

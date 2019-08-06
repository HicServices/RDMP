using System.Linq;
using NPOI.SS.Formula.Functions;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.Copying.Commands;
using Tests.Common;

namespace Rdmp.UI.Tests.CohortBuilding
{
    class CohortIdentificationConfigurationUnitTests:UITests
    {
        private void GetObjects(out Catalogue cata, out CohortIdentificationConfiguration cic)
        {
            cic = WhenIHaveA<CohortIdentificationConfiguration>();
             
            cic.CreateRootContainerIfNotExists();

            //clear anything old
            foreach (var old in cic.RootCohortAggregateContainer.GetOrderedContents().Cast<DatabaseEntity>())
                old.DeleteInDatabase();

            //we need a patient identifier column
            var ei = WhenIHaveA<ExtractionInformation>();
            ei.IsExtractionIdentifier = true;
            ei.SaveToDatabase();

            //in a catalogue
            cata = ei.CatalogueItem.Catalogue;
        }

        [Test, UITimeout(50000)]
        public void Test_AggregateConfigurationOrder_TwoAggregates()
        {
            GetObjects(out Catalogue cata, out CohortIdentificationConfiguration cic);
            
            //we should be able to add it
            var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCommand(cata),cic.RootCohortAggregateContainer);
            AssertCommandIsPossible(cmd);

            cmd.Execute();

            var ac1 = (AggregateConfiguration)(cic.RootCohortAggregateContainer.GetOrderedContents().First());
            Assert.AreEqual(0,ac1.Order);

            //add another one
            var cmd2 = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCommand(cata),cic.RootCohortAggregateContainer);
            AssertCommandIsPossible(cmd2);
            cmd2.Execute();

            //the added ones should have sensible order (no collisions)
            var all = cic.RootCohortAggregateContainer.GetOrderedContents().ToArray();
            ac1 = (AggregateConfiguration)all[0];
            var ac2 = (AggregateConfiguration)all[1];

            Assert.AreEqual(0,ac1.Order);
            Assert.AreEqual(1,ac2.Order);
        }


        [Test, UITimeout(50000)]
        public void Test_AggregateConfigurationOrder_MovingAggregatesBetweenContainers()
        {
            GetObjects(out Catalogue cata, out CohortIdentificationConfiguration cic);
            
            //we should be able to add it to root container
            var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCommand(cata),cic.RootCohortAggregateContainer);
            cmd.Execute();

            
            //create a subcontainer
            var subcontainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
            cic.RootCohortAggregateContainer.AddChild(subcontainer);

            //add the second ac to the subcontainer 
            var cmd2 = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCommand(cata),subcontainer);
            cmd2.Execute();

            //should now look like this:
            //Root
            //  INTERSECT
            //      Ac2
            //  Ac 1
            // 

            var all = cic.RootCohortAggregateContainer.GetOrderedContents().ToArray();

            var ac1 = (AggregateConfiguration) all[1];
            var ac2 = (AggregateConfiguration)subcontainer.GetOrderedContents().Single();
            var intersect = (CohortAggregateContainer) all[0];

            Assert.AreEqual(0,intersect.Order);
            Assert.AreEqual(1,ac1.Order);
            Assert.AreEqual(0,ac2.Order);

            //now move the Ac2 to Root (problematic since both Ac 2 and the INTERSECT have Order 0 - in their own separate containers)
            var cmd3 = new ExecuteCommandMoveAggregateIntoContainer(ItemActivator, new AggregateConfigurationCommand(ac2),cic.RootCohortAggregateContainer);
            cmd3.Execute();

            all = cic.RootCohortAggregateContainer.GetOrderedContents().ToArray();

            //should now look like this 
            //Root
            //  Ac2
            //  INTERSECT (empty)
            //  Ac 1

            ac2 = (AggregateConfiguration) all[0];
            intersect = (CohortAggregateContainer) all[1];
            ac1 = (AggregateConfiguration)all[2];

            Assert.AreEqual(0,ac2.Order);
            Assert.AreEqual(1,intersect.Order);
            Assert.AreEqual(2,ac1.Order);
            

        }

    }
}

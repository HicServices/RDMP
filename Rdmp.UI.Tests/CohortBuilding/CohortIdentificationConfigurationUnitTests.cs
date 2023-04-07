// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.UI.CommandExecution.AtomicCommands;

namespace Rdmp.UI.Tests.CohortBuilding;

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
        DeleteOldAggregates();

        GetObjects(out Catalogue cata, out CohortIdentificationConfiguration cic);
            
        //we should be able to add it
        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCombineable(cata),cic.RootCohortAggregateContainer);
        AssertCommandIsPossible(cmd);

        cmd.Execute();

        var ac1 = (AggregateConfiguration)(cic.RootCohortAggregateContainer.GetOrderedContents().First());
        Assert.AreEqual(0,ac1.Order);

        //add another one
        var cmd2 = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCombineable(cata),cic.RootCohortAggregateContainer);
        AssertCommandIsPossible(cmd2);
        cmd2.Execute();

        //the added ones should have sensible order (no collisions)
        var all = cic.RootCohortAggregateContainer.GetOrderedContents().ToArray();
        ac1 = (AggregateConfiguration)all[0];
        var ac2 = (AggregateConfiguration)all[1];

        Assert.AreEqual(0,ac1.Order);
        Assert.AreEqual(1,ac2.Order);

        Assert.AreEqual(2, Repository.GetAllObjects<AggregateConfiguration>().Length, "Expected you to create 2 AggregateConfiguration only");
    }


    [Test, UITimeout(50000)]
    public void Test_AggregateConfigurationOrder_MovingAggregatesBetweenContainers()
    {
        GetObjects(out Catalogue cata, out CohortIdentificationConfiguration cic);
            
        //we should be able to add it to root container
        var cmd = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCombineable(cata),cic.RootCohortAggregateContainer);
        cmd.Execute();

            
        //create a subcontainer
        var subcontainer = new CohortAggregateContainer(Repository, SetOperation.INTERSECT);
        cic.RootCohortAggregateContainer.AddChild(subcontainer);

        //add the second ac to the subcontainer 
        var cmd2 = new ExecuteCommandAddCatalogueToCohortIdentificationSetContainer(ItemActivator, new CatalogueCombineable(cata),subcontainer);
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
        var cmd3 = new ExecuteCommandMoveAggregateIntoContainer(ItemActivator, new AggregateConfigurationCombineable(ac2),cic.RootCohortAggregateContainer);
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

    private void DeleteOldAggregates()
    {
        //remove any remnants so we can count them at the end to make sure no duplicates were created
        foreach (AggregateConfiguration ac in Repository.GetAllObjects<AggregateConfiguration>())
            ac.DeleteInDatabase();

        Assert.AreEqual(0, Repository.GetAllObjects<AggregateConfiguration>().Length, "We just deleted the AggregateConfigurations why were there suddenly some in the db!?");
    }

}
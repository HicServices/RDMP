// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.Cohort;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation;

public class SimpleCohortIdentificationTests : DatabaseTests
{
    [Test]
    public void CreateNewCohortIdentificationConfiguration_SaveAndReload()
    {
        var config = new CohortIdentificationConfiguration(CatalogueRepository, "franky");

        try
        {
            Assert.That(config.Exists());
            Assert.That(config.Name, Is.EqualTo("franky"));

            config.Description = "Hi there";
            config.SaveToDatabase();


            var config2 = CatalogueRepository.GetObjectByID<CohortIdentificationConfiguration>(config.ID);
            Assert.That(config2.Description, Is.EqualTo("Hi there"));
        }
        finally
        {
            config.DeleteInDatabase();
            Assert.That(config.Exists(), Is.False);
        }
    }

    [Test]
    public void ContainerCreate()
    {
        var container = new CohortAggregateContainer(CatalogueRepository, SetOperation.UNION);

        try
        {
            Assert.That(container.Operation, Is.EqualTo(SetOperation.UNION));

            container.Operation = SetOperation.INTERSECT;
            container.SaveToDatabase();

            var container2 = CatalogueRepository.GetObjectByID<CohortAggregateContainer>(container.ID);
            Assert.That(container2.Operation, Is.EqualTo(SetOperation.INTERSECT));
        }
        finally
        {
            container.DeleteInDatabase();
        }
    }


    [Test]
    public void Container_Subcontainering()
    {
        var container = new CohortAggregateContainer(CatalogueRepository, SetOperation.UNION);

        var container2 = new CohortAggregateContainer(CatalogueRepository, SetOperation.INTERSECT);
        try
        {
            Assert.That(container.GetSubContainers(), Is.Empty);


            Assert.That(container.GetSubContainers(), Is.Empty);

            //set container to parent
            container.AddChild(container2);

            //container 1 should now contain container 2
            Assert.That(container.GetSubContainers(), Has.Length.EqualTo(1));
            Assert.That(container.GetSubContainers(), Does.Contain(container2));

            //container 2 should not have any children
            Assert.That(container2.GetSubContainers(), Is.Empty);
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
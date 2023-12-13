// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataQualityEngine.Data;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.DataQualityEngine;

public class DQEGraphAnnotationTests : DatabaseTests
{
    [Test]
    public void TestCreatingOne()
    {
        var c = new Catalogue(CatalogueRepository, "FrankyMicky");


        try
        {
            var dqeRepo = new DQERepository(CatalogueRepository);
            var evaluation = new Evaluation(dqeRepo, c);

            var annotation = new DQEGraphAnnotation(dqeRepo, 1, 2, 3, 4, "Fishesfly", evaluation,
                DQEGraphType.TimePeriodicityGraph, "ALL");

            Assert.Multiple(() =>
            {
                Assert.That(annotation.StartX, Is.EqualTo(1));
                Assert.That(annotation.StartY, Is.EqualTo(2));
                Assert.That(annotation.EndX, Is.EqualTo(3));
                Assert.That(annotation.EndY, Is.EqualTo(4));
                Assert.That(annotation.AnnotationIsForGraph, Is.EqualTo(DQEGraphType.TimePeriodicityGraph));

                //should be about 2 milliseconds ago
                Assert.That(annotation.CreationDate, Is.LessThanOrEqualTo(DateTime.Now.AddSeconds(3)));
            });
            Assert.Multiple(() =>
            {
                //certainly shouldn't be before yesterday!
                Assert.That(annotation.CreationDate, Is.GreaterThan(DateTime.Now.AddDays(-1)));

                //text should match
                Assert.That(annotation.Text, Is.EqualTo("Fishesfly"));
            });

            annotation.Text = "flibble";
            annotation.SaveToDatabase();

            annotation.Text = "";

            //new copy is flibble
            Assert.That(dqeRepo.GetObjectByID<DQEGraphAnnotation>(annotation.ID).Text, Is.EqualTo("flibble"));

            annotation.DeleteInDatabase();
        }
        finally
        {
            c.DeleteInDatabase();
        }
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Governance;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class GovernanceTests : DatabaseTests
{
    [OneTimeTearDown]
    protected void OneTimeTearDown()
    {
        //delete all governance periods
        foreach (var governancePeriod in toCleanup)
            try
            {
                governancePeriod.DeleteInDatabase();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ignoring exception {e.Message} during clean up");
            }
    }

    [Test]
    public void TestCreatingGovernance_StartsAtToday()
    {
        var gov = GetGov();

        Assert.Multiple(() =>
        {
            Assert.That(gov, Is.Not.Null);
            Assert.That(DateTime.Now.Date, Is.EqualTo(gov.StartDate));
        });
    }

    [Test]
    public void TestCreatingGovernance_ChangeName()
    {
        if (CatalogueRepository is not TableRepository)
            Assert.Inconclusive("This test for stale objects only applies to database repositories");

        var gov = GetGov();
        gov.Name = "Fish";
        var freshCopy = CatalogueRepository.GetObjectByID<GovernancePeriod>(gov.ID);

        //local change not applied yet
        Assert.That(freshCopy.Name, Is.Not.EqualTo(gov.Name));

        //committed change to database
        gov.SaveToDatabase();

        //notice that this fresh copy is still desynced
        Assert.That(freshCopy.Name, Is.Not.EqualTo(gov.Name));

        //sync it
        freshCopy = CatalogueRepository.GetObjectByID<GovernancePeriod>(gov.ID);
        Assert.That(freshCopy.Name, Is.EqualTo(gov.Name));
    }

    [Test]
    public void TestCreatingGovernance_CannotHaveSameNames()
    {
        var gov1 = GetGov();
        var gov2 = GetGov();

        gov1.Name = "HiDuplicate";
        gov1.SaveToDatabase();

        gov2.Name = "HiDuplicate";

        if (CatalogueRepository is TableRepository)
        {
            var ex = Assert.Throws<SqlException>(gov2.SaveToDatabase);
            Assert.That(
                ex?.Message, Does.StartWith("Cannot insert duplicate key row in object 'dbo.GovernancePeriod' with unique index 'idxGovernancePeriodNameMustBeUnique'. The duplicate key value is (HiDuplicate)"));
        }
    }

    [Test]
    public void Checkability_ExpiresBeforeStarts()
    {
        var gov = GetGov();
        gov.Name = "TestExpiryBeforeStarting";

        //valid to start with
        gov.Check(ThrowImmediatelyCheckNotifier.Quiet);

        gov.EndDate = DateTime.MinValue;
        var ex = Assert.Throws<Exception>(() =>
            gov.Check(ThrowImmediatelyCheckNotifier
                .Quiet)); //no longer valid - notice there is no SaveToDatabase because we can shouldn't be going back to db anyway
        Assert.That(ex?.Message, Is.EqualTo("GovernancePeriod TestExpiryBeforeStarting expires before it begins!"));
    }

    [Test]
    public void Checkability_NoExpiryDateWarning()
    {
        var gov = GetGov();
        gov.Name = "NeverExpires";

        //valid to start with
        var ex = Assert.Throws<Exception>(() => gov.Check(ThrowImmediatelyCheckNotifier.QuietPicky));
        Assert.That(ex?.Message, Is.EqualTo("There is no end date for GovernancePeriod NeverExpires"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void GovernsCatalogue(bool memoryRepository)
    {
        var repo = memoryRepository ? (ICatalogueRepository)new MemoryCatalogueRepository() : CatalogueRepository;

        var gov = GetGov(repo);
        var c = new Catalogue(repo, "GovernedCatalogue");
        try
        {
            Assert.That(gov.GovernedCatalogues.Count(), Is.EqualTo(0));

            //should be no governanced catalogues for this governancer yet
            gov.CreateGovernanceRelationshipTo(c);

            var allCatalogues = gov.GovernedCatalogues.ToArray();
            var governedCatalogue = allCatalogues[0];
            Assert.That(c, Is.EqualTo(governedCatalogue)); //we now govern C
        }
        finally
        {
            gov.DeleteGovernanceRelationshipTo(c);
            Assert.That(gov.GovernedCatalogues.Count(), Is.EqualTo(0)); //we govern c nevermore!

            c.DeleteInDatabase();
        }
    }

    [Test]
    public void GovernsSameCatalogueTwice()
    {
        var c = new Catalogue(CatalogueRepository, "GovernedCatalogue");

        var gov = GetGov();
        Assert.That(gov.GovernedCatalogues.Count(), Is.EqualTo(0)); //should be no governanced catalogues for this governancer yet

        gov.CreateGovernanceRelationshipTo(c);
        gov.CreateGovernanceRelationshipTo(c);
    }


    private List<GovernancePeriod> toCleanup = new();

    private GovernancePeriod GetGov(ICatalogueRepository repo = null)
    {
        var gov = new GovernancePeriod(repo ?? CatalogueRepository);
        toCleanup.Add(gov);

        return gov;
    }
}
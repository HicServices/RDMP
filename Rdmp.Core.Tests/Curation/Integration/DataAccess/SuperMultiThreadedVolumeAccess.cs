// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using FAnsi.Connections;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.DataAccess;

public class SuperMultiThreadedVolumeAccess : DatabaseTests
{
    [OneTimeSetUp]
    protected override void SetUp()
    {
        base.SetUp();

        var timeoutBefore = DatabaseCommandHelper.GlobalTimeout;
        DatabaseCommandHelper.GlobalTimeout = 60;

        foreach (
            var catalogue in
            CatalogueRepository.GetAllObjects<Catalogue>()
                .Where(c => c.Name.StartsWith("SuperMultiThreadedTestCatalogue", StringComparison.Ordinal)))
            catalogue.DeleteInDatabase();

        DatabaseCommandHelper.GlobalTimeout = timeoutBefore;
    }

    [OneTimeTearDown]
    protected void DeleteRemnants()
    {
        foreach (
            var catalogue in
            CatalogueRepository.GetAllObjects<Catalogue>()
                .Where(c => c.Name.StartsWith("SuperMultiThreadedTestCatalogue", StringComparison.Ordinal)))
            catalogue.DeleteInDatabase();
    }


    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SingleThreadedBulkCatalogueCreation(bool useTransactions)
    {
        IManagedConnection c = null;


        if (CatalogueRepository is not TableRepository && useTransactions)
            Assert.Inconclusive("YamlRepository does not support transactions so don't test this");

        if (useTransactions) c = CatalogueTableRepository.BeginNewTransactedConnection();

        using (c)
        {
            //create lots of catalogues
            for (var i = 0; i < 30; i++)
            {
                var cata = new Catalogue(CatalogueRepository, $"SuperMultiThreadedTestCatalogue{Guid.NewGuid()}");
                var copy = CatalogueRepository.GetObjectByID<Catalogue>(cata.ID);

                copy.Description = "fish";
                Assert.That(copy.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.DatabaseCopyDifferent));

                copy.SaveToDatabase();
                Assert.That(copy.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges));
            }

            //now fetch them out of database lots of times
            for (var i = 0; i < 100; i++)
                CatalogueRepository.GetAllObjects<Catalogue>();

            if (useTransactions)
                CatalogueTableRepository.EndTransactedConnection(false);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void MultiThreaded(bool useTransactions)
    {
        FireMultiThreaded(SingleThreadedBulkCatalogueCreation, 5, useTransactions);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SimpleCaseSingleThreaded(bool useTransaction)
    {
        using var con = useTransaction
            ? CatalogueTableRepository.BeginNewTransactedConnection()
            : CatalogueTableRepository.GetConnection();
        Assert.That(con.Connection.State, Is.EqualTo(ConnectionState.Open));
        Thread.Sleep(1000);

        if (useTransaction)
            CatalogueTableRepository.EndTransactedConnection(false);
        else
            con.Connection.Close();

        Assert.That(con.Connection.State, Is.EqualTo(ConnectionState.Closed));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SimpleCaseMultiThreaded(bool useTransactions)
    {
        FireMultiThreaded(SimpleCaseSingleThreaded, 50, useTransactions);
    }

    private void FireMultiThreaded(Action<bool> method, int numberToFire, bool useTransactions)
    {
        if (CatalogueRepository is not TableRepository)
            Assert.Inconclusive("We don't have to test this for yaml repos");


        var exes = new List<Exception>();


        var ts = new List<Thread>();

        for (var i = 0; i < numberToFire; i++)
        {
            var i1 = i;
            ts.Add(new Thread(() =>
            {
                try
                {
                    method(useTransactions && i1 == 0);
                }
                catch (Exception ex)
                {
                    exes.Add(ex);
                }
            }));
        }

        foreach (var thread in ts)
            thread.Start();

        while (ts.Any(t => t.IsAlive))
            ts.FirstOrDefault(t => t.IsAlive)?.Join(100);

        Assert.That(exes, Is.Empty);
    }
}
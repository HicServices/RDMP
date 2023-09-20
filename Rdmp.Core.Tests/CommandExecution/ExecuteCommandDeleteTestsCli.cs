// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using System;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandDeleteTestsCli : CommandCliTests
{
    [Test]
    public void TestDeletingACatalogue_NoneInDbIsFine()
    {
        var prev = RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
        Assert.IsEmpty(prev);

        Assert.AreEqual(0, Run("delete", "Catalogue:bob"));

        var now = RepositoryLocator.CatalogueRepository.GetAllObjects<Catalogue>();
        Assert.IsEmpty(now);
    }

    [Test]
    public void TestDeletingACatalogue_DeleteBecauseMatches()
    {
        var cata = WhenIHaveA<Catalogue>();
        cata.Name = "bob";

        Assert.AreEqual(0, Run("delete", "Catalogue:bob"));
        Assert.IsFalse(cata.Exists());
    }

    [Test]
    public void TestDeletingACatalogue_DoesNotMatchPattern()
    {
        var cata = WhenIHaveA<Catalogue>();
        cata.Name = "ffff";

        Assert.AreEqual(0, Run("delete", "Catalogue:bob"));

        // should not have been deleted because name does not match what is sought to be deleted
        Assert.IsTrue(cata.Exists());

        //cleanup
        cata.DeleteInDatabase();
    }

    [Test]
    public void TestDeleteMany_ThrowsBecauseNotExpected()
    {
        // 2 catalogues
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();

        // delete all catalogues
        var ex = Assert.Throws<Exception>(() => Run("delete", "Catalogue"));

        Assert.AreEqual(ex.Message,
            "Allow delete many is false but multiple objects were matched for deletion (Mycata,Mycata)");

        c1.DeleteInDatabase();
        c2.DeleteInDatabase();
    }

    [Test]
    public void TestDeleteMany_Allowed()
    {
        // 2 catalogues
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();

        // delete all catalogues
        Assert.AreEqual(0, Run("delete", "Catalogue", "true"));

        Assert.IsFalse(c1.Exists());
        Assert.IsFalse(c2.Exists());
    }

    [Test]
    public void TestDeleteMany_BadParameterFormat()
    {
        var c1 = WhenIHaveA<Catalogue>();

        // delete all catalogues
        var ex = Assert.Throws<Exception>(() => Run("delete", "Catalogue", "FLIBBLE!"));

        Assert.AreEqual(
            "Expected parameter at index 1 to be a System.Boolean (for parameter 'deleteMany') but it was FLIBBLE!",
            ex.Message);

        c1.DeleteInDatabase();
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandSimilarTests : CommandCliTests
{
    [Test]
    public void FindSameName_MixedCaps()
    {
        var cata1 = new Catalogue(Repository, "Bob");
        var cata2 = new Catalogue(Repository, "bob");

        var activator = new ThrowImmediatelyActivator(RepositoryLocator);
        var cmd = new ExecuteCommandSimilar(activator, cata1, false);

        Assert.AreEqual(cata2, cmd.Matched.Single());

        cata1.DeleteInDatabase();
        cata2.DeleteInDatabase();
    }

    [Test]
    public void FindDifferent_ColumnInfosSame()
    {
        var c1 = WhenIHaveA<ColumnInfo>();
        var c2 = WhenIHaveA<ColumnInfo>();

        var activator = new ThrowImmediatelyActivator(RepositoryLocator);
        var cmd = new ExecuteCommandSimilar(activator, c1, true);

        Assert.IsEmpty(cmd.Matched);

        c1.DeleteInDatabase();
        c2.DeleteInDatabase();
    }

    [Test]
    public void FindDifferent_ColumnInfosDiffer_OnType()
    {
        var c1 = WhenIHaveA<ColumnInfo>();
        c1.Data_type = "varchar(10)";

        var c2 = WhenIHaveA<ColumnInfo>();
        c2.Data_type = "varchar(20)";

        var activator = new ThrowImmediatelyActivator(RepositoryLocator);
        var cmd = new ExecuteCommandSimilar(activator, c1, true);

        Assert.AreEqual(c2, cmd.Matched.Single());

        c1.DeleteInDatabase();
        c2.DeleteInDatabase();
    }

    [Test]
    public void FindDifferent_ColumnInfosDiffer_OnCollation()
    {
        var c1 = WhenIHaveA<ColumnInfo>();
        c1.Collation = "troll doll";

        var c2 = WhenIHaveA<ColumnInfo>();
        c2.Collation = "durdur";

        var activator = new ThrowImmediatelyActivator(RepositoryLocator);
        var cmd = new ExecuteCommandSimilar(activator, c1, true);

        Assert.AreEqual(c2, cmd.Matched.Single());

        c1.DeleteInDatabase();
        c2.DeleteInDatabase();
    }
}
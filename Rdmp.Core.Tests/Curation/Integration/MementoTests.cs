// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class MementoTests : DatabaseTests
{
    [Test]
    public void FakeMemento_Catalogue_Modify()
    {
        var c = new Catalogue(CatalogueRepository, "Hey");

        var g = Guid.NewGuid();
        var commit = new Commit(CatalogueRepository, g, "Breaking stuff!");

        var mem = new Memento(CatalogueRepository, commit, MementoType.Modify, c, "yar", "blerg");
        mem.SaveToDatabase();

        mem.BeforeYaml = "haha";
        Assert.That(mem.BeforeYaml, Is.EqualTo("haha"));
        mem.RevertToDatabaseState();
        Assert.That(mem.BeforeYaml, Is.EqualTo("yar"));

        var mem2 = CatalogueRepository.GetObjectByID<Memento>(mem.ID);

        Assert.Multiple(() =>
        {
            Assert.That(new Guid(mem2.Commit.Transaction), Is.EqualTo(g));
            Assert.That(mem2.AfterYaml, Is.EqualTo("blerg"));
            Assert.That(mem2.BeforeYaml, Is.EqualTo("yar"));
            Assert.That(mem2.Type, Is.EqualTo(MementoType.Modify));
            Assert.That(mem2.Commit.Username, Is.EqualTo(Environment.UserName));
            Assert.That(mem2.GetReferencedObject(RepositoryLocator), Is.EqualTo(c));
        });
    }


    [Test]
    public void FakeMemento_Catalogue_Add()
    {
        var c = new Catalogue(CatalogueRepository, "Hey");

        var g = Guid.NewGuid();
        var commit = new Commit(CatalogueRepository, g, "Breaking stuff!");

        var mem = new Memento(CatalogueRepository, commit, MementoType.Add, c, null, "blerg");
        mem.SaveToDatabase();

        foreach (var check in new[] { mem, CatalogueRepository.GetObjectByID<Memento>(mem.ID) })
        {
            Assert.Multiple(() =>
            {
                Assert.That(check.BeforeYaml, Is.Null);
                Assert.That(new Guid(check.Commit.Transaction), Is.EqualTo(g));
                Assert.That(check.AfterYaml, Is.EqualTo("blerg"));
                Assert.That(check.Type, Is.EqualTo(MementoType.Add));
                Assert.That(check.Commit.Username, Is.EqualTo(Environment.UserName));
                Assert.That(check.GetReferencedObject(RepositoryLocator), Is.EqualTo(c));
            });
        }
    }

    [Test]
    public void FakeMemento_Catalogue_Delete()
    {
        var c = new Catalogue(CatalogueRepository, "Hey");

        var g = Guid.NewGuid();
        var commit = new Commit(CatalogueRepository, g, "Breaking stuff!");

        var mem = new Memento(CatalogueRepository, commit, MementoType.Delete, c, "blah", null);
        mem.SaveToDatabase();

        foreach (var check in new[] { mem, CatalogueRepository.GetObjectByID<Memento>(mem.ID) })
        {
            Assert.Multiple(() =>
            {
                Assert.That(check.AfterYaml, Is.Null);
                Assert.That(new Guid(check.Commit.Transaction), Is.EqualTo(g));
                Assert.That(check.BeforeYaml, Is.EqualTo("blah"));
                Assert.That(check.Type, Is.EqualTo(MementoType.Delete));
                Assert.That(check.Commit.Username, Is.EqualTo(Environment.UserName));
                Assert.That(check.GetReferencedObject(RepositoryLocator), Is.EqualTo(c));
            });
        }
    }
}
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
        var c = new Catalogue(CatalogueRepository,"Hey");

        var g = Guid.NewGuid();
        var commit = new Commit(CatalogueRepository, g, "Breaking stuff!");

        var mem = new Memento(CatalogueRepository,commit,MementoType.Modify,c,"yar","blerg");
        mem.SaveToDatabase();

        mem.BeforeYaml = "haha";
        Assert.AreEqual("haha", mem.BeforeYaml);
        mem.RevertToDatabaseState();
        Assert.AreEqual("yar", mem.BeforeYaml);

        var mem2 = CatalogueRepository.GetObjectByID<Memento>(mem.ID);

        Assert.AreEqual(g, new Guid(mem2.Commit.Transaction));
        Assert.AreEqual("blerg", mem2.AfterYaml);
        Assert.AreEqual("yar", mem2.BeforeYaml);
        Assert.AreEqual(MementoType.Modify, mem2.Type);
        Assert.AreEqual(Environment.UserName, mem2.Commit.Username);
        Assert.AreEqual(c, mem2.GetReferencedObject(RepositoryLocator)); 
    }


    [Test]
    public void FakeMemento_Catalogue_Add()
    {
        var c = new Catalogue(CatalogueRepository, "Hey");

        var g = Guid.NewGuid();
        var commit = new Commit(CatalogueRepository, g, "Breaking stuff!");

        var mem = new Memento(CatalogueRepository, commit, MementoType.Add, c, null, "blerg");
        mem.SaveToDatabase();

        foreach(var check in new[] { mem, CatalogueRepository.GetObjectByID<Memento>(mem.ID) })
        {
            Assert.IsNull(check.BeforeYaml);
            Assert.AreEqual(g, new Guid(check.Commit.Transaction));
            Assert.AreEqual("blerg", check.AfterYaml);
            Assert.AreEqual(MementoType.Add, check.Type);
            Assert.AreEqual(Environment.UserName, check.Commit.Username);
            Assert.AreEqual(c, check.GetReferencedObject(RepositoryLocator));
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
            Assert.IsNull(check.AfterYaml);
            Assert.AreEqual(g, new Guid(check.Commit.Transaction));
            Assert.AreEqual("blah", check.BeforeYaml);
            Assert.AreEqual(MementoType.Delete, check.Type);
            Assert.AreEqual(Environment.UserName, check.Commit.Username);
            Assert.AreEqual(c, check.GetReferencedObject(RepositoryLocator));
        }
    }
}
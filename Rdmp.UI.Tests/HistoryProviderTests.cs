// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataExport.Data;
using Rdmp.UI.Collections.Providers;
using Tests.Common;

namespace Rdmp.UI.Tests;

internal class HistoryProviderTests : UnitTests
{
    protected override void SetUp()
    {
        base.SetUp();

        SetupMEF();
    }

    [Test]
    public void TestHistoryProvider_DuplicateAdding()
    {
        var c = WhenIHaveA<Catalogue>();

        var provider = new HistoryProvider(RepositoryLocator);
        provider.Clear();

        Assert.IsEmpty(provider.History);

        provider.Add(c);

        Assert.AreEqual(1,provider.History.Count);

        provider.Add(c);

        Assert.AreEqual(1,provider.History.Count);

        provider.Add(c);

        Assert.AreEqual(1,provider.History.Count);

        var p = WhenIHaveA<Project>();
            
        provider.Add(p);

        Assert.AreEqual(2,provider.History.Count);
    }
    [Test]
    public void TestHistoryProvider_Persist()
    {
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();
        var c3 = WhenIHaveA<Catalogue>();
        var c4 = WhenIHaveA<Catalogue>();

        var provider = new HistoryProvider(RepositoryLocator);
        provider.Clear();
        Assert.IsEmpty(provider.History);

        provider.History.Add(new HistoryEntry(c1,new DateTime(2001,01,01)));
        provider.History.Add(new HistoryEntry(c2,new DateTime(2002,02,01)));
        provider.History.Add(new HistoryEntry(c3,new DateTime(2003,03,01)));
        provider.History.Add(new HistoryEntry(c4,new DateTime(2004,01,04)));

        provider.Save(2);
        Assert.AreEqual(2,provider.History.Count);

        Assert.AreEqual(c4,provider.History[0].Object);
        Assert.AreEqual(c3,provider.History[1].Object);
        Assert.AreEqual(new DateTime(2004,01,04),provider.History[0].Date);
        Assert.AreEqual(new DateTime(2003,03,01),provider.History[1].Date);
            

        var provider2 = new HistoryProvider(RepositoryLocator);

        Assert.AreEqual(c4,provider2.History[0].Object);
        Assert.AreEqual(c3,provider2.History[1].Object);
        Assert.AreEqual(new DateTime(2004,01,04),provider2.History[0].Date);
        Assert.AreEqual(new DateTime(2003,03,01),provider2.History[1].Date);

    }
    [Test]
    public void TestHistoryProvider_PersistByType()
    {
        var c1 = WhenIHaveA<Catalogue>();
        var c2 = WhenIHaveA<Catalogue>();
        var lmd1 = WhenIHaveA<LoadMetadata>();
        var lmd2 = WhenIHaveA<LoadMetadata>();

        var provider = new HistoryProvider(RepositoryLocator);
        provider.Clear();
        Assert.IsEmpty(provider.History);

        //these were viewed recently
        provider.History.Add(new HistoryEntry(c1,new DateTime(2001,01,01)));
        provider.History.Add(new HistoryEntry(c2,new DateTime(2002,02,02)));

        //these were viewed ages ago
        provider.History.Add(new HistoryEntry(lmd1,new DateTime(1999,01,01)));
        provider.History.Add(new HistoryEntry(lmd2,new DateTime(2000,02,03)));

        //persist only 1 entry (by Type)
        provider.Save(1);
        Assert.AreEqual(2,provider.History.Count);

        Assert.AreEqual(c2,provider.History[0].Object);
        Assert.AreEqual(lmd2,provider.History[1].Object);
            
        var provider2 = new HistoryProvider(RepositoryLocator);
            
        Assert.AreEqual(c2,provider2.History[0].Object);
        Assert.AreEqual(lmd2,provider2.History[1].Object);
    }
}
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
    [Test]
    public void TestHistoryProvider_DuplicateAdding()
    {
        var c = WhenIHaveA<Catalogue>();

        var provider = new HistoryProvider(RepositoryLocator);
        provider.Clear();

        Assert.That(provider.History, Is.Empty);

        provider.Add(c);

        Assert.That(provider.History, Has.Count.EqualTo(1));

        provider.Add(c);

        Assert.That(provider.History, Has.Count.EqualTo(1));

        provider.Add(c);

        Assert.That(provider.History, Has.Count.EqualTo(1));

        var p = WhenIHaveA<Project>();

        provider.Add(p);

        Assert.That(provider.History, Has.Count.EqualTo(2));
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
        Assert.That(provider.History, Is.Empty);

        provider.History.Add(new HistoryEntry(c1, new DateTime(2001, 01, 01)));
        provider.History.Add(new HistoryEntry(c2, new DateTime(2002, 02, 01)));
        provider.History.Add(new HistoryEntry(c3, new DateTime(2003, 03, 01)));
        provider.History.Add(new HistoryEntry(c4, new DateTime(2004, 01, 04)));

        provider.Save(2);
        Assert.That(provider.History, Has.Count.EqualTo(2));

        Assert.That(provider.History[0].Object, Is.EqualTo(c4));
        Assert.That(provider.History[1].Object, Is.EqualTo(c3));
        Assert.That(provider.History[0].Date, Is.EqualTo(new DateTime(2004, 01, 04)));
        Assert.That(provider.History[1].Date, Is.EqualTo(new DateTime(2003, 03, 01)));


        var provider2 = new HistoryProvider(RepositoryLocator);

        Assert.That(provider2.History[0].Object, Is.EqualTo(c4));
        Assert.That(provider2.History[1].Object, Is.EqualTo(c3));
        Assert.That(provider2.History[0].Date, Is.EqualTo(new DateTime(2004, 01, 04)));
        Assert.That(provider2.History[1].Date, Is.EqualTo(new DateTime(2003, 03, 01)));
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
        Assert.That(provider.History, Is.Empty);

        //these were viewed recently
        provider.History.Add(new HistoryEntry(c1, new DateTime(2001, 01, 01)));
        provider.History.Add(new HistoryEntry(c2, new DateTime(2002, 02, 02)));

        //these were viewed ages ago
        provider.History.Add(new HistoryEntry(lmd1, new DateTime(1999, 01, 01)));
        provider.History.Add(new HistoryEntry(lmd2, new DateTime(2000, 02, 03)));

        //persist only 1 entry (by Type)
        provider.Save(1);
        Assert.That(provider.History, Has.Count.EqualTo(2));

        Assert.That(provider.History[0].Object, Is.EqualTo(c2));
        Assert.That(provider.History[1].Object, Is.EqualTo(lmd2));

        var provider2 = new HistoryProvider(RepositoryLocator);

        Assert.That(provider2.History[0].Object, Is.EqualTo(c2));
        Assert.That(provider2.History[1].Object, Is.EqualTo(lmd2));
    }
}
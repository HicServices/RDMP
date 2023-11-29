// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers;

internal class RowVerTest : DatabaseTests
{
    [Test]
    public void Test_RowVer()
    {
        if (CatalogueRepository is not TableRepository)
            throw new InconclusiveException("RowVer system only applies when using Database back repository");

        var cata = new Catalogue(CatalogueRepository, "FFFF");

        //When we get all the Catalogues we should include cata
        Assert.That(CatalogueRepository.GetAllObjects<Catalogue>(), Does.Contain(cata));
        Assert.That(CatalogueRepository.GetAllObjects<Catalogue>()[0], Is.EqualTo(cata));
        Assert.That(CatalogueRepository.GetAllObjects<Catalogue>()[0], Is.Not.SameAs(cata));

        //create a cache
        var rowVerCache = new RowVerCache<Catalogue>(CatalogueTableRepository);

        //should fill the cache
        cata = rowVerCache.GetAllObjects()[0];

        //should return the same instance
        Assert.That(rowVerCache.GetAllObjects()[0], Is.SameAs(cata));

        cata.DeleteInDatabase();
        Assert.That(rowVerCache.GetAllObjects(), Is.Empty);

        //create some catalogues
        new Catalogue(CatalogueRepository, "1");
        new Catalogue(CatalogueRepository, "2");
        new Catalogue(CatalogueRepository, "3");

        //fill up the cache
        rowVerCache.GetAllObjects();

        //give it a fresh object
        var cata2 = new Catalogue(CatalogueRepository, "dddd");

        //fresh fetch for this
        Assert.That(rowVerCache.GetAllObjects(), Does.Contain(cata2));
        Assert.That(rowVerCache.GetAllObjects().Contains(cata), Is.False);

        //change a value in the background but first make sure that what the cache has is a Equal but not reference to cata2
        Assert.That(rowVerCache.GetAllObjects().Any(o => ReferenceEquals(o, cata2)), Is.False);
        Assert.That(rowVerCache.GetAllObjects().Any(o => Equals(o, cata2)));

        cata2.Name = "boom";
        cata2.SaveToDatabase();

        Assert.That(rowVerCache.GetAllObjects().Count(static c => c.Name.Equals("boom")), Is.EqualTo(1));

        cata2.Name = "vroom";
        cata2.SaveToDatabase();

        Assert.That(rowVerCache.GetAllObjects().Count(c => c.Name.Equals("vroom")), Is.EqualTo(1));

        Assert.That(rowVerCache.GetAllObjects().Count(c => c.Name.Equals("vroom")), Is.EqualTo(1));

        Assert.That(rowVerCache.Broken, Is.False);
    }
}
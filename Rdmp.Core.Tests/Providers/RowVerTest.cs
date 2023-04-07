// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers;

class RowVerTest : DatabaseTests
{
    [Test]
    public void Test_RowVer()
    {
        if (CatalogueRepository is not TableRepository)
            throw new InconclusiveException("RowVer system only applies when using Database back repository");

        var cata = new Catalogue(CatalogueRepository, "FFFF");

        //When we get all the Catalogues we should include cata
        Assert.Contains(cata,CatalogueRepository.GetAllObjects<Catalogue>());
        Assert.AreEqual(cata,CatalogueRepository.GetAllObjects<Catalogue>()[0]);
        Assert.AreNotSame(cata,CatalogueRepository.GetAllObjects<Catalogue>()[0]);

        //create a cache
        var rowVerCache = new RowVerCache<Catalogue>(CatalogueTableRepository);

        //should fill the cache
        cata = rowVerCache.GetAllObjects()[0];
            
        //should return the same instance
        Assert.AreSame(cata, rowVerCache.GetAllObjects()[0]);

        cata.DeleteInDatabase();
        Assert.IsEmpty(rowVerCache.GetAllObjects());
            
        //create some catalogues
        new Catalogue(CatalogueRepository, "1");
        new Catalogue(CatalogueRepository, "2");
        new Catalogue(CatalogueRepository, "3");

        //fill up the cache
        rowVerCache.GetAllObjects();

        //give it a fresh object
        var cata2 = new Catalogue(CatalogueRepository, "dddd");
            
        //fresh fetch for this
        Assert.Contains(cata2,rowVerCache.GetAllObjects());
        Assert.IsFalse(rowVerCache.GetAllObjects().Contains(cata));
            
        //change a value in the background but first make sure that what the cache has is a Equal but not reference to cata2
        Assert.IsFalse(rowVerCache.GetAllObjects().Any(o=>ReferenceEquals(o,cata2)));
        Assert.IsTrue(rowVerCache.GetAllObjects().Any(o=>Equals(o,cata2)));

        cata2.Name = "boom";
        cata2.SaveToDatabase();
            
        Assert.AreEqual(1,rowVerCache.GetAllObjects().Count(c=>c.Name.Equals("boom")));
            
        cata2.Name = "vroom";
        cata2.SaveToDatabase();
            
        Assert.AreEqual(1,rowVerCache.GetAllObjects().Count(c=>c.Name.Equals("vroom")));

        Assert.AreEqual(1,rowVerCache.GetAllObjects().Count(c=>c.Name.Equals("vroom")));

        Assert.IsFalse(rowVerCache.Broken);
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.Providers
{
    class RowVerTest : DatabaseTests
    {
        [Test]
        public void Test_RowVer()
        {
            var cata = new Catalogue(CatalogueRepository, "FFFF");

            //When we get all the Catalogues we should include cata
            Assert.Contains(cata,CatalogueRepository.GetAllObjects<Catalogue>());
            Assert.AreEqual(cata,CatalogueRepository.GetAllObjects<Catalogue>()[0]);
            Assert.AreNotSame(cata,CatalogueRepository.GetAllObjects<Catalogue>()[0]);

            //create a cache
            var rowVerCache = new RowVerCache<Catalogue>(CatalogueRepository);
            List<Catalogue> deleted;
            List<Catalogue> added;
            List<Catalogue> changed;

            //should fill the cache
            cata = rowVerCache.GetAllObjects(out _, out added, out changed)[0];
            Assert.Contains(cata,added);
            
            //should return the same instance
            Assert.AreSame(cata, rowVerCache.GetAllObjects(out deleted, out added, out changed)[0]);
            Assert.IsEmpty(deleted);

            cata.DeleteInDatabase();
            Assert.IsEmpty(rowVerCache.GetAllObjects(out deleted, out added, out changed));
            Assert.AreSame(cata,deleted[0]);
            
            //create some catalogues
            new Catalogue(CatalogueRepository, "1");
            new Catalogue(CatalogueRepository, "2");
            new Catalogue(CatalogueRepository, "3");

            //fill up the cache
            rowVerCache.GetAllObjects(out deleted, out added, out changed);

            //give it a fresh object
            var cata2 = new Catalogue(CatalogueRepository, "dddd");
            
            //fresh fetch for this
            Assert.Contains(cata2,rowVerCache.GetAllObjects(out deleted, out added, out changed));
            Assert.AreEqual(cata2,added.Single());
            Assert.IsFalse(rowVerCache.GetAllObjects(out deleted, out added, out changed).Contains(cata));
            Assert.IsEmpty(deleted);

            //change a value in the background but first make sure that what the cache has is a Equal but not reference to cata2
            Assert.IsFalse(rowVerCache.GetAllObjects(out deleted, out added, out changed).Any(o=>ReferenceEquals(o,cata2)));
            Assert.IsTrue(rowVerCache.GetAllObjects(out deleted, out added, out changed).Any(o=>Equals(o,cata2)));

            cata2.Name = "boom";
            cata2.SaveToDatabase();
            
            Assert.AreEqual(1,rowVerCache.GetAllObjects(out deleted, out added, out changed).Count(c=>c.Name.Equals("boom")));
            Assert.AreEqual(1,changed.Count);
            
            cata2.Name = "vroom";
            cata2.SaveToDatabase();
            
            Assert.AreEqual(1,rowVerCache.GetAllObjects(out deleted, out added, out changed).Count(c=>c.Name.Equals("vroom")));
            Assert.AreEqual(1,changed.Count);

            Assert.AreEqual(1,rowVerCache.GetAllObjects(out deleted, out added, out changed).Count(c=>c.Name.Equals("vroom")));
            Assert.AreEqual(0,changed.Count);
        }
    }
}

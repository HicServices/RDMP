// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

class ExtractionFilterParameterSetTests : DatabaseTests
{
    [Test]
    public void ExtractionFilterParameterSet_Deleting()
    {
        var cata = new Catalogue(CatalogueRepository, "myCata");
        var cataItem = new CatalogueItem(CatalogueRepository, cata, "MyCol");

        var table = new TableInfo(CatalogueRepository, "myTbl");
        var col = new ColumnInfo(CatalogueRepository, "myCol", "varchar(10)", table);

        var ei = new ExtractionInformation(CatalogueRepository, cataItem, col, "[myTbl].[mycol]");
        var filter = new ExtractionFilter(CatalogueRepository, "Age", ei);
        filter.WhereSQL = "Age >= @age";
        new ExtractionFilterParameter(CatalogueRepository, "DECLARE @age int", filter);

        var paramSet = new ExtractionFilterParameterSet(CatalogueRepository, filter, "Old");
        var vals = paramSet.CreateNewValueEntries();

        Assert.AreEqual(1, vals.Length);
        Assert.IsTrue(vals[0].Exists());

        paramSet.DeleteInDatabase();
        Assert.IsFalse(paramSet.Exists());
        Assert.IsFalse(vals[0].Exists());
    }
}
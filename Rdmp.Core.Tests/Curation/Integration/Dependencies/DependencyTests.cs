// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.Dependencies;

public class DependencyTests : DatabaseTests
{
    [Test]
    public void ExtractionInformationTriangle()
    {
        var t = new TableInfo(CatalogueRepository, "t");
        var col = new ColumnInfo(CatalogueRepository, "mycol", "varchar(10)", t);

        var cat = new Catalogue(CatalogueRepository, "MyCat");
        var ci = new CatalogueItem(CatalogueRepository, cat, "myci");


        try
        {
            //col depends on tr
            Assert.Contains(t,col.GetObjectsThisDependsOn());
            Assert.Contains(col,t.GetObjectsDependingOnThis());

            //catalogue depends on catalogue items existing (slightly counter intuitive but think of it as data flow out of technical low level data through transforms into datasets - and then into researchers and research projects)
            Assert.Contains(cat,ci.GetObjectsDependingOnThis());
            Assert.Contains(ci,cat.GetObjectsThisDependsOn());

            //catalogue item should not be relying on anything currently (no link to underlying technical data)
            Assert.IsNull(ci.GetObjectsThisDependsOn());

            //now they are associated so the ci should be dependent on the col
            ci.SetColumnInfo(col);
            Assert.Contains(col, ci.GetObjectsDependingOnThis());

        }
        finally
        {
            t.DeleteInDatabase();
            cat.DeleteInDatabase();
        }
    }
}
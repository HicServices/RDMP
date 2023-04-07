// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.EntityNaming;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

class TableNamingConventionTests : DatabaseTests
{
    [Test]
    public void GetAllTableInfos_moreThan1_pass()
    {
        var ti = new TableInfo(CatalogueRepository, "AMAGAD!!!");
        Assert.IsTrue(CatalogueRepository.GetAllObjects<TableInfo>().Any());
        ti.DeleteInDatabase();
    }


    [Test]
    public void update_changeAllProperties_pass()
    {
        var tableInfo = new TableInfo(CatalogueRepository, "CHI_AMALG..SearchStuff")
        {
            Database = "CHI_AMALG",
            Server = "Highly restricted",
            Name = "Fishmongery!",
            DatabaseType = DatabaseType.Oracle
        };

        tableInfo.SaveToDatabase();

        var tableInfoAfter = CatalogueRepository.GetObjectByID<TableInfo>(tableInfo.ID);

        Assert.IsTrue(tableInfoAfter.Database == "CHI_AMALG");
        Assert.IsTrue(tableInfoAfter.Server == "Highly restricted");
        Assert.IsTrue(tableInfoAfter.Name == "Fishmongery!");
        Assert.IsTrue(tableInfoAfter.DatabaseType == DatabaseType.Oracle);

        tableInfoAfter.DeleteInDatabase();
            
    }

    [Test]
    public void SuffixBasedTableNamingConventionHelper()
    {
        const string baseTableName = "MyTable";
        var namingScheme = new SuffixBasedNamer();

        var stagingTable = namingScheme.GetName(baseTableName, LoadBubble.Staging);
        Assert.AreEqual("MyTable_STAGING", stagingTable);

        var newLookupTable = namingScheme.GetName(baseTableName, LoadBubble.Live);
        Assert.AreEqual("MyTable", newLookupTable);
    }

}
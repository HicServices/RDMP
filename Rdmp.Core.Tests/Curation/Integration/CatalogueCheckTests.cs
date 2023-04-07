// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class CatalogueCheckTests:DatabaseTests
{
    [Test]
    public void CatalogueCheck_DodgyName()
    {
        var cata = new Catalogue(CatalogueRepository, "fish");
            
        //name broken
        cata.Name = @"c:\bob.txt#";
        var ex = Assert.Throws<Exception>(()=>cata.Check(new ThrowImmediatelyCheckNotifier()));
        Assert.IsTrue(ex.Message.Contains("The following invalid characters were found:'\\','.','#'"));

        cata.DeleteInDatabase();
    }

    [TestCase(DatabaseType.MicrosoftSQLServer)]
    [TestCase(DatabaseType.MySql)]
    public void CatalogueCheck_FetchData(DatabaseType databaseType)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Name");
        dt.Rows.Add("Frank");
        dt.Rows.Add("Peter");

        var database = GetCleanedServer(databaseType);
        var tbl = database.CreateTable("CatalogueCheck_CanReadText",dt);

        var cata = Import(tbl);

        //shouldn't be any errors
        var tomemory = new ToMemoryCheckNotifier();
        cata.Check(tomemory);
        Assert.AreEqual(CheckResult.Success,tomemory.GetWorst());

        //delete all the records in the table
        tbl.Truncate();
        cata.Check(tomemory);

        //now it should warn us that it is empty 
        Assert.AreEqual(CheckResult.Warning, tomemory.GetWorst());

        tbl.Drop();


        cata.Check(tomemory);

        //now it should fail checks
        Assert.AreEqual(CheckResult.Fail, tomemory.GetWorst());


    }
}
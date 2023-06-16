// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Validation.Constraints.Secondary;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.Validation;

public class ReferentialIntegrityConstraintTests : DatabaseTests
{
    private ITableInfo _tableInfo;
    private ColumnInfo[] _columnInfo;
    private ReferentialIntegrityConstraint _constraint;

    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        var tbl = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer)
            .ExpectTable("ReferentialIntegrityConstraintTests");

        if (tbl.Exists())
            tbl.Drop();

        var server = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer).Server;

        using (var con = server.GetConnection())
        {
            con.Open();

            server.GetCommand("CREATE TABLE ReferentialIntegrityConstraintTests(MyValue int)", con).ExecuteNonQuery();
            server.GetCommand("INSERT INTO ReferentialIntegrityConstraintTests (MyValue) VALUES (5)", con)
                .ExecuteNonQuery();
        }

        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out _tableInfo, out _columnInfo);

        _constraint = new ReferentialIntegrityConstraint(CatalogueRepository)
        {
            OtherColumnInfo = _columnInfo.Single()
        };
    }

    [Test]
    [TestCase(5, false)]
    [TestCase("5", false)]
    [TestCase(4, true)]
    [TestCase(6, true)]
    [TestCase(-5, true)]
    public void NormalLogic(object value, bool expectFailure)
    {
        _constraint.InvertLogic = false;
        var failure = _constraint.Validate(value, null, null);

        //if it did not fail validation and we expected failure
        if (failure == null && expectFailure)
            Assert.Fail();

        //or it did fail validation and we did not expect failure
        if (failure != null && !expectFailure)
            Assert.Fail();

        Assert.Pass();
    }


    [Test]
    [TestCase(5, true)]
    [TestCase("5", true)]
    [TestCase(4, false)]
    [TestCase(6, false)]
    [TestCase(-5, false)]
    public void InvertedLogic(object value, bool expectFailure)
    {
        _constraint.InvertLogic = true;
        var failure = _constraint.Validate(value, null, null);

        //if it did not fail validation and we expected failure
        if (failure == null && expectFailure)
            Assert.Fail();

        //or it did fail validation and we did not expect failure
        if (failure != null && !expectFailure)
            Assert.Fail();

        Assert.Pass();
    }
}
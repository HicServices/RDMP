// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.Curation.Unit;

[Category("Unit")]
internal class IColumnTests
{
    /// <summary>
    /// For tests
    /// </summary>
    private class TestColumn : SpontaneousObject, IColumn
    {
        public TestColumn() : base(new MemoryRepository())
        {
        }

        public string GetRuntimeName()
        {
            var helper = MicrosoftQuerySyntaxHelper.Instance;

            return Alias ?? helper.GetRuntimeName(SelectSQL);
        }

        public ColumnInfo ColumnInfo { get; private set; }
        public int Order { get; set; }

        [Sql] public string SelectSQL { get; set; }
        public string Alias { get; set; }
        public bool HashOnDataRelease { get; private set; }
        public bool IsExtractionIdentifier { get; private set; }
        public bool IsPrimaryKey { get; private set; }

        public void Check(ICheckNotifier notifier)
        {
            new ColumnSyntaxChecker(this).Check(notifier);
        }
    }

    [Test]
    public void GetRuntimeName_Strings_Pass()
    {
        var syntax = MicrosoftQuerySyntaxHelper.Instance;

        Assert.AreEqual(syntax.GetRuntimeName("[test]"), "test");
        Assert.AreEqual(syntax.GetRuntimeName("`test`"), "`test`");
        Assert.AreEqual(syntax.GetRuntimeName("`[test]`"), "`[test]`");
        Assert.AreEqual(syntax.GetRuntimeName("[mydb].[test]"), "test");
        Assert.AreEqual(syntax.GetRuntimeName("`mymysqldb`.`test`"), "`test`");
        Assert.AreEqual(syntax.GetRuntimeName("[mydb]..[test]"), "test");
        Assert.AreEqual(syntax.GetRuntimeName("[SERVER].[mydb]..[test]"), "test");
    }

    [Test]
    public void GetRuntimeName_IColumns_Pass()
    {
        var tc = new TestColumn
        {
            Alias = "test"
        };

        Assert.AreEqual(tc.GetRuntimeName(),"test");

        tc.SelectSQL = "MangleQuery([mydb]..[myExcitingField])"; //still has Alias
        Assert.AreEqual(tc.GetRuntimeName(), "test");

        tc.Alias = null;
        tc.SelectSQL = "[mydb]..[myExcitingField]";
        Assert.AreEqual(tc.GetRuntimeName(), "myExcitingField");
    }

    [Test]
    public void GetRuntimeName_IColumns_ThrowBecauseMissingAliasOnScalarValueFunction()
    {
        var tc = new TestColumn
        {
            SelectSQL = "MangleQuery([mydb]..[myExcitingField])"
        };

        var ex = Assert.Throws<RuntimeNameException>(()=> tc.GetRuntimeName());
    }


    [Test]
    public void CheckSyntax_IColumn_Valid()
    {
        var tc = new TestColumn
        {
            Alias = "[bob smith]"
        };

        tc.Check(new ThrowImmediatelyCheckNotifier());
        tc.Alias = "`bob smith`";
        tc.Check(new ThrowImmediatelyCheckNotifier());
        tc.Alias = "`[bob smith]`";
        tc.Check(new ThrowImmediatelyCheckNotifier());
    }


    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias1()
    {
        var tc = new TestColumn
        {
            Alias = "bob smith"
        };
        var ex = Assert.Throws<SyntaxErrorException>(()=>tc.Check(new ThrowImmediatelyCheckNotifier()));
        Assert.AreEqual("Whitespace found in unwrapped Alias \"bob smith\"",ex.Message);

    }

    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias2()
    {
        var tc = new TestColumn
        {
            Alias = "`bob"
        };

        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(new ThrowImmediatelyCheckNotifier()));
        Assert.AreEqual("Invalid characters found in Alias \"`bob\"", ex.Message);
    }

    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias3()
    {
        var tc = new TestColumn
        {
            Alias = "bob]"
        };
        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(new ThrowImmediatelyCheckNotifier()));
        Assert.AreEqual("Invalid characters found in Alias \"bob]\"", ex.Message);
    }

    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidSelectSQL()
    {
        var tc = new TestColumn
        {
            Alias = "bob",
            SelectSQL = "GetSomething('here'"
        };
        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(new ThrowImmediatelyCheckNotifier()));
        Assert.AreEqual("Mismatch in the number of opening '(' and closing ')'", ex.Message);
    }
}
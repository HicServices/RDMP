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
    ///     For tests
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

        Assert.Multiple(() =>
        {
            Assert.That(syntax.GetRuntimeName("[test]"), Is.EqualTo("test"));
            Assert.That(syntax.GetRuntimeName("`test`"), Is.EqualTo("`test`"));
            Assert.That(syntax.GetRuntimeName("`[test]`"), Is.EqualTo("`[test]`"));
            Assert.That(syntax.GetRuntimeName("[mydb].[test]"), Is.EqualTo("test"));
            Assert.That(syntax.GetRuntimeName("`mymysqldb`.`test`"), Is.EqualTo("`test`"));
            Assert.That(syntax.GetRuntimeName("[mydb]..[test]"), Is.EqualTo("test"));
            Assert.That(syntax.GetRuntimeName("[SERVER].[mydb]..[test]"), Is.EqualTo("test"));
        });
    }

    [Test]
    public void GetRuntimeName_IColumns_Pass()
    {
        var tc = new TestColumn
        {
            Alias = "test"
        };

        Assert.That(tc.GetRuntimeName(), Is.EqualTo("test"));

        tc.SelectSQL = "MangleQuery([mydb]..[myExcitingField])"; //still has Alias
        Assert.That(tc.GetRuntimeName(), Is.EqualTo("test"));

        tc.Alias = null;
        tc.SelectSQL = "[mydb]..[myExcitingField]";
        Assert.That(tc.GetRuntimeName(), Is.EqualTo("myExcitingField"));
    }

    [Test]
    public void GetRuntimeName_IColumns_ThrowBecauseMissingAliasOnScalarValueFunction()
    {
        var tc = new TestColumn
        {
            SelectSQL = "MangleQuery([mydb]..[myExcitingField])"
        };

        Assert.Throws<RuntimeNameException>(() => tc.GetRuntimeName());
    }


    [Test]
    public void CheckSyntax_IColumn_Valid()
    {
        var tc = new TestColumn
        {
            Alias = "[bob smith]"
        };

        tc.Check(ThrowImmediatelyCheckNotifier.Quiet);
        tc.Alias = "`bob smith`";
        tc.Check(ThrowImmediatelyCheckNotifier.Quiet);
        tc.Alias = "`[bob smith]`";
        tc.Check(ThrowImmediatelyCheckNotifier.Quiet);
    }


    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias1()
    {
        var tc = new TestColumn
        {
            Alias = "bob smith"
        };
        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("Whitespace found in unwrapped Alias \"bob smith\""));
    }

    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias2()
    {
        var tc = new TestColumn
        {
            Alias = "`bob"
        };

        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("Invalid characters found in Alias \"`bob\""));
    }

    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias3()
    {
        var tc = new TestColumn
        {
            Alias = "bob]"
        };
        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("Invalid characters found in Alias \"bob]\""));
    }

    [Test]
    public void CheckSyntax_IColumn_ThrowBecauseInvalidSelectSQL()
    {
        var tc = new TestColumn
        {
            Alias = "bob",
            SelectSQL = "GetSomething('here'"
        };
        var ex = Assert.Throws<SyntaxErrorException>(() => tc.Check(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.That(ex.Message, Is.EqualTo("Mismatch in the number of opening '(' and closing ')'"));
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Sharing.Refactoring;
using Rdmp.Core.Sharing.Refactoring.Exceptions;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.RefactoringTests;

public class SelectSQLRefactorerTests : UnitTests
{
    [Test]
    public void RefactorTableName_TestValidReplacement_ColumnInfo()
    {
        var columnInfo = WhenIHaveA<ColumnInfo>();
        columnInfo.Name = "[database]..[table].[column]";

        var tableInfo = columnInfo.TableInfo;
        tableInfo.Database = "database";
        tableInfo.Name = "[database]..[table]";

        SelectSQLRefactorer.RefactorTableName(columnInfo, tableInfo, "[database]..[table2]");

        Assert.That(columnInfo.Name, Is.EqualTo("[database]..[table2].[column]"));
    }

    [Test]
    public void RefactorTableName_TestValidReplacement_ExtractionInformation()
    {
        var ei = WhenIHaveA<ExtractionInformation>();
        ei.SelectSQL = "UPPER([database]..[table].[column])";
        ei.Alias = "MyCatalogueItem";
        ei.SaveToDatabase();

        var ci = ei.ColumnInfo;
        ci.Name = "[database]..[table].[column]";
        ci.SaveToDatabase();

        var tableInfo = ei.ColumnInfo.TableInfo;
        tableInfo.Database = "database";
        tableInfo.Name = "[database]..[table]";
        tableInfo.SaveToDatabase();

        SelectSQLRefactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]");

        Assert.That(ei.SelectSQL, Is.EqualTo("UPPER([database]..[table2].[column])"));
    }

    [Test]
    [TestCase("UPPER([database]..[table].[column])", true)]
    [TestCase(
        "dbo.MyScalarFunction([database]..[table].[column]) in Select(distinct [database]..[table].[column] from bob)",
        true)]
    [TestCase("dbo.MyNewRand()", false)]
    [TestCase(
        "[dbo].MyScalarFunction([database]..[table].[column]) in Select(distinct [database]..[table].[column] from bob)",
        true)]
    [TestCase("[dbo].MyNewRand()", false)]
    public void RefactorTableName_IsRefactorable_ExtractionInformation(string transformSql,
        bool expectedToBeRefactorable)
    {
        var ei = WhenIHaveA<ExtractionInformation>();
        ei.SelectSQL = transformSql;
        ei.Alias = "MyCatalogueItem";
        ei.SaveToDatabase();

        var ci = ei.ColumnInfo;
        ci.Name = "[database]..[table].[column]";
        ci.SaveToDatabase();

        var tableInfo = ei.ColumnInfo.TableInfo;
        tableInfo.Database = "database";
        tableInfo.Name = "[database]..[table]";
        tableInfo.SaveToDatabase();

        Assert.That(SelectSQLRefactorer.IsRefactorable(ei), Is.EqualTo(expectedToBeRefactorable));

        if (expectedToBeRefactorable)
            SelectSQLRefactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]");
        else
            Assert.Throws<RefactoringException>(() =>
                SelectSQLRefactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]"));
    }

    [TestCase("[Fish]..[MyTbl]", "[Fish]..[MyTbl2]")]
    public void RefactorTableName_IsRefactorable_TableInfoWithNoColumnInfos(string oldName, string newName)
    {
        var ti = WhenIHaveA<TableInfo>();
        ti.Name = oldName;
        ti.Database = "Fish";
        ti.SaveToDatabase();

        foreach (IDeleteable d in ti.ColumnInfos)
            d.DeleteInDatabase();

        Assert.Multiple(() =>
        {
            Assert.That(SelectSQLRefactorer.IsRefactorable(ti));

            Assert.That(SelectSQLRefactorer.RefactorTableName(ti, newName), Is.EqualTo(1));
            Assert.That(ti.Name, Is.EqualTo(newName));
        });
    }

    [TestCase("[Donkey]..[MyTbl]", "[Fish]..[MyTbl2]", "'[Donkey]..[MyTbl]' has incorrect database property 'Fish'")]
    public void RefactorTableName_IsNotRefactorable_TableInfoWithNoColumnInfos(string oldName, string newName,
        string expectedReason)
    {
        var ti = WhenIHaveA<TableInfo>();
        ti.Name = oldName;
        ti.Database = "Fish";
        ti.SaveToDatabase();

        foreach (IDeleteable d in ti.ColumnInfos)
            d.DeleteInDatabase();

        Assert.That(SelectSQLRefactorer.IsRefactorable(ti), Is.False);

        var ex = Assert.Throws<RefactoringException>(() => SelectSQLRefactorer.RefactorTableName(ti, newName));
        Assert.That(ex.Message, Does.Contain(expectedReason));
    }


    /// <summary>
    ///     Tests when the Column name does not exactly match the search/replace table name pattern during refactoring.
    /// </summary>
    /// <example>
    ///     Refactor columns belonging to [Fish].[dbo].[TableA] to now belong in [Fish].[dbo].[TableB]
    ///     BUT column name is [Fish]..[TableA].[MyCol].  In this case it should be refactored to [Fish].[dbo].[TableB].[MyCol]
    /// </example>
    /// <param name="columnName">
    ///     A column that belongs to <paramref name="findTableName" /> which should be refactored even if
    ///     its name isn't an exact match to the table name
    /// </param>
    /// <param name="findTableName">The table being renamed, will be renamed MyTbl to MyNewTbl</param>
    [TestCase("[Fish]..[MyTbl].[A]", "[Fish]..[MyTbl]")]
    [TestCase("[Fish].[dbo].[MyTbl].[A]", "[Fish]..[MyTbl]")]
    [TestCase("[Fish]..[MyTbl].[A]", "[Fish].[dbo].[MyTbl]")]
    [TestCase("[Fish].[dbo].[MyTbl].[A]", "[Fish].[dbo].[MyTbl]")]
    [TestCase("[Fish].dbo.[MyTbl].[A]", "[Fish]..[MyTbl]")]
    [TestCase("[Fish]..[MyTbl].[A]", "[Fish].dbo.[MyTbl]")]
    [TestCase("[Fish].dbo.[MyTbl].[A]", "[Fish].dbo.[MyTbl]")]
    public void RefactorTableName_IsRefactorable_ColumnInfo(string columnName, string findTableName)
    {
        var col = WhenIHaveA<ColumnInfo>();
        col.Name = columnName;
        col.SaveToDatabase();

        var oldName = findTableName;
        var newName = oldName.Replace("MyTbl", "MyNewTbl");

        Assert.Multiple(() =>
        {
            Assert.That(SelectSQLRefactorer.RefactorTableName(col, oldName, newName), Is.EqualTo(1));

            Assert.That(col.Name, Is.EqualTo($"{newName}.[A]"));
        });
    }
}
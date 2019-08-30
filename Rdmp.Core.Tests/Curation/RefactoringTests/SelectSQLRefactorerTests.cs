// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Sharing.Refactoring;
using Rdmp.Core.Sharing.Refactoring.Exceptions;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.RefactoringTests
{
    public class SelectSQLRefactorerTests:UnitTests
    {
        [Test]
        public void RefactorTableName_TestValidReplacement_ColumnInfo()
        {
            var columnInfo = WhenIHaveA<ColumnInfo>();
            columnInfo.Name = "[database]..[table].[column]";
            
            var tableInfo = columnInfo.TableInfo;
            tableInfo.Database = "database";
            tableInfo.Name = "[database]..[table]";

            var refactorer = new SelectSQLRefactorer();
            refactorer.RefactorTableName(columnInfo,tableInfo,"[database]..[table2]");

            Assert.AreEqual("[database]..[table2].[column]", columnInfo.Name);
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

            var refactorer = new SelectSQLRefactorer();
            refactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]");

            Assert.AreEqual("UPPER([database]..[table2].[column])", ei.SelectSQL);
        }

        [Test]
        [TestCase("UPPER([database]..[table].[column])",true)]
        [TestCase("dbo.MyScalarFunction([database]..[table].[column]) in Select(distinct [database]..[table].[column] from bob)", true)]
        [TestCase("dbo.MyNewRand()", false)]
        public void RefactorTableName_IsRefactorable_ExtractionInformation(string transformSql,bool expectedToBeRefactorable)
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
            
            var refactorer = new SelectSQLRefactorer();
            
            Assert.AreEqual(expectedToBeRefactorable,refactorer.IsRefactorable(ei));

            if (expectedToBeRefactorable)
                refactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]");
            else
                Assert.Throws<RefactoringException>(() => refactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]"));
        }

        [TestCase("[Fish]..[MyTbl]","[Fish]..[MyTbl2]")]
        public void RefactorTableName_IsRefactorable_TableInfoWithNoColumnInfos(string oldName, string newName)
        {
            var ti = WhenIHaveA<TableInfo>();
            ti.Name = oldName;
            ti.Database = "Fish";
            ti.SaveToDatabase();

            foreach(IDeleteable d in ti.ColumnInfos)
                d.DeleteInDatabase();
            
            var refactorer = new SelectSQLRefactorer();
            Assert.IsTrue(refactorer.IsRefactorable(ti));

            Assert.AreEqual(1,refactorer.RefactorTableName(ti,newName));
            Assert.AreEqual(newName,ti.Name);
        }

        [TestCase("[Donkey]..[MyTbl]","[Fish]..[MyTbl2]","'[Donkey]..[MyTbl]' has incorrect database propery 'Fish'")]
        public void RefactorTableName_IsNotRefactorable_TableInfoWithNoColumnInfos(string oldName, string newName,string expectedReason)
        {
            var ti = WhenIHaveA<TableInfo>();
            ti.Name = oldName;
            ti.Database = "Fish";
            ti.SaveToDatabase();

            foreach(IDeleteable d in ti.ColumnInfos)
                d.DeleteInDatabase();
            
            var refactorer = new SelectSQLRefactorer();
            Assert.IsFalse(refactorer.IsRefactorable(ti));

            var ex = Assert.Throws<RefactoringException>(()=>refactorer.RefactorTableName(ti,newName));
            StringAssert.Contains(expectedReason,ex.Message);
        }


        //It shouldn't matter if you have dbo or not
        [TestCase("[Fish]..","[Fish]..")]
        [TestCase("[Fish].dbo.","[Fish]..")]
        [TestCase("[Fish]..","[Fish].dbo.")]
        [TestCase("[Fish].dbo.","[Fish].dbo.")]
        public void RefactorTableName_IsRefactorable_ColumnInfo(string columnTable,string findTableName)
        {
            var col = WhenIHaveA<ColumnInfo>();
            col.Name = columnTable + "[MyTbl].[A]";
            col.SaveToDatabase();

            var refactorer = new SelectSQLRefactorer();
            Assert.AreEqual(1,refactorer.RefactorTableName(col,findTableName + "[MyTbl].[A]" , findTableName + "[MyTbl2].[A]"));

            Assert.AreEqual( findTableName + "[MyTbl2].[A]",col.Name);
        }
    }
}

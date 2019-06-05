// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Sharing.Refactoring;
using Rdmp.Core.Sharing.Refactoring.Exceptions;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.RefactoringTests
{
    public class SelectSQLRefactorerTests:DatabaseTests
    {
        private TableInfo tableInfo;
        private ColumnInfo columnInfo;
        
        private Catalogue catalogue;
        private CatalogueItem catalogueItem;

        [SetUp]
        public void CreateEntities()
        {
            tableInfo = new TableInfo(CatalogueRepository, "[database]..[table]");
            tableInfo.Database = "database";
            tableInfo.SaveToDatabase();

            columnInfo = new ColumnInfo(CatalogueRepository, "[database]..[table].[column]", "varchar(10)", tableInfo);

            catalogue = new Catalogue(CatalogueRepository, "MyCatalogue");
            catalogueItem = new CatalogueItem(CatalogueRepository, catalogue, "MyCatalogueItem");
        }
        
        [TearDown]
        public void DeleteEntities()
        {
            tableInfo.DeleteInDatabase();
            catalogue.DeleteInDatabase();
        }
        [Test]
        public void RefactorTableName_TestValidReplacement_ColumnInfo()
        {
            var refactorer = new SelectSQLRefactorer();
            refactorer.RefactorTableName(columnInfo,tableInfo,"[database]..[table2]");

            Assert.AreEqual("[database]..[table2].[column]", columnInfo.Name);
        }

        [Test]
        public void RefactorTableName_TestValidReplacement_ExtractionInformation()
        {
            var ei = new ExtractionInformation(CatalogueRepository, catalogueItem, columnInfo,"UPPER([database]..[table].[column])");
            ei.Alias = "MyCatalogueItem";
            ei.SaveToDatabase();

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
            var ei = new ExtractionInformation(CatalogueRepository, catalogueItem, columnInfo, transformSql);
            ei.Alias = "MyCatalogueItem";
            ei.SaveToDatabase();

            var refactorer = new SelectSQLRefactorer();
            
            Assert.AreEqual(expectedToBeRefactorable,refactorer.IsRefactorable(ei));

            if (expectedToBeRefactorable)
                refactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]");
            else
                Assert.Throws<RefactoringException>(() => refactorer.RefactorTableName(ei, tableInfo, "[database]..[table2]"));
        }
    }
}

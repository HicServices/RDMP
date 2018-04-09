using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using NUnit.Framework;
using Sharing.Refactoring;
using Sharing.Refactoring.Exceptions;
using Tests.Common;

namespace CatalogueLibraryTests.RefactoringTests
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

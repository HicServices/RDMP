using System;
using CatalogueLibrary.Checks;
using CatalogueLibrary.Checks.SyntaxChecking;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Spontaneous;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Attributes;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace CatalogueLibraryTests.Unit
{
    [Category("Unit")]
    class IColumnTests
    {

        /// <summary>
        /// For tests
        /// </summary>
        private class TestColumn:SpontaneousObject,IColumn
        {
            public string GetRuntimeName()
            {
                return RDMPQuerySyntaxHelper.GetRuntimeName(this);
            }

            public ColumnInfo ColumnInfo { get; private set; }
            public int Order { get; set; }

            [Sql]
            public string SelectSQL { get; set; }
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
            var syntax = new MicrosoftQuerySyntaxHelper();

            Assert.AreEqual(syntax.GetRuntimeName("[test]"), "test");
            Assert.AreEqual(syntax.GetRuntimeName("`test`"), "test");
            Assert.AreEqual(syntax.GetRuntimeName("`[test]`"), "test");
            Assert.AreEqual(syntax.GetRuntimeName("[mydb].[test]"), "test");
            Assert.AreEqual(syntax.GetRuntimeName("`mymysqldb`.`test`"), "test");
            Assert.AreEqual(syntax.GetRuntimeName("[mydb]..[test]"), "test");
            Assert.AreEqual(syntax.GetRuntimeName("[SERVER].[mydb]..[test]"), "test");
        }

        [Test]
        public void GetRuntimeName_IColumns_Pass()
        {
            TestColumn tc = new TestColumn();

            tc.Alias = "test";
            Assert.AreEqual(tc.GetRuntimeName(),"test");

            tc.SelectSQL = "MangleQuery([mydb]..[myExcitingField])"; //still has Alias
            Assert.AreEqual(tc.GetRuntimeName(),"test");

            tc.Alias = null;
            tc.SelectSQL = "[mydb]..[myExcitingField]"; 
            Assert.AreEqual(tc.GetRuntimeName(), "myExcitingField");
            
        }

        [Test]
        [ExpectedException(ExpectedMessage = @"The IExtractableColumn.SelectSQL value ""MangleQuery([mydb]..[myExcitingField])"" looks like a ScalarValuedFunction but it is missing an Alias.  Add an Alias so that it has a runtime name.")]
        public void GetRuntimeName_IColumns_ThrowBecauseMissingAliasOnScalarValueFunction()
        {
            TestColumn tc = new TestColumn();

            tc.SelectSQL = "MangleQuery([mydb]..[myExcitingField])";
            string shouldHaveThrownHere = tc.GetRuntimeName();

            Console.WriteLine("should have thrown but instead it returned:\"" + shouldHaveThrownHere + "\"");
            
        }


        [Test]
        public void CheckSyntax_IColumn_Valid()
        {
            TestColumn tc = new TestColumn();
            
            tc.Alias = "[bob smith]";
            tc.Check(new ThrowImmediatelyCheckNotifier());
            tc.Alias = "`bob smith`";
            tc.Check(new ThrowImmediatelyCheckNotifier());
            tc.Alias = "`[bob smith]`";
            tc.Check(new ThrowImmediatelyCheckNotifier());

        }


        [Test]
        [ExpectedException(ExpectedMessage = "Whitespace found in unwrapped Alias \"bob smith\"")]
        public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias1()
        {
            TestColumn tc = new TestColumn();
            tc.Alias = "bob smith";
            tc.Check(new ThrowImmediatelyCheckNotifier());

        }

        [Test]
        [ExpectedException(ExpectedMessage = "Invalid characters found in Alias \"`bob\"")]
        public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias2()
        {
            TestColumn tc = new TestColumn();
            tc.Alias = "`bob";
            tc.Check(new ThrowImmediatelyCheckNotifier());
           
        }
        [Test]
        [ExpectedException(ExpectedMessage = "Invalid characters found in Alias \"bob]\"")]
        public void CheckSyntax_IColumn_ThrowBecauseInvalidAlias3()
        {
            TestColumn tc = new TestColumn();
            tc.Alias = "bob]";
            tc.Check(new ThrowImmediatelyCheckNotifier());
        }

        [Test] 
        [ExpectedException(ExpectedMessage = "Mismatch in the number of opening '(' and closing ')'")]
        public void CheckSyntax_IColumn_ThrowBecauseInvalidSelectSQL()
        {
            TestColumn tc = new TestColumn();
            tc.Alias = "bob";
            tc.SelectSQL = "GetSomething('here'";
            tc.Check(new ThrowImmediatelyCheckNotifier());
        }
    }
}

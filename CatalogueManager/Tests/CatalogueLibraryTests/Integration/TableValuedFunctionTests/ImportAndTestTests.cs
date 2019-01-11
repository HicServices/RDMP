using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.DataHelper;
using FAnsi.Connections;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace CatalogueLibraryTests.Integration.TableValuedFunctionTests
{
    public class ImportAndTestTests : DatabaseTests
    {
        private TestableTableValuedFunction _function = new TestableTableValuedFunction();
        [SetUp]
        public void CreateFunction()
        {
            _function.Create(DiscoveredDatabaseICanCreateRandomTablesIn, CatalogueRepository);
        }

        [Test]
        public void FunctionWorks()
        {
            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                var r = server.GetCommand("Select * from dbo.MyAwesomeFunction(5,10,'Fish')",con).ExecuteReader();

                r.Read();
                Assert.AreEqual(5, r["Number"]);
                Assert.AreEqual("Fish", r["Name"]);


                r.Read();
                Assert.AreEqual(6, r["Number"]);
                Assert.AreEqual("Fish", r["Name"]);


                r.Read();
                Assert.AreEqual(7, r["Number"]);
                Assert.AreEqual("Fish", r["Name"]);


                r.Read();
                Assert.AreEqual(8, r["Number"]);
                Assert.AreEqual("Fish", r["Name"]);


                r.Read();
                Assert.AreEqual(9, r["Number"]);
                Assert.AreEqual("Fish", r["Name"]);


                Assert.IsFalse(r.Read());
            }
        }

        [TearDown]
        public void TearDown()
        {
            _function.Destroy();
        }

        [Test]
        public void ImportFunctionIntoCatalogue()
        {
            Assert.AreEqual(2, _function.ColumnInfosCreated.Length);
            Assert.IsTrue(_function.TableInfoCreated.Name.Contains("MyAwesomeFunction(@startNumber,@stopNumber,@name)"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestDiscovery(bool usetransaction)
        {
            var db = DiscoveredDatabaseICanCreateRandomTablesIn;
            
            IManagedTransaction transaction = null;

            
            {
                using (var con = db.Server.GetConnection())
                {
                    con.Open();

                    //drop function - outside of transaction
                    db.Server.GetCommand("drop function MyAwesomeFunction", con).ExecuteNonQuery();

                    //now begin transaction
                    DbTransaction t = null;
                    
                    if(usetransaction)
                        t = con.BeginTransaction();

                    //create it within the scope of the transaction
                    transaction = new ManagedTransaction(con, t);
                    var cmd = db.Server.GetCommand(_function.CreateFunctionSQL.Substring(_function.CreateFunctionSQL.IndexOf("GO") + 3), con);
                    cmd.Transaction = t;
                    cmd.ExecuteNonQuery();

                    Assert.IsTrue(db.DiscoverTableValuedFunctions(transaction).Any(tbv => tbv.GetRuntimeName().Equals("MyAwesomeFunction")));
                    Assert.IsTrue(db.ExpectTableValuedFunction("MyAwesomeFunction").Exists(transaction));

                    var cols = db.ExpectTableValuedFunction("MyAwesomeFunction").DiscoverColumns(transaction);

                    Assert.AreEqual(2, cols.Length);
                    Assert.IsTrue(cols[0].GetFullyQualifiedName().Contains("MyAwesomeFunction.[Number]"));
                    Assert.IsTrue(cols[1].GetFullyQualifiedName().Contains("MyAwesomeFunction.[Name]"));

                    Assert.AreEqual("int", cols[0].DataType.SQLType);
                    Assert.AreEqual("varchar(50)", cols[1].DataType.SQLType);

                    if (usetransaction)
                        transaction.CommitAndCloseConnection();
                }
            }
        }

        [Test]
        public void Synchronization_ExtraParameter()
        {
            string expectedMessage =
                "MyAwesomeFunction is a Table Valued Function, in the Catalogue it has a parameter called @fish but this parameter no longer appears in the underlying database";

            var excessParameter = new AnyTableSqlParameter(CatalogueRepository, _function.TableInfoCreated, "DECLARE @fish as int");
            var checker = new ToMemoryCheckNotifier();
            _function.TableInfoCreated.Check(checker);
            
            Assert.IsTrue(checker.Messages.Any(m=>m.Result == CheckResult.Fail 
                &&
                m.Message.Contains(expectedMessage)));

            var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

            var ex = Assert.Throws<Exception>(()=>syncer.Synchronize(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.Contains(expectedMessage));

            //no changes yet
            Assert.IsTrue(excessParameter.HasLocalChanges().Evaluation == ChangeDescription.NoChanges);

            //sync should have proposed to drop the excess parameter (see above), accept the change
            Assert.IsTrue(syncer.Synchronize(new AcceptAllCheckNotifier()));

            //now parameter shouldnt be there
            Assert.IsTrue(excessParameter.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyWasDeleted);

        }

        [Test]
        public void Synchronization_MissingParameter()
        {
            string expectedMessage = "MyAwesomeFunction is a Table Valued Function but it does not have a record of the parameter @startNumber which appears in the underlying database";

            AnyTableSqlParameter parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters().Single(p => p.ParameterName.Equals("@startNumber"));
            parameter.DeleteInDatabase();

            var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

            var ex = Assert.Throws<Exception>(() => syncer.Synchronize(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.Contains(expectedMessage));

            //no parameter called @startNumber (because we deleted it right!)
            Assert.IsFalse(_function.TableInfoCreated.GetAllParameters().Any(p => p.ParameterName.Equals("@startNumber")));

            //sync should have proposed to create the missing parameter (see above), accept the change
            Assert.IsTrue(syncer.Synchronize(new AcceptAllCheckNotifier()));

            //now parameter should have reappeared due to accepthing change
            Assert.IsTrue(_function.TableInfoCreated.GetAllParameters().Any(p => p.ParameterName.Equals("@startNumber")));
            
        }

        [Test]
        public void Synchronization_ParameterDefinitionChanged()
        {
            string expectedMessage =
                "Parameter @startNumber is declared as 'DECLARE @startNumber AS int;' but in the Catalogue it appears as 'DECLARE @startNumber AS datetime;'";

            AnyTableSqlParameter parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters().Single(p => p.ParameterName.Equals("@startNumber"));
            parameter.ParameterSQL = "DECLARE @startNumber AS datetime;";
            parameter.SaveToDatabase();

            var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

            var ex = Assert.Throws<Exception>(() => syncer.Synchronize(new ThrowImmediatelyCheckNotifier()));
            Assert.IsTrue(ex.Message.Contains(expectedMessage));

            //no changes should yet have taken place since we didn't accept it yet
            Assert.IsTrue(parameter.HasLocalChanges().Evaluation == ChangeDescription.NoChanges);

            //sync should have proposed to adjusting the datatype
            Assert.IsTrue(syncer.Synchronize(new AcceptAllCheckNotifier()));

            //now parameter should have the correct datatype
            Assert.IsTrue(parameter.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent);
            var diff = parameter.HasLocalChanges().Differences.Single();

            Assert.AreEqual("DECLARE @startNumber AS datetime;",diff.LocalValue);
            Assert.AreEqual("DECLARE @startNumber AS int;", diff.DatabaseValue);

        }

        [Test]
        public void Synchronization_ParameterRenamed()
        {
            AnyTableSqlParameter parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters().Single(p => p.ParameterName.Equals("@startNumber"));
            parameter.ParameterSQL = "DECLARE @startNum AS int";
            parameter.SaveToDatabase();

            var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

            //shouldn't be any
            Assert.IsFalse(_function.TableInfoCreated.GetAllParameters().Any(p => p.ParameterName.Equals("@startNumber")));
            syncer.Synchronize(new AcceptAllCheckNotifier());

            var after = _function.TableInfoCreated.GetAllParameters();
            //now there should be recreated (actually it will suggest deleting the excess one and creating the underlying one as 2 separate suggestions one after the other)
            Assert.IsTrue(after.Any(p => p.ParameterName.Equals("@startNumber")));

            //still there should only be 3 parameters
            Assert.AreEqual(3,after.Length);

        }


        [Test]
        public void TableInfoCheckingWorks()
        {
            _function.TableInfoCreated.Check(new ThrowImmediatelyCheckNotifier() { ThrowOnWarning = true });
        }
        
        [Test]
        public void CatalogueCheckingWorks()
        {
            _function.Cata.Check(new ThrowImmediatelyCheckNotifier() { ThrowOnWarning = true });
        }
    }
}

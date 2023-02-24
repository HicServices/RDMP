// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTable.Revertable;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.Cohort;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.TableValuedFunctionTests;

public class ImportAndTestTests : DatabaseTests
{
    private TestableTableValuedFunction _function = new();
    private DiscoveredDatabase _database;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        _database = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        _function.Create(_database, CatalogueRepository);
    }

    [Test]
    public void FunctionWorks()
    {
        var server = _database.Server;
        using var con = server.GetConnection();
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

        
    [Test]
    public void ImportFunctionIntoCatalogue()
    {
        Assert.AreEqual(2, _function.ColumnInfosCreated.Length);
        Assert.IsTrue(_function.TableInfoCreated.Name.Contains("MyAwesomeFunction(@startNumber,@stopNumber,@name)"));
    }

    [Test]
    public void TestDiscovery()
    {
        var db = _database;

        using var con = db.Server.BeginNewTransactedConnection();
        //drop function - outside of transaction
        db.Server.GetCommand("drop function MyAwesomeFunction", con).ExecuteNonQuery();

        //create it within the scope of the transaction
        var cmd = db.Server.GetCommand(_function.CreateFunctionSQL[(_function.CreateFunctionSQL.IndexOf("GO") + 3)..], con);
        cmd.ExecuteNonQuery();

        Assert.IsTrue(db.DiscoverTableValuedFunctions(con.ManagedTransaction).Any(tbv => tbv.GetRuntimeName().Equals("MyAwesomeFunction")));
        Assert.IsTrue(db.ExpectTableValuedFunction("MyAwesomeFunction").Exists(con.ManagedTransaction));

        var cols = db.ExpectTableValuedFunction("MyAwesomeFunction").DiscoverColumns(con.ManagedTransaction);

        Assert.AreEqual(2, cols.Length);
        Assert.IsTrue(cols[0].GetFullyQualifiedName().Contains("MyAwesomeFunction.[Number]"));
        Assert.IsTrue(cols[1].GetFullyQualifiedName().Contains("MyAwesomeFunction.[Name]"));

        Assert.AreEqual("int", cols[0].DataType.SQLType);
        Assert.AreEqual("varchar(50)", cols[1].DataType.SQLType);

        con.ManagedTransaction.CommitAndCloseConnection();
    }

    [Test]
    public void Synchronization_ExtraParameter()
    {
        var expectedMessage =
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
        var expectedMessage = "MyAwesomeFunction is a Table Valued Function but it does not have a record of the parameter @startNumber which appears in the underlying database";

        var parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters().Single(p => p.ParameterName.Equals("@startNumber"));
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
        var expectedMessage =
            "Parameter @startNumber is declared as 'DECLARE @startNumber AS int;' but in the Catalogue it appears as 'DECLARE @startNumber AS datetime;'";

        var parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters().Single(p => p.ParameterName.Equals("@startNumber"));
        parameter.ParameterSQL = "DECLARE @startNumber AS datetime;";
        parameter.SaveToDatabase();

        var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

        var ex = Assert.Throws<Exception>(() => syncer.Synchronize(new ThrowImmediatelyCheckNotifier()));
        StringAssert.Contains(expectedMessage,ex.Message);

        //no changes should yet have taken place since we didn't accept it yet
        Assert.IsTrue(parameter.HasLocalChanges().Evaluation == ChangeDescription.NoChanges);

        //sync should have proposed to adjusting the datatype
        Assert.IsTrue(syncer.Synchronize(new AcceptAllCheckNotifier()));

        if(CatalogueRepository is not TableRepository)
        {
            // with a Yaml repository there is only one copy of the object so no need
            // to check for differences in db
            return;
        }

        //now parameter should have the correct datatype
        Assert.IsTrue(parameter.HasLocalChanges().Evaluation == ChangeDescription.DatabaseCopyDifferent);
        var diff = parameter.HasLocalChanges().Differences.Single();

        Assert.AreEqual("DECLARE @startNumber AS datetime;",diff.LocalValue);
        Assert.AreEqual("DECLARE @startNumber AS int;", diff.DatabaseValue);

    }

    [Test]
    public void Synchronization_ParameterRenamed()
    {
        var parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters().Single(p => p.ParameterName.Equals("@startNumber"));
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
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode.Checks;
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
        var r = server.GetCommand("Select * from dbo.MyAwesomeFunction(5,10,'Fish')", con).ExecuteReader();

        r.Read();
        Assert.Multiple(() =>
        {
            Assert.That(r["Number"], Is.EqualTo(5));
            Assert.That(r["Name"], Is.EqualTo("Fish"));
        });


        r.Read();
        Assert.Multiple(() =>
        {
            Assert.That(r["Number"], Is.EqualTo(6));
            Assert.That(r["Name"], Is.EqualTo("Fish"));
        });


        r.Read();
        Assert.Multiple(() =>
        {
            Assert.That(r["Number"], Is.EqualTo(7));
            Assert.That(r["Name"], Is.EqualTo("Fish"));
        });


        r.Read();
        Assert.Multiple(() =>
        {
            Assert.That(r["Number"], Is.EqualTo(8));
            Assert.That(r["Name"], Is.EqualTo("Fish"));
        });


        r.Read();
        Assert.Multiple(() =>
        {
            Assert.That(r["Number"], Is.EqualTo(9));
            Assert.That(r["Name"], Is.EqualTo("Fish"));


            Assert.That(r.Read(), Is.False);
        });
    }


    [Test]
    public void ImportFunctionIntoCatalogue()
    {
        Assert.Multiple(() =>
        {
            Assert.That(_function.ColumnInfosCreated, Has.Length.EqualTo(2));
            Assert.That(_function.TableInfoCreated.Name, Does.Contain("MyAwesomeFunction(@startNumber,@stopNumber,@name)"));
        });
    }

    [Test]
    public void TestDiscovery()
    {
        var db = _database;

        using var con = db.Server.BeginNewTransactedConnection();
        //drop function - outside of transaction
        db.Server.GetCommand("drop function MyAwesomeFunction", con).ExecuteNonQuery();

        //create it within the scope of the transaction
        var cmd = db.Server.GetCommand(
            _function.CreateFunctionSQL[(_function.CreateFunctionSQL.IndexOf("GO", StringComparison.Ordinal) + 3)..],
            con);
        cmd.ExecuteNonQuery();

        Assert.Multiple(() =>
        {
            Assert.That(db.DiscoverTableValuedFunctions(con.ManagedTransaction)
                    .Any(tbv => tbv.GetRuntimeName().Equals("MyAwesomeFunction")));
            Assert.That(db.ExpectTableValuedFunction("MyAwesomeFunction").Exists(con.ManagedTransaction));
        });

        var cols = db.ExpectTableValuedFunction("MyAwesomeFunction").DiscoverColumns(con.ManagedTransaction);

        Assert.That(cols, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(cols[0].GetFullyQualifiedName(), Does.Contain("MyAwesomeFunction.[Number]"));
            Assert.That(cols[1].GetFullyQualifiedName(), Does.Contain("MyAwesomeFunction.[Name]"));

            Assert.That(cols[0].DataType.SQLType, Is.EqualTo("int"));
            Assert.That(cols[1].DataType.SQLType, Is.EqualTo("varchar(50)"));
        });

        con.ManagedTransaction.CommitAndCloseConnection();
    }

    [Test]
    public void Synchronization_ExtraParameter()
    {
        const string expectedMessage = "MyAwesomeFunction is a Table Valued Function, in the Catalogue it has a parameter called @fish but this parameter no longer appears in the underlying database";

        var excessParameter =
            new AnyTableSqlParameter(CatalogueRepository, _function.TableInfoCreated, "DECLARE @fish as int");
        var checker = new ToMemoryCheckNotifier();
        _function.TableInfoCreated.Check(checker);

        Assert.That(checker.Messages.Any(m => m.Result == CheckResult.Fail
                                                &&
                                                m.Message.Contains(expectedMessage)));

        var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

        var ex = Assert.Throws<Exception>(() => syncer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Does.Contain(expectedMessage));

            //no changes yet
            Assert.That(excessParameter.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges));

            //sync should have proposed to drop the excess parameter (see above), accept the change
            Assert.That(syncer.Synchronize(new AcceptAllCheckNotifier()));
        });

        //now parameter shouldn't be there
        Assert.That(excessParameter.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.DatabaseCopyWasDeleted));
    }

    [Test]
    public void Synchronization_MissingParameter()
    {
        const string expectedMessage = "MyAwesomeFunction is a Table Valued Function but it does not have a record of the parameter @startNumber which appears in the underlying database";

        var parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters()
            .Single(static p => p.ParameterName.Equals("@startNumber"));
        parameter.DeleteInDatabase();

        var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

        var ex = Assert.Throws<Exception>(() => syncer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.Multiple(() =>
        {
            Assert.That(ex.Message, Does.Contain(expectedMessage));

            //no parameter called @startNumber (because we deleted it right!)
            Assert.That(_function.TableInfoCreated.GetAllParameters().Any(p => p.ParameterName.Equals("@startNumber")), Is.False);

            //sync should have proposed to create the missing parameter (see above), accept the change
            Assert.That(syncer.Synchronize(new AcceptAllCheckNotifier()));
        });

        //now parameter should have reappeared due to accepthing change
        Assert.That(_function.TableInfoCreated.GetAllParameters().Any(p => p.ParameterName.Equals("@startNumber")));
    }

    [Test]
    public void Synchronization_ParameterDefinitionChanged()
    {
        const string expectedMessage = "Parameter @startNumber is declared as 'DECLARE @startNumber AS int;' but in the Catalogue it appears as 'DECLARE @startNumber AS datetime;'";

        var parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters()
            .Single(static p => p.ParameterName.Equals("@startNumber"));
        parameter.ParameterSQL = "DECLARE @startNumber AS datetime;";
        parameter.SaveToDatabase();

        var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

        var ex = Assert.Throws<Exception>(() => syncer.Synchronize(ThrowImmediatelyCheckNotifier.Quiet));
        Assert.Multiple(() =>
        {
            Assert.That(ex?.Message, Does.Contain(expectedMessage));

            //no changes should yet have taken place since we didn't accept it yet
            Assert.That(parameter.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.NoChanges));

            //sync should have proposed to adjusting the datatype
            Assert.That(syncer.Synchronize(new AcceptAllCheckNotifier()));
        });

        if (CatalogueRepository is not TableRepository)
            // with a Yaml repository there is only one copy of the object so no need
            // to check for differences in db
            return;

        //now parameter should have the correct datatype
        Assert.That(parameter.HasLocalChanges().Evaluation, Is.EqualTo(ChangeDescription.DatabaseCopyDifferent));
        var diff = parameter.HasLocalChanges().Differences.Single();

        Assert.Multiple(() =>
        {
            Assert.That(diff.LocalValue, Is.EqualTo("DECLARE @startNumber AS datetime;"));
            Assert.That(diff.DatabaseValue, Is.EqualTo("DECLARE @startNumber AS int;"));
        });
    }

    [Test]
    public void Synchronization_ParameterRenamed()
    {
        var parameter = (AnyTableSqlParameter)_function.TableInfoCreated.GetAllParameters()
            .Single(p => p.ParameterName.Equals("@startNumber"));
        parameter.ParameterSQL = "DECLARE @startNum AS int";
        parameter.SaveToDatabase();

        var syncer = new TableInfoSynchronizer(_function.TableInfoCreated);

        //shouldn't be any
        Assert.That(_function.TableInfoCreated.GetAllParameters().Any(p => p.ParameterName.Equals("@startNumber")), Is.False);
        syncer.Synchronize(new AcceptAllCheckNotifier());

        var after = _function.TableInfoCreated.GetAllParameters();
        Assert.Multiple(() =>
        {
            //now there should be recreated (actually it will suggest deleting the excess one and creating the underlying one as 2 separate suggestions one after the other)
            Assert.That(after.Any(p => p.ParameterName.Equals("@startNumber")));

            //still there should only be 3 parameters
            Assert.That(after, Has.Length.EqualTo(3));
        });
    }


    [Test]
    public void TableInfoCheckingWorks()
    {
        _function.TableInfoCreated.Check(ThrowImmediatelyCheckNotifier.QuietPicky);
    }

    [Test]
    public void CatalogueCheckingWorks()
    {
        _function.Cata.Check(ThrowImmediatelyCheckNotifier.QuietPicky);
    }
}
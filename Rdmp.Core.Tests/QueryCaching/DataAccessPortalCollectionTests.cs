﻿// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Tests.QueryCaching;

internal class DataAccessPortalCollectionTests
{
    [OneTimeSetUp]
    public void LoadImplementation()
    {
        ImplementationManager.Load<MicrosoftSQLImplementation>();
        ImplementationManager.Load<MySqlImplementation>();
        ImplementationManager.Load<OracleImplementation>();
        ImplementationManager.Load<PostgreSqlImplementation>();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TestOneServer_CountCorrect(bool singleServer)
    {
        var collection = new DataAccessPointCollection(singleServer);
        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        collection.Add(_dap);
        Assert.That(collection.Points, Has.Count.EqualTo(1));
    }

    [Test]
    public void TestTwo_SameServer_PersistDatabase()
    {
        var collection = new DataAccessPointCollection(true);

        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server = "loco";
        _dap.Database = "B";
        _dap.DatabaseType = DatabaseType.Oracle;

        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server = "loco";
        _dap0.Database = "A";
        _dap0.DatabaseType = DatabaseType.Oracle;

        collection.Add(_dap0);
        collection.Add(_dap);

        Assert.That(collection.Points, Has.Count.EqualTo(2));

        var db = collection.GetDistinctServer().GetCurrentDatabase();
        Assert.That(db, Is.Null);
    }

    [Test]
    public void TestTwo_SameServer_NoCredentials()
    {
        var collection = new DataAccessPointCollection(true);
        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("ff");
        _cred.GetDecryptedPassword().Returns("pwd2");
        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("B");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        _dap.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);


        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("loco");
        _dap0.Database.Returns("A");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        _dap0.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);
        collection.Add(_dap);

        collection.Add(_dap0);

        Assert.That(collection.Points, Has.Count.EqualTo(2));

        //they both go to loco server so the single server should specify loco but no clear db
        var db = collection.GetDistinctServer().GetCurrentDatabase();
        Assert.That(db, Is.Null);
    }

    [Test]
    public void TestTwo_SameServer_SameCredentials()
    {
        var collection = new DataAccessPointCollection(true);
        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("ff");
        _cred.GetDecryptedPassword().Returns("pwd2");
        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("B");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        _dap.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);


        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("loco");
        _dap0.Database.Returns("A");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        _dap0.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);

        collection.Add(_dap);

        collection.Add(_dap0);

        Assert.That(collection.Points, Has.Count.EqualTo(2));
    }

    [Test]
    public void TestTwo_SameServer_OnlyOneUsesCredentials()
    {
        var collection = new DataAccessPointCollection(true);

        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("ff");
        _cred.GetDecryptedPassword().Returns("pwd2");

        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("B");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        _dap.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);


        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("loco");
        _dap0.Database.Returns("A");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        collection.Add(_dap);

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(_dap0)
        );

        Assert.Multiple(() =>
        {
            //should be relevant error and it shouldn't have been added
            Assert.That(ex.InnerException.Message, Does.Contain("ollection could not agree on a single Username"));
            Assert.That(collection.Points, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void TestTwo_SameServer_DifferentUsernames()
    {
        var collection = new DataAccessPointCollection(true);
        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("user1");
        _cred.GetDecryptedPassword().Returns("pwd2");


        var _cred2 = Substitute.For<IDataAccessCredentials>();
        _cred2.Username.Returns("user2");
        _cred2.GetDecryptedPassword().Returns("pwd2");

        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("B");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        _dap.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);
        collection.Add(_dap);


        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("loco");
        _dap0.Database.Returns("A");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        _dap0.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred2);

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(_dap0)
        );

        Assert.Multiple(() =>
        {
            //should be relevant error and it shouldn't have been added
            Assert.That(ex.InnerException.Message, Does.Contain("could not agree on a single Username to use to access the data under"));
            Assert.That(collection.Points, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void TestTwo_SameServer_DifferentPasswords()
    {
        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("user1");
        _cred.GetDecryptedPassword().Returns("pwd2");


        var _cred2 = Substitute.For<IDataAccessCredentials>();
        _cred2.Username.Returns("user1");
        _cred2.GetDecryptedPassword().Returns("pwd");
        var collection = new DataAccessPointCollection(true);
        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("B");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        _dap.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);
        collection.Add(_dap);



        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("loco");
        _dap0.Database.Returns("A");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        _dap0.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred2);

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(_dap0)
        );

        Assert.Multiple(() =>
        {
            //should be relevant error and it shouldn't have been added
            Assert.That(ex.InnerException.Message, Does.Contain("collection could not agree on a single Password to use to access the data under"));
            Assert.That(collection.Points, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void TestTwo_DifferentServer()
    {
        var collection = new DataAccessPointCollection(true);
        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("B");
        _dap.DatabaseType.Returns(DatabaseType.Oracle);
        collection.Add(_dap);
        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("joco");
        _dap0.Database.Returns("B");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>

        collection.Add(_dap0)
        );

        Assert.Multiple(() =>
        {
            //should be relevant error and it shouldn't have been added
            Assert.That(ex.InnerException.Message, Does.Contain("There was a mismatch in server names for data access points"));
            Assert.That(collection.Points, Has.Count.EqualTo(1));
        });
    }


    [Test]
    public void TestTwo_DifferentDatabaseType()
    {
        var collection = new DataAccessPointCollection(true);
        var _dap0 = Substitute.For<IDataAccessPoint>();
        _dap0.Server.Returns("loco");
        _dap0.Database.Returns("B");
        _dap0.DatabaseType.Returns(DatabaseType.Oracle);
        collection.Add(_dap0);
        var _dap = Substitute.For<IDataAccessPoint>();
        _dap.Server.Returns("loco");
        _dap.Database.Returns("A");
        _dap.DatabaseType.Returns(DatabaseType.MySql);
        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>

        collection.Add(_dap)
        );

        Assert.Multiple(() =>
        {
            //should be relevant error and it shouldn't have been added
            Assert.That(ex.InnerException.Message, Does.Contain("There was a mismatch on DatabaseType for data access points"));
            Assert.That(collection.Points, Has.Count.EqualTo(1));
        });
    }
}
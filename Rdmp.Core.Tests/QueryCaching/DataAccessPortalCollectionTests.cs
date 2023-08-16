// Copyright (c) The University of Dundee 2018-2019
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
        Assert.AreEqual(1, collection.Points.Count);
    }

    [Test]
    public void TestTwo_SameServer_PersistDatabase()
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
        collection.Add(_dap0);

        collection.Add(_dap);

        Assert.AreEqual(2, collection.Points.Count);

        //they both go to B so the single server should specify B
        var db = collection.GetDistinctServer().GetCurrentDatabase();
        Assert.AreEqual("B", db.GetRuntimeName());
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

        Assert.AreEqual(2, collection.Points.Count);

        //they both go to loco server so the single server should specify loco but no clear db
        var db = collection.GetDistinctServer().GetCurrentDatabase();
        Assert.IsNull(db);
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

        Assert.AreEqual(2, collection.Points.Count);
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

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("collection could not agree whether to use Credentials or not",
            ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
    }

    [Test]
    public void TestTwo_SameServer_DifferentUsernames()
    {
        var collection = new DataAccessPointCollection(true);
        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("user1");
        _cred.GetDecryptedPassword().Returns("pwd2");
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
        _dap0.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);
        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(_dap0)
        );

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("could not agree on a single Username to use to access the data under",
            ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
    }

    [Test]
    public void TestTwo_SameServer_DifferentPasswords()
    {
        var _cred = Substitute.For<IDataAccessCredentials>();
        _cred.Username.Returns("user1");
        _cred.GetDecryptedPassword().Returns("pwd2");
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
        _dap0.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing).Returns(_cred);

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(_dap0)
        );

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("collection could not agree on a single Password to use to access the data under",
            ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
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

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("There was a mismatch in server names for data access points", ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
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

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("There was a mismatch on DatabaseType for data access points", ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
    }
}
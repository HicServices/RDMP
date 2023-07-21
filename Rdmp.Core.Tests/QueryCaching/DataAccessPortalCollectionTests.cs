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
using Moq;
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
        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.DatabaseType == DatabaseType.Oracle));
        Assert.AreEqual(1, collection.Points.Count);
    }

    [Test]
    public void TestTwo_SameServer_PersistDatabase()
    {
        var collection = new DataAccessPointCollection(true);

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetQuerySyntaxHelper() == OracleQuerySyntaxHelper.Instance));

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetQuerySyntaxHelper() == OracleQuerySyntaxHelper.Instance));

        Assert.AreEqual(2, collection.Points.Count);

        //they both go to B so the single server should specify B
        var db = collection.GetDistinctServer().GetCurrentDatabase();
        Assert.AreEqual("B", db.GetRuntimeName());
    }

    [Test]
    public void TestTwo_SameServer_NoCredentials()
    {
        var collection = new DataAccessPointCollection(true);

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle));

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "A" &&
            m.DatabaseType == DatabaseType.Oracle));

        Assert.AreEqual(2, collection.Points.Count);

        //they both go to loco server so the single server should specify loco but no clear db
        var db = collection.GetDistinctServer().GetCurrentDatabase();
        Assert.IsNull(db);
    }

    [Test]
    public void TestTwo_SameServer_SameCredentials()
    {
        var collection = new DataAccessPointCollection(true);

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(u =>
                u.Username == "ff" &&
                u.GetDecryptedPassword() == "pwd")));

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "A" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(u =>
                u.Username == "ff" &&
                u.GetDecryptedPassword() == "pwd")));

        Assert.AreEqual(2, collection.Points.Count);
    }

    [Test]
    public void TestTwo_SameServer_OnlyOneUsesCredentials()
    {
        var collection = new DataAccessPointCollection(true);

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(u =>
                u.Username == "ff" &&
                u.GetDecryptedPassword() == "pwd")));

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(Mock.Of<IDataAccessPoint>(m =>
                m.Server == "loco" &&
                m.Database == "A" &&
                m.DatabaseType == DatabaseType.Oracle))
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

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(u =>
                u.Username == "user1" &&
                u.GetDecryptedPassword() == "pwd")));

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(Mock.Of<IDataAccessPoint>(m =>
                m.Server == "loco" &&
                m.Database == "A" &&
                m.DatabaseType == DatabaseType.Oracle &&
                m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(
                    u =>
                        u.Username == "user2" &&
                        u.GetDecryptedPassword() == "pwd")))
        );

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("could not agree on a single Username to use to access the data under",
            ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
    }

    [Test]
    public void TestTwo_SameServer_DifferentPasswords()
    {
        var collection = new DataAccessPointCollection(true);

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle &&
            m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(u =>
                u.Username == "user1" &&
                u.GetDecryptedPassword() == "pwd1")));

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(Mock.Of<IDataAccessPoint>(m =>
                m.Server == "loco" &&
                m.Database == "A" &&
                m.DatabaseType == DatabaseType.Oracle &&
                m.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing) == Mock.Of<IDataAccessCredentials>(
                    u =>
                        u.Username == "user1" &&
                        u.GetDecryptedPassword() == "pwd2")))
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

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle));

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(Mock.Of<IDataAccessPoint>(m =>
                m.Server == "joco" &&
                m.Database == "A" &&
                m.DatabaseType == DatabaseType.Oracle))
        );

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("There was a mismatch in server names for data access points", ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
    }


    [Test]
    public void TestTwo_DifferentDatabaseType()
    {
        var collection = new DataAccessPointCollection(true);

        collection.Add(Mock.Of<IDataAccessPoint>(m =>
            m.Server == "loco" &&
            m.Database == "B" &&
            m.DatabaseType == DatabaseType.Oracle));

        //cannot add because the second one wants integrated security
        var ex = Assert.Throws<InvalidOperationException>(() =>
            collection.Add(Mock.Of<IDataAccessPoint>(m =>
                m.Server == "loco" &&
                m.Database == "A" &&
                m.DatabaseType == DatabaseType.MySql))
        );

        //should be relevant error and it shouldn't have been added
        StringAssert.Contains("There was a mismatch on DatabaseType for data access points", ex.InnerException.Message);
        Assert.AreEqual(1, collection.Points.Count);
    }
}
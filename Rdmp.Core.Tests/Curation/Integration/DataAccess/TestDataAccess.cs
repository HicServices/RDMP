// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Rdmp.Core.ReusableLibraryCode.Exceptions;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration.DataAccess;

public class TestDataAccess : DatabaseTests
{
    private readonly List<Exception> asyncExceptions = new();

    [Test]
    public void AsyncTest()
    {
        if (CatalogueRepository is not TableRepository)
            Assert.Inconclusive("Test only applies to database repositories");

        var threads = new List<Thread>();


        for (var i = 0; i < 30; i++)
            threads.Add(new Thread(MessWithCatalogue));

        foreach (var t in threads)
            t.Start();

        while (threads.Any(t => t.ThreadState != ThreadState.Stopped))
            Thread.Sleep(100);

        for (var index = 0; index < asyncExceptions.Count; index++)
        {
            Console.WriteLine($"Exception {index}");
            var asyncException = asyncExceptions[index];
            Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(asyncException, true));
        }

        Assert.IsEmpty(asyncExceptions);
    }

    private void MessWithCatalogue()
    {
        try
        {
            var repository = new CatalogueRepository(CatalogueTableRepository.ConnectionStringBuilder);
            var cata = new Catalogue(repository, "bob")
            {
                Name = "Fuss"
            };
            cata.SaveToDatabase();
            cata.DeleteInDatabase();
        }
        catch (Exception ex)
        {
            asyncExceptions.Add(ex);
        }
    }


    /// <summary>
    ///     Real life test case where TableInfo is the IDataAccessPoint not just the test class
    /// </summary>
    [Test]
    public void TestGettingConnectionStrings()
    {
        foreach (var tbl in CatalogueRepository.GetAllObjects<TableInfo>()
                     .Where(table => table.Name.ToLower().Equals("bob")))
            tbl.DeleteInDatabase();

        foreach (var c in CatalogueRepository.GetAllObjects<DataAccessCredentials>()
                     .Where(cred => cred.Name.ToLower().Equals("bob")))
            c.DeleteInDatabase();

        //test it with TableInfos
        var t = new TableInfo(CatalogueRepository, "Bob");
        try
        {
            t.Server = "fish";
            t.Database = "bobsDatabase";
            t.SaveToDatabase();

            //t has no credentials
            var server = DataAccessPortal.ExpectServer(t, DataAccessContext.InternalDataProcessing);

            Assert.AreEqual(typeof(SqlConnectionStringBuilder), server.Builder.GetType());
            Assert.AreEqual("fish", ((SqlConnectionStringBuilder)server.Builder).DataSource);
            Assert.AreEqual("bobsDatabase", ((SqlConnectionStringBuilder)server.Builder).InitialCatalog);
            Assert.AreEqual(true, ((SqlConnectionStringBuilder)server.Builder).IntegratedSecurity);

            var creds = new DataAccessCredentials(CatalogueRepository, "Bob");
            try
            {
                t.SetCredentials(creds, DataAccessContext.InternalDataProcessing, true);
                creds.Username = "frank";
                creds.Password = "bobsPassword";
                creds.SaveToDatabase();

                //credentials are cached
                t.ClearAllInjections();

                ////t has some credentials now
                server = DataAccessPortal.ExpectServer(t, DataAccessContext.InternalDataProcessing);

                Assert.AreEqual(typeof(SqlConnectionStringBuilder), server.Builder.GetType());
                Assert.AreEqual("fish", ((SqlConnectionStringBuilder)server.Builder).DataSource);
                Assert.AreEqual("bobsDatabase", ((SqlConnectionStringBuilder)server.Builder).InitialCatalog);
                Assert.AreEqual("frank", ((SqlConnectionStringBuilder)server.Builder).UserID);
                Assert.AreEqual("bobsPassword", ((SqlConnectionStringBuilder)server.Builder).Password);
                Assert.AreEqual(false, ((SqlConnectionStringBuilder)server.Builder).IntegratedSecurity);
            }
            finally
            {
                var linker = new TableInfoCredentialsManager(CatalogueTableRepository);
                linker.BreakAllLinksBetween(creds, t);
                creds.DeleteInDatabase();
            }
        }
        finally
        {
            t.DeleteInDatabase();
        }
    }


    internal class TestAccessPoint : IDataAccessPoint, IDataAccessCredentials
    {
        public TestAccessPoint(string server, string database, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
        }

        public string Username { get; set; }
        public string Password { get; set; }


        public string GetDecryptedPassword()
        {
            return Password ?? "";
        }

        public string Server { get; set; }
        public string Database { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context)
        {
            if (Username != null)
                return this;

            return null;
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            return MicrosoftQuerySyntaxHelper.Instance;
        }

        public bool DiscoverExistence(DataAccessContext context, out string reason)
        {
            reason = "TestDataAccess never finds anything, it's a test";
            return false;
        }

        public override string ToString()
        {
            return Server + Database;
        }
    }

    #region Distinct Connection String (from Collection tests - Failing)

    [Test]
    public void TestDistinctCredentials_PasswordMismatch()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "bob", "username", "mypas"),
            new("frank", "bob", "username", "mydifferentPass")
        };

        //call this
        var ex = Assert.Throws<Exception>(() =>
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing,
                true));
        StringAssert.Contains("collection could not agree on a single Password", ex.Message);
    }

    [Test]
    public void TestDistinctCredentials_UsernamePasswordAreNull()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "bob", null, null),
            new("frank", "bob", "username", "mydifferentPass")
        };

        //call this
        var ex = Assert.Throws<Exception>(() =>
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing,
                true));
        StringAssert.Contains("collection could not agree whether to use Credentials", ex.Message);
    }

    [Test]
    public void TestDistinctCredentials_UsernameMismatch()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "bob", "usernameasdasd", "mydifferentpass"),
            new("frank", "bob", "username", "mydifferentPass")
        };

        //call this

        var ex = Assert.Throws<Exception>(() =>
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing,
                true));
        StringAssert.Contains("collection could not agree on a single Username", ex.Message);
    }


    [Test]
    public void TestDistinctCredentials_ServerMixedCapitalization_Allowed()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "bob", null, null),
            new("FRANK", "bob", null, null)
        };

        var server =
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing, true);
        Assert.AreEqual("frank", server.Name);
    }

    [Test]
    public void TestDistinctCredentials_DatabaseMixedCapitalization_NotAllowed()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "bob", null, null),
            new("frank", "BOB", null, null)
        };

        var ex = Assert.Throws<ExpectedIdenticalStringsException>(() =>
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing,
                true));
        StringAssert.Contains(
            "All data access points must be into the same database, access points 'frankbob' and 'frankBOB' are into different databases",
            ex.Message);
    }

    #endregion

    #region Distinct Connection String (from Collection tests - Passing)

    [Test]
    public void TestDistinctCredentials_WrappedDatabaseName()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "[bob's Database]", "username", "mypas"),
            new("frank", "bob's Database", "username", "mypas")
        };
        //call this
        var result =
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing, true);

        //test result
        Assert.AreEqual("bob's Database", result.Builder["Initial Catalog"]);
    }

    [Test]
    public void TestDistinctCredentials_PasswordMatch()
    {
        var testPoints = new List<TestAccessPoint>
        {
            new("frank", "bob", "username", "mypas"),
            new("frank", "bob", "username", "mypas")
        };

        //call this
        var result =
            DataAccessPortal.ExpectDistinctServer(testPoints.ToArray(), DataAccessContext.InternalDataProcessing, true);

        //test result
        Assert.AreEqual("mypas", result.Builder["Password"]);
    }

    #endregion
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Implementations.MicrosoftSQL;
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
        Assert.That(ex.Message, Does.Contain("collection could not agree on a single Password"));
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
        Assert.That(ex.Message, Does.Contain("collection could not agree whether to use Credentials"));
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
        Assert.That(ex.Message, Does.Contain("collection could not agree on a single Username"));
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
        Assert.That(server.Name, Is.EqualTo("frank"));
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
        Assert.That(
            ex.Message, Does.Contain("All data access points must be into the same database, access points 'frankbob' and 'frankBOB' are into different databases"));
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
        Assert.That(result.Builder["Initial Catalog"], Is.EqualTo("bob's Database"));
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
        Assert.That(result.Builder["Password"], Is.EqualTo("mypas"));
    }

    #endregion

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

        Assert.That(asyncExceptions, Is.Empty);
    }

    private List<Exception> asyncExceptions = new();

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
    /// Real life test case where TableInfo is the IDataAccessPoint not just the test class
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

            Assert.Multiple(() =>
            {
                Assert.That(server.Builder.GetType(), Is.EqualTo(typeof(SqlConnectionStringBuilder)));
                Assert.That(((SqlConnectionStringBuilder)server.Builder).DataSource, Is.EqualTo("fish"));
                Assert.That(((SqlConnectionStringBuilder)server.Builder).InitialCatalog, Is.EqualTo("bobsDatabase"));
                Assert.That(((SqlConnectionStringBuilder)server.Builder).IntegratedSecurity, Is.EqualTo(true));
            });

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

                Assert.Multiple(() =>
                {
                    Assert.That(server.Builder.GetType(), Is.EqualTo(typeof(SqlConnectionStringBuilder)));
                    Assert.That(((SqlConnectionStringBuilder)server.Builder).DataSource, Is.EqualTo("fish"));
                    Assert.That(((SqlConnectionStringBuilder)server.Builder).InitialCatalog, Is.EqualTo("bobsDatabase"));
                    Assert.That(((SqlConnectionStringBuilder)server.Builder).UserID, Is.EqualTo("frank"));
                    Assert.That(((SqlConnectionStringBuilder)server.Builder).Password, Is.EqualTo("bobsPassword"));
                    Assert.That(((SqlConnectionStringBuilder)server.Builder).IntegratedSecurity, Is.EqualTo(false));
                });
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
        public string Server { get; set; }
        public string Database { get; set; }
        public DatabaseType DatabaseType { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public TestAccessPoint(string server, string database, string username, string password)
        {
            Server = server;
            Database = database;
            Username = username;
            Password = password;
        }

        public IDataAccessCredentials GetCredentialsIfExists(DataAccessContext context) =>
            Username != null ? this : (IDataAccessCredentials)null;


        public string GetDecryptedPassword() => Password ?? "";

        public override string ToString() => Server + Database;

        public IQuerySyntaxHelper GetQuerySyntaxHelper() => MicrosoftQuerySyntaxHelper.Instance;

        public bool DiscoverExistence(DataAccessContext context, out string reason)
        {
            reason = "TestDataAccess never finds anything, it's a test";
            return false;
        }
    }
}
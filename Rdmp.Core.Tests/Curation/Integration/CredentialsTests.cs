// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Managers;
using Rdmp.Core.ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration;

public class CredentialsTests : DatabaseTests
{
    [OneTimeSetUp]
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        foreach (var table in CatalogueRepository.GetAllObjects<TableInfo>())
            if (table.Name.Equals("GetCredentialsFromATableInfo")
                ||
                table.Name.Equals("Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt1")
                ||
                table.Name.Equals("Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt2")
                ||
                table.Name.Equals("Dependency1")
                ||
                table.Name.Equals("Dependency2")
                ||
                table.Name.Equals("My Exciting Table")
                ||
                table.Name.Equals("Test")
                ||
                table.Name.Equals("Tableinfo1")
               )
                table.DeleteInDatabase();

        foreach (var cred in CatalogueRepository.GetAllObjects<DataAccessCredentials>())
            if (cred.Name.Equals("bob")
                ||
                cred.Name.Equals("Test")
               )
                cred.DeleteInDatabase();
    }

    [Test]
    public void CreateNewCredentials()
    {
        var newCredentials = new DataAccessCredentials(CatalogueRepository, "bob");

        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(newCredentials.Name, Is.EqualTo("bob"));
                Assert.That(newCredentials.ID, Is.Not.EqualTo(0));
            });
        }
        finally
        {
            newCredentials.DeleteInDatabase();
        }
    }


    [Test]
    public void CreateNewCredentialsThenGetByUsernamePasswordCombo()
    {
        var newCredentials = new DataAccessCredentials(CatalogueRepository, "bob")
        {
            Username = "myusername",
            Password = "mypassword"
        };

        newCredentials.SaveToDatabase();

        var newCopy = CatalogueRepository.GetAllObjects<DataAccessCredentials>()
            .SingleOrDefault(c => c.Username == "myusername");
        Assert.That(newCopy, Is.Not.Null);

        try
        {
            Assert.That(newCopy, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(newCopy.ID, Is.EqualTo(newCredentials.ID));
                Assert.That(newCopy.Username, Is.EqualTo(newCredentials.Username));
                Assert.That(newCopy.GetDecryptedPassword(), Is.EqualTo(newCredentials.GetDecryptedPassword()));
                Assert.That(newCopy.Password, Is.EqualTo(newCredentials.Password));
            });
        }
        finally
        {
            newCredentials.DeleteInDatabase();
        }
    }

    [Test]
    public void TestThe_Any_EnumValue_CannotRequestAnyCredentials()
    {
        var tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo")
        {
            Name = "My Exciting Table"
        };

        var creds = new DataAccessCredentials(CatalogueRepository);
        try
        {
            creds.Name = "Test";
            creds.SaveToDatabase();

            tableInfo.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
            tableInfo.SaveToDatabase();

            //attempt to request ANY credentials
            var ex = Assert.Throws<Exception>(() => tableInfo.GetCredentialsIfExists(DataAccessContext.Any));
            Assert.That(ex.Message, Is.EqualTo("You cannot ask for any credentials, you must supply a usage context."));
        }
        finally
        {
            tableInfo.DeleteInDatabase();
            creds.DeleteInDatabase();
        }
    }

    [Test]
    public void TestThe_Any_EnumValue()
    {
        var tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo")
        {
            Name = "My Exciting Table"
        };
        tableInfo.SaveToDatabase();

        var creds = new DataAccessCredentials(CatalogueRepository);
        try
        {
            creds.Name = "Test";
            creds.SaveToDatabase();

            //now create the association as Any
            tableInfo.SetCredentials(creds, DataAccessContext.Any);

            //because the credential is licenced to be used under ANY context, you can make requests under any of the specific contexts and be served the Any result
            var creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            Assert.That(creds2, Is.Not.Null);
            creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.DataExport);
            Assert.That(creds2, Is.Not.Null);
            creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.DataLoad);
            Assert.That(creds2, Is.Not.Null);
        }
        finally
        {
            tableInfo.DeleteInDatabase();
            creds.DeleteInDatabase();
        }
    }

    [Test]
    public void Test_Any_PrioritisingTheMoreAppropriateCredential()
    {
        var tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo")
        {
            Name = "Tableinfo1"
        };
        tableInfo.SaveToDatabase();

        var creds = new DataAccessCredentials(CatalogueRepository);
        var creds2 = new DataAccessCredentials(CatalogueRepository);

        try
        {
            creds.Name = "Test";
            creds.SaveToDatabase();

            //now create the association as Any
            tableInfo.SetCredentials(creds, DataAccessContext.DataLoad);
            tableInfo.SetCredentials(creds2, DataAccessContext.Any);


            Assert.That(tableInfo.GetCredentialsIfExists(DataAccessContext.DataLoad), Is.EqualTo(creds));
        }
        finally
        {
            tableInfo.DeleteInDatabase();
            creds.DeleteInDatabase();
            creds2.DeleteInDatabase();
        }
    }

    [Test]
    public void SaveAndReloadCredentials()
    {
        var originalCredentials = new DataAccessCredentials(CatalogueRepository, "bob");

        try
        {
            originalCredentials.Name = "bob1";
            originalCredentials.Username = "user";
            originalCredentials.Password = "pass";
            originalCredentials.SaveToDatabase();

            var newCopy = CatalogueRepository.GetObjectByID<DataAccessCredentials>(originalCredentials.ID);
            Assert.Multiple(() =>
            {
                Assert.That(newCopy.Name, Is.EqualTo(originalCredentials.Name));
                Assert.That(newCopy.Username, Is.EqualTo(originalCredentials.Username));
                Assert.That(newCopy.Password, Is.EqualTo(originalCredentials.Password));

                //test overridden Equals
                Assert.That(newCopy, Is.EqualTo(originalCredentials));
            });
            originalCredentials.Password = "fish";
            Assert.That(newCopy, Is.EqualTo(originalCredentials)); //they are still equal because IDs are the same
        }
        finally
        {
            originalCredentials.DeleteInDatabase();
        }
    }

    [Test]
    public void GetCredentialsFromATableInfo()
    {
        var tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo")
        {
            Name = "My Exciting Table"
        };

        var creds = new DataAccessCredentials(CatalogueRepository);
        try
        {
            creds.Name = "Test";
            creds.SaveToDatabase();

            tableInfo.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
            tableInfo.SaveToDatabase();

            //Go via TableInfo and get credentials
            var creds2 =
                (DataAccessCredentials)tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            Assert.That(creds.Name, Is.EqualTo(creds2.Name));
        }
        finally
        {
            tableInfo.DeleteInDatabase();
            creds.DeleteInDatabase();
        }
    }

    [Test]
    public void Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt_ThrowsThatCredentialsHasDependencies()
    {
        //Get all TableInfos that share this credential
        var tableInfo1 = new TableInfo(CatalogueRepository, "Dependency1");
        var tableInfo2 = new TableInfo(CatalogueRepository, "Dependency2");
        var creds = new DataAccessCredentials(CatalogueRepository, "bob");

        try
        {
            tableInfo1.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
            tableInfo2.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
            tableInfo1.SaveToDatabase();
            tableInfo2.SaveToDatabase();

            var ex = Assert.Throws<CredentialsInUseException>(creds
                .DeleteInDatabase); //the bit that fails (because tables are there)
            Assert.That(
                ex.Message,
                Is.EqualTo(
                    "Cannot delete credentials bob because it is in use by one or more TableInfo objects(Dependency1,Dependency2)"));
        }
        finally
        {
            tableInfo1.DeleteInDatabase(); //will work
            tableInfo2.DeleteInDatabase(); //will work
            creds.DeleteInDatabase(); //will work
        }
    }

    [Test]
    public void GetAllUsersOfACredential()
    {
        //Get all TableInfos that share this credential
        var tableInfo1 = new TableInfo(CatalogueRepository,
            "Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt1");
        var tableInfo2 = new TableInfo(CatalogueRepository,
            "Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt2");
        var creds = new DataAccessCredentials(CatalogueRepository, "bob");

        tableInfo1.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
        tableInfo2.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
        tableInfo1.SaveToDatabase();
        tableInfo2.SaveToDatabase();


        var TablesThatUseCredential =
            creds.GetAllTableInfosThatUseThis()[DataAccessContext.InternalDataProcessing].ToArray();

        Assert.That(TablesThatUseCredential, Does.Contain(tableInfo1));
        Assert.That(TablesThatUseCredential, Does.Contain(tableInfo2));

        tableInfo1.DeleteInDatabase();
        tableInfo2.DeleteInDatabase();
        creds.DeleteInDatabase();
    }

    [Test]
    public void GetConnectionStringFromCatalogueWhereOneTableInfoUsesACredentialsOverride()
    {
        var c = new Catalogue(CatalogueRepository,
            "GetConnectionStringFromCatalogueWhereOneTableInfoUsesACredentialsOverride");
        var ci = new CatalogueItem(CatalogueRepository, c,
            "GetConnectionStringFromCatalogueWhereOneTableInfoUsesACredentialsOverride");
        var t = new TableInfo(CatalogueRepository, "Test");
        var col = new ColumnInfo(CatalogueRepository, "[mydatabase].[dbo].test.col", "varchar(10)", t);

        DataAccessCredentials cred = null;
        try
        {
            t.Server = "myserver";
            t.Database = "mydatabase";

            cred = new DataAccessCredentials(CatalogueRepository, "bob")
            {
                Username = "bob",
                Password = "pass"
            };

            Assert.Multiple(() =>
            {
                Assert.That(cred.Password, Is.Not.EqualTo("pass"));
                Assert.That(cred.GetDecryptedPassword(), Is.EqualTo("pass"));
            });


            cred.SaveToDatabase();
            t.SetCredentials(cred, DataAccessContext.InternalDataProcessing);
            t.SaveToDatabase();

            var constr =
                (SqlConnectionStringBuilder)c
                    .GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing, false).Builder;
            Assert.Multiple(() =>
            {
                Assert.That(constr.DataSource, Is.EqualTo("myserver"));
                Assert.That(constr.IntegratedSecurity, Is.False);
                Assert.That(constr.UserID, Is.EqualTo("bob"));
                Assert.That(constr.Password, Is.EqualTo("pass"));
            });
        }
        finally
        {
            t.DeleteInDatabase();
            cred?.DeleteInDatabase();
            c.DeleteInDatabase(); //no need to delete ci because of cascades
        }
    }

    [Test]
    public void Test_BlankPasswords()
    {
        var creds = new DataAccessCredentials(CatalogueRepository, "blankpwdCreds")
        {
            Username = "Root",
            Password = ""
        };

        creds.SaveToDatabase();


        var manager = new TableInfoCredentialsManager(CatalogueTableRepository);
        Assert.Multiple(() =>
        {
            Assert.That(manager.GetCredentialByUsernameAndPasswordIfExists("Root", null), Is.EqualTo(creds));
            Assert.That(manager.GetCredentialByUsernameAndPasswordIfExists("Root", ""), Is.EqualTo(creds));
        });
    }

    [Test]
    public void Test_NoDuplicatePasswords()
    {
        var t1 = new TableInfo(CatalogueRepository, "tbl1");
        var t2 = new TableInfo(CatalogueRepository, "tbl2");

        var credCount = CatalogueRepository.GetAllObjects<DataAccessCredentials>().Length;

        //if there is a username then we need to associate it with the TableInfo we just created
        var credentialsFactory = new DataAccessCredentialsFactory(CatalogueRepository);
        var cred = credentialsFactory.Create(t1, "blarg", "flarg", DataAccessContext.Any);
        var cred2 = credentialsFactory.Create(t2, "blarg", "flarg", DataAccessContext.Any);

        Assert.Multiple(() =>
        {
            Assert.That(CatalogueRepository.GetAllObjects<DataAccessCredentials>(), Has.Length.EqualTo(credCount + 1));

            Assert.That(cred2, Is.EqualTo(cred),
                $"Expected {nameof(DataAccessCredentialsFactory)} to reuse existing credentials for both tables as they have the same username/password - e.g. bulk insert");
        });
    }
}
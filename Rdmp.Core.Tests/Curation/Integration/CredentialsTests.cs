// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Managers;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation.Integration
{
    public class CredentialsTests : DatabaseTests
    {
        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            base.OneTimeSetUp();

            foreach (TableInfo table in CatalogueRepository.GetAllObjects<TableInfo>())
            {
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
            }

            foreach (DataAccessCredentials cred in CatalogueRepository.GetAllObjects<DataAccessCredentials>())
            {
                if(cred.Name.Equals("bob")
                    ||
                    cred.Name.Equals("Test")
                    )
                    cred.DeleteInDatabase();
            }


            
        }

        [Test]
        public void CreateNewCredentials()
        {
            var newCredentials = new DataAccessCredentials(CatalogueRepository, "bob");

            try
            {
                Assert.AreEqual("bob", newCredentials.Name);
                Assert.AreNotEqual(0, newCredentials.ID);
            }
            finally
            {
                newCredentials.DeleteInDatabase();
            }
        }


        [Test]
        public void CreateNewCredentialsThenGetByUsernamePasswordCombo()
        {
            var newCredentials = new DataAccessCredentials(CatalogueRepository, "bob");

            newCredentials.Username = "myusername";
            newCredentials.Password = "mypassword";
            newCredentials.SaveToDatabase();

            var newCopy = CatalogueRepository.GetAllObjects<DataAccessCredentials>().SingleOrDefault(c=>c.Username == "myusername");
            Assert.IsNotNull(newCopy);
            
            try
            {
                Assert.NotNull(newCopy);
                Assert.AreEqual(newCredentials.ID, newCopy.ID);
                Assert.AreEqual(newCredentials.Username, newCopy.Username);
                Assert.AreEqual(newCredentials.GetDecryptedPassword(), newCopy.GetDecryptedPassword());
                Assert.AreEqual(newCredentials.Password, newCopy.Password);
            }
            finally
            {
                newCredentials.DeleteInDatabase();
                
            }
        }

        [Test]
        public void TestThe_Any_EnumValue_CannotRequestAnyCredentials()
        {
            TableInfo tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo");
            tableInfo.Name = "My Exciting Table";

            var creds = new DataAccessCredentials(CatalogueRepository);
            try
            {
                creds.Name = "Test";
                creds.SaveToDatabase();

                tableInfo.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
                tableInfo.SaveToDatabase();

                //attempt to request ANY credentials
                var ex = Assert.Throws<Exception>(()=> tableInfo.GetCredentialsIfExists(DataAccessContext.Any));
                Assert.AreEqual("You cannot ask for any credentials, you must supply a usage context.",ex.Message);


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
            TableInfo tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo");
            tableInfo.Name = "My Exciting Table";
            tableInfo.SaveToDatabase();

            var creds = new DataAccessCredentials(CatalogueRepository);
            try
            {
                creds.Name = "Test";
                creds.SaveToDatabase();
                
                                //now create the association as Any
                tableInfo.SetCredentials(creds, DataAccessContext.Any);
                
                //because the credential is liscenced to be used under ANY context, you can make requests under any of the specific contexts and be served the Any result
                var creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
                Assert.NotNull(creds2);
                creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.DataExport);
                Assert.NotNull(creds2);
                creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.DataLoad);
                Assert.NotNull(creds2);

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
            TableInfo tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo");
            tableInfo.Name = "Tableinfo1";
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
                
                
                Assert.AreEqual(creds, tableInfo.GetCredentialsIfExists(DataAccessContext.DataLoad));

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
                Assert.AreEqual(originalCredentials.Name, newCopy.Name);
                Assert.AreEqual(originalCredentials.Username, newCopy.Username);
                Assert.AreEqual(originalCredentials.Password, newCopy.Password);

                //test overridden Equals
                Assert.AreEqual(originalCredentials,newCopy);
                originalCredentials.Password = "fish";
                Assert.AreEqual(originalCredentials, newCopy);//they are still equal because IDs are the same

            }
            finally
            {
                originalCredentials.DeleteInDatabase();
            }
        }

        [Test]
        public void GetCredentialsFromATableInfo()
        {

            TableInfo tableInfo = new TableInfo(CatalogueRepository, "GetCredentialsFromATableInfo");
            tableInfo.Name = "My Exciting Table";

            var creds = new DataAccessCredentials(CatalogueRepository);
            try
            {
                creds.Name = "Test";
                creds.SaveToDatabase();

                tableInfo.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
                tableInfo.SaveToDatabase();

                //Go via TableInfo and get credentials
                DataAccessCredentials creds2 = (DataAccessCredentials)tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
                Assert.AreEqual(creds2.Name, creds.Name);
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
            TableInfo tableInfo1 = new TableInfo(CatalogueRepository, "Dependency1");
            TableInfo tableInfo2 = new TableInfo(CatalogueRepository, "Dependency2");
            var creds = new DataAccessCredentials(CatalogueRepository, "bob");

            try
            {
            
                tableInfo1.SetCredentials(creds,DataAccessContext.InternalDataProcessing);
                tableInfo2.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
                tableInfo1.SaveToDatabase();
                tableInfo2.SaveToDatabase();

                var ex = Assert.Throws<CredentialsInUseException>(creds.DeleteInDatabase);//the bit that fails (because tables are there)
                Assert.AreEqual("Cannot delete credentials bob because it is in use by one or more TableInfo objects(Dependency1,Dependency2)",ex.Message);
            }
            finally
            {
                tableInfo1.DeleteInDatabase();//will work
                tableInfo2.DeleteInDatabase();//will work
                creds.DeleteInDatabase();//will work
            }

            

        }

        [Test]
        public void GetAllUsersOfACredential()
        {

            //Get all TableInfos that share this credential
            TableInfo tableInfo1 = new TableInfo(CatalogueRepository, "Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt1");
            TableInfo tableInfo2 = new TableInfo(CatalogueRepository, "Create2TableInfosThatShareTheSameCredentialAndTestDeletingIt2");
            var creds = new DataAccessCredentials(CatalogueRepository, "bob");

            tableInfo1.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
            tableInfo2.SetCredentials(creds, DataAccessContext.InternalDataProcessing);
            tableInfo1.SaveToDatabase();
            tableInfo2.SaveToDatabase();


            ITableInfo[] TablesThatUseCredential = creds.GetAllTableInfosThatUseThis()[DataAccessContext.InternalDataProcessing].ToArray();

            Assert.Contains(tableInfo1, TablesThatUseCredential);
            Assert.Contains(tableInfo2, TablesThatUseCredential); 

            tableInfo1.DeleteInDatabase();
            tableInfo2.DeleteInDatabase();
            creds.DeleteInDatabase();
        }

        [Test]
        public void GetConnectionStringFromCatalogueWhereOneTableInfoUsesACredentialsOverride()
        {
            Catalogue c = new Catalogue(CatalogueRepository, "GetConnectionStringFromCatalogueWhereOneTableInfoUsesACredentialsOverride");
            CatalogueItem ci = new CatalogueItem(CatalogueRepository, c,"GetConnectionStringFromCatalogueWhereOneTableInfoUsesACredentialsOverride");
            TableInfo t = new TableInfo(CatalogueRepository, "Test");
            ColumnInfo col = new ColumnInfo(CatalogueRepository, "[mydatabase].[dbo].test.col","varchar(10)", t);
            
            var extractionInformation = new ExtractionInformation(CatalogueRepository, ci, col, col.Name);

            DataAccessCredentials cred = null;
            try
            {
                t.Server = "myserver";
                t.Database = "mydatabase";
                
                cred = new DataAccessCredentials(CatalogueRepository, "bob");
                cred.Username = "bob";
                cred.Password = "pass";

                Assert.AreNotEqual("pass",cred.Password);
                Assert.AreEqual("pass", cred.GetDecryptedPassword());


                cred.SaveToDatabase();
                t.SetCredentials(cred, DataAccessContext.InternalDataProcessing);
                t.SaveToDatabase();

                var constr = (SqlConnectionStringBuilder)c.GetDistinctLiveDatabaseServer(DataAccessContext.InternalDataProcessing,false).Builder;
                Assert.AreEqual("myserver",constr.DataSource);
                Assert.False(constr.IntegratedSecurity);
                Assert.AreEqual("bob", constr.UserID);
                Assert.AreEqual("pass", constr.Password);

     
            }
            finally 
            {
                t.DeleteInDatabase();
                if(cred != null)
                    cred.DeleteInDatabase();
                c.DeleteInDatabase();//no need to delete ci because of cascades
                
            }

        }

        [Test]
        public void Test_BlankPasswords()
        {
            var creds = new DataAccessCredentials(CatalogueRepository, "blankpwdCreds");
            creds.Username = "Root";
            creds.Password = "";

            creds.SaveToDatabase();


            var manager = new TableInfoCredentialsManager(CatalogueTableRepository);
            Assert.AreEqual(creds,manager.GetCredentialByUsernameAndPasswordIfExists("Root",null));
            Assert.AreEqual(creds,manager.GetCredentialByUsernameAndPasswordIfExists("Root",""));
        }

        [Test]
        public void Test_NoDuplicatePasswords()
        {
            var t1 = new TableInfo(CatalogueRepository, "tbl1");
            var t2 = new TableInfo(CatalogueRepository, "tbl2");

            var credCount = CatalogueRepository.GetAllObjects<DataAccessCredentials>().Length;

            //if there is a username then we need to associate it with the TableInfo we just created
            DataAccessCredentialsFactory credentialsFactory = new DataAccessCredentialsFactory(CatalogueRepository);
            var cred = credentialsFactory.Create(t1, "blarg", "flarg",DataAccessContext.Any);
            var cred2 = credentialsFactory.Create(t2, "blarg", "flarg", DataAccessContext.Any);

            Assert.AreEqual(credCount + 1, CatalogueRepository.GetAllObjects<DataAccessCredentials>().Length);
            
            Assert.AreEqual(cred, cred2, $"Expected {nameof(DataAccessCredentialsFactory)} to reuse existing credentials for both tables as they have the same username/password - e.g. bulk insert");
        }
    }
}

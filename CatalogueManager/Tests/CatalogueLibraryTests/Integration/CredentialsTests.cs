using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using CatalogueLibrary.Data;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace CatalogueLibraryTests.Integration
{
    public class CredentialsTests : DatabaseTests
    {
        [TestFixtureSetUp]
        public void TestSetup()
        {
            
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

            var newCopy = CatalogueRepository.GetAllObjects<DataAccessCredentials>("WHERE Username='myusername'").SingleOrDefault();
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
        [ExpectedException(ExpectedMessage = "You cannot ask for any credentials, you must supply a usage context",MatchType = MessageMatch.Contains)]
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
                DataAccessCredentials creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.Any);

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
                DataAccessCredentials creds2 = tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
                Assert.AreEqual(creds2.Name, creds.Name);
            }
            finally
            {
                tableInfo.DeleteInDatabase();
                creds.DeleteInDatabase();
            }
        }

        [Test]
        [ExpectedException(ExpectedMessage = "Cannot delete credentials bob because it is in use by one or more TableInfo objects(Dependency1,Dependency2)")]
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

                creds.DeleteInDatabase();//the bit that fails (because tables are there)
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


            TableInfo[] TablesThatUseCredential = creds.GetAllTableInfosThatUseThis()[DataAccessContext.InternalDataProcessing].ToArray();

            Assert.AreEqual(TablesThatUseCredential[0], tableInfo1);
            Assert.AreEqual(TablesThatUseCredential[1], tableInfo2); 

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
            ColumnInfo col = new ColumnInfo(CatalogueRepository, "[mydatabase].dbo.test.col","varchar(10)", t);
            
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
    }
}

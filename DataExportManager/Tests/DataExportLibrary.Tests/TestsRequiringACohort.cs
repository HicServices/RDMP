using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using NUnit.Framework;
using ReusableLibraryCode;
using Tests.Common;

namespace DataExportLibrary.Tests
{
    public class TestsRequiringACohort : DatabaseTests
    {
        protected const int cohortIDInTestData = -599;
        protected const int projectNumberInTestData = 99;
        protected ExternalCohortTable _externalCohortTable;
        protected IExtractableCohort _extractableCohort;

        protected string customTableName;

        protected string cohortTableName = "Cohort";
        protected string definitionTableName = "CohortDefinition";
        protected string customTablesTableName = "CohortCustomData";

        protected string ExternalCohortTableNameInCatalogue = "CohortTests";
        protected readonly string CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
        protected SqlConnectionStringBuilder _externalCohortDetails;
        private Exception _setupException;
        
        /// <summary>
        /// Dictionary of the private and release IDs generated for the cohort, where Keys is a collection of private identifiers and Values are the corresponding release identifiers
        /// </summary>
        protected readonly Dictionary<string, string> _cohortKeysGenerated = new Dictionary<string, string>();

        [TestFixtureSetUp]
        protected override void SetUp()
        {
            try
            {
                base.SetUp();
                
                CreateCohortDatabase();

                _externalCohortDetails = new SqlConnectionStringBuilder(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString);
                _externalCohortDetails.InitialCatalog = CohortDatabaseName;

                EmptyCohortTables();
                SetupCohortDefinitionAndCustomTable();

                CreateExternalCohortTableReference();
                CreateExtractableCohort();

                InsertIntoCohortTable("Priv_12345", "Pub_54321");
                InsertIntoCohortTable("Priv_66666", "Pub_66666");
                InsertIntoCohortTable("Priv_54321", "Pub_12345");
                InsertIntoCohortTable("Priv_66999", "Pub_99666");
                InsertIntoCohortTable("Priv_14722", "Pub_22741");
                InsertIntoCohortTable("Priv_wtf11", "Pub_11ftw");
            }
            catch (Exception e)
            {
                Console.WriteLine(ExceptionHelper.ExceptionToListOfInnerMessages(e));
                _setupException = e;
                throw;
            }            
        }

        [SetUp]
        protected void BeforeEachTest()
        {
            if (_setupException != null)
                throw _setupException;
        }

        private void CreateCohortDatabase()
        {
            //Where {0} is the name of the Cohort database
            //Where {1} is either CHI or ANOCHI depending on whether anonymisation is enabled on the target Catalogue
            //Where {2} is either 10 or 13 -- the column length of either CHI or ANOCHI

            var database = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);
            if (database.Exists())
            {
                database.DiscoverTables(false).ToList().ForEach(t => t.Drop());
                database.Drop();
            }
            
            string sql = string.Format(@"
CREATE DATABASE {0} 
GO

USE {0}

CREATE TABLE [dbo].[Cohort](
       [PrivateID] [char](10) NOT NULL,
       [ReleaseID] [char](10) NOT NULL,
       [cohortDefinition_id] [int] NOT NULL,
CONSTRAINT [PK_Cohort] PRIMARY KEY CLUSTERED 
(
       [PrivateID] ASC,
       [ReleaseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[CohortDefinition](
       [id] [int] IDENTITY(1,1) NOT NULL,
       [projectNumber] [int] NOT NULL,
       [version] [int] NOT NULL,
       [description] [varchar](4000) NOT NULL,
       [dtCreated] [date] NOT NULL,
CONSTRAINT [PK_CohortDefinition] PRIMARY KEY NONCLUSTERED 
(
       [id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[CohortDefinition] ADD  CONSTRAINT [DF_CohortDefinition_dtCreated]  DEFAULT (getdate()) FOR [dtCreated]
GO
ALTER TABLE [dbo].[Cohort]  WITH CHECK ADD  CONSTRAINT [FK_Cohort_CohortDefinition] FOREIGN KEY([cohortDefinition_id])
REFERENCES [dbo].[CohortDefinition] ([id])
GO
ALTER TABLE [dbo].[Cohort] CHECK CONSTRAINT [FK_Cohort_CohortDefinition]
GO

CREATE TABLE [dbo].[CohortCustomData](
	[cohortDefinition_id] [int] NOT NULL,
	[customTableName] [varchar](256) NOT NULL,
	[active] [bit] NOT NULL,
 CONSTRAINT [PK_CohortCustomData] PRIMARY KEY CLUSTERED 
(
	[cohortDefinition_id] ASC,
	[customTableName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[CohortCustomData] ADD  CONSTRAINT [DF_CohortCustomData_active]  DEFAULT ((1)) FOR [active]
GO

ALTER TABLE [dbo].[CohortCustomData]  WITH CHECK ADD  CONSTRAINT [FK_CohortCustomData_CohortDefinition] FOREIGN KEY([cohortDefinition_id])
REFERENCES [dbo].[CohortDefinition] ([id])
GO

ALTER TABLE [dbo].[CohortCustomData] CHECK CONSTRAINT [FK_CohortCustomData_CohortDefinition]
GO
",
                //{0}
CohortDatabaseName);

            SqlConnection con = new SqlConnection(ServerICanCreateRandomDatabasesAndTablesOn.ConnectionString);
            con.Open();
            UsefulStuff.ExecuteBatchNonQuery(sql, con,timeout: 15);
            con.Close();
        }


        private void CreateExternalCohortTableReference()
        {
            ExternalCohortTable alreadyExisting = DataExportRepository.GetAllObjects<ExternalCohortTable>()
                .SingleOrDefault(external => external.Name.Equals(ExternalCohortTableNameInCatalogue));

            if (alreadyExisting != null)
            {
                //remove dependencies cohorts
                ExtractableCohort toCleanup = DataExportRepository.GetAllObjects<ExtractableCohort>().SingleOrDefault(ec => ec.OriginID == cohortIDInTestData);


                if (toCleanup != null)
                {
                    //cleanup any configs that use the cohort
                    foreach (
                        var configToCleanup in DataExportRepository.GetAllObjects<ExtractionConfiguration>()
                                .Where(config => config.Cohort_ID == toCleanup.ID))
                        configToCleanup.DeleteInDatabase();

                    toCleanup.DeleteInDatabase();
                }

                alreadyExisting.DeleteInDatabase();
            }

            var newExternal = new ExternalCohortTable(DataExportRepository, "")
            {
                Database = CohortDatabaseName,
                Server = _externalCohortDetails.DataSource,
                DefinitionTableName = definitionTableName,
                TableName = cohortTableName,
                Name = ExternalCohortTableNameInCatalogue,
                CustomTablesTableName = customTablesTableName,
                Username = _externalCohortDetails.UserID,
                Password = _externalCohortDetails.Password,
                PrivateIdentifierField = "PrivateID",
                ReleaseIdentifierField = "ReleaseID",
                DefinitionTableForeignKeyField = "cohortDefinition_id"
            };

            newExternal.SaveToDatabase();

            _externalCohortTable = newExternal;
        }

        private void CreateExtractableCohort()
        {
            int colsCreated;
            _extractableCohort = new ExtractableCohort(DataExportRepository, _externalCohortTable, cohortIDInTestData,out colsCreated);
            Assert.AreEqual(2, colsCreated);

            Assert.AreEqual(_extractableCohort.OriginID,cohortIDInTestData);

            var cols = _extractableCohort.CustomCohortColumns;
            Assert.IsTrue(cols.Any(c => c.GetRuntimeName().Equals("SuperSecretThing")));
            Assert.IsTrue(cols.Any(c => c.GetRuntimeName().Equals("PrivateID")));
        }

        private void SetupCohortDefinitionAndCustomTable()
        {
            SqlConnection con = new SqlConnection(_externalCohortDetails.ConnectionString);
            con.Open();

            try
            {
                //setup description of cohort (with custom table
                customTableName = "custTable99";

                string insertSQL = "SET IDENTITY_INSERT " + definitionTableName + " ON ;" + Environment.NewLine;
                insertSQL += "INSERT INTO " + definitionTableName +
                             " (id,projectNumber,description,version) VALUES (" + cohortIDInTestData + "," +
                             projectNumberInTestData + ",'unitTestDataForCohort',1)";

                SqlCommand cmdInsert = new SqlCommand(insertSQL, con);
                Assert.AreEqual(1, cmdInsert.ExecuteNonQuery());


                string insertCustomTableDataSQL = "INSERT INTO " + customTablesTableName +
                            " (cohortDefinition_id,customTableName,active) VALUES (" + cohortIDInTestData + ",'" + customTableName + "',1)";

                SqlCommand cmdInsertCustomTableDataSQL = new SqlCommand(insertCustomTableDataSQL, con);
                Assert.AreEqual(1, cmdInsertCustomTableDataSQL.ExecuteNonQuery());
                
                string sqlCreateCustomTable =
                    "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[custTable99]') AND TYPE IN (N'U'))DROP TABLE [dbo].[custTable99];" +
                    Environment.NewLine;

                sqlCreateCustomTable += @"
CREATE TABLE [dbo].[custTable99](
	[SuperSecretThing] [varchar](max) NULL,
	[PrivateID] [varchar](10) NULL
) ON [PRIMARY]

INSERT INTO custTable99 VALUES ('monkeys can all secretly fly','Priv_12345')
INSERT INTO custTable99 VALUES ('the wizard of OZ was a man behind a machine','Priv_wtf11')
";


                SqlCommand createCustomTable = new SqlCommand(sqlCreateCustomTable, con);
                createCustomTable.ExecuteNonQuery();


            }
            finally
            {
                con.Close();
            }
        }



        private void EmptyCohortTables()
        {

            SqlConnection con = new SqlConnection(_externalCohortDetails.ConnectionString);
            con.Open();

            try
            {
                //clear out old data
                SqlCommand cmdDelete =
                    new SqlCommand(
                        "DELETE FROM " + cohortTableName + "; DELETE FROM " + definitionTableName + ";", con);
                cmdDelete.ExecuteNonQuery();
            }
            finally
            {
                con.Close();
            }
        }

        private void InsertIntoCohortTable(string privateID, string publicID)
        {
            _cohortKeysGenerated.Add(privateID,publicID);

            SqlConnection con = new SqlConnection(_externalCohortDetails.ConnectionString);
            con.Open();

            try
            {

                string insertIntoList = "INSERT INTO Cohort(PrivateID,ReleaseID,cohortDefinition_id) VALUES ('" + privateID + "','" + publicID + "'," + cohortIDInTestData + ")";

                SqlCommand insertRecord = new SqlCommand(insertIntoList, con);
                Assert.AreEqual(1, insertRecord.ExecuteNonQuery());
            }
            finally
            {
                con.Close();
            }
        }
        
    }
}
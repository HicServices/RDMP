// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode;

namespace Tests.Common.Scenarios
{
    [TestFixture]
    public class TestsRequiringACohort : TestsRequiringA
    {
        protected const int cohortIDInTestData = -599;
        protected const int projectNumberInTestData = 99;
        protected ExternalCohortTable _externalCohortTable;
        protected IExtractableCohort _extractableCohort;
        
        protected string cohortTableName = "Cohort";
        protected string definitionTableName = "CohortDefinition";

        protected string ExternalCohortTableNameInCatalogue = "CohortTests";
        protected readonly string CohortDatabaseName = TestDatabaseNames.GetConsistentName("CohortDatabase");
        protected DiscoveredDatabase _cohortDatabase;
        private Exception _setupException;

        /// <summary>
        /// Set the Project_ID to your project to make this 'custom data'
        /// </summary>
        protected ExtractableDataSet CustomExtractableDataSet;
        protected ICatalogue CustomCatalogue;
        protected DiscoveredTable CustomTable;
        protected ITableInfo CustomTableInfo;

        /// <summary>
        /// Dictionary of the private and release IDs generated for the cohort, where Keys is a collection of private identifiers and Values are the corresponding release identifiers
        /// </summary>
        protected readonly Dictionary<string, string> _cohortKeysGenerated = new Dictionary<string, string>();

        

        [OneTimeSetUp]
        protected override void OneTimeSetUp()
        {
            try
            {
                base.OneTimeSetUp();
                
                CreateCohortDatabase();
                
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
        protected override void SetUp()
        {
            base.SetUp();

            if (_setupException != null)
                throw _setupException;
        }

        private void CreateCohortDatabase()
        {
            _cohortDatabase = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(CohortDatabaseName);

            if(_cohortDatabase.Exists())
                DeleteTables(_cohortDatabase);
            else
                _cohortDatabase.Create();
            
            string sql = string.Format(@"

CREATE TABLE [dbo].[Cohort](
       [PrivateID] [varchar](10) NOT NULL,
       [ReleaseID] [varchar](10) NULL,
       [cohortDefinition_id] [int] NOT NULL,
CONSTRAINT [PK_Cohort] PRIMARY KEY CLUSTERED 
(
       [PrivateID] ASC,
       [cohortDefinition_id] ASC
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
");

            using (var con = _cohortDatabase.Server.GetConnection())
            {
                con.Open();
                UsefulStuff.ExecuteBatchNonQuery(sql, con, timeout: 15);
                con.Close();
            }
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

            var newExternal = new ExternalCohortTable(DataExportRepository, "TestExternalCohort",DatabaseType.MicrosoftSQLServer)
            {
                Database = CohortDatabaseName,
                Server = _cohortDatabase.Server.Name,
                DefinitionTableName = definitionTableName,
                TableName = cohortTableName,
                Name = ExternalCohortTableNameInCatalogue,
                Username = _cohortDatabase.Server.ExplicitUsernameIfAny,
                Password = _cohortDatabase.Server.ExplicitPasswordIfAny,
                PrivateIdentifierField = "PrivateID",
                ReleaseIdentifierField = "ReleaseID",
                DefinitionTableForeignKeyField = "cohortDefinition_id"
            };

            newExternal.SaveToDatabase();

            _externalCohortTable = newExternal;
        }

        private void CreateExtractableCohort()
        {
            _extractableCohort = new ExtractableCohort(DataExportRepository, _externalCohortTable, cohortIDInTestData);
            Assert.AreEqual(_extractableCohort.OriginID,cohortIDInTestData);
        }

        private void SetupCohortDefinitionAndCustomTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("SuperSecretThing");
            dt.Columns.Add("PrivateID");

            dt.Rows.Add(new[] {"monkeys can all secretly fly", "Priv_12345"});
            dt.Rows.Add(new[] { "the wizard of OZ was a man behind a machine", "Priv_wtf11" });

            CustomTable = _cohortDatabase.CreateTable("custTable99", dt);

            ColumnInfo[] cols;
            new TableInfoImporter(CatalogueRepository, CustomTable).DoImport(out CustomTableInfo, out cols);
            
            CatalogueItem[] cis;
            ExtractionInformation[] eis;
            new ForwardEngineerCatalogue(CustomTableInfo, cols).ExecuteForwardEngineering(out CustomCatalogue, out cis, out eis);

            CustomExtractableDataSet = new ExtractableDataSet(DataExportRepository, CustomCatalogue);
            
            foreach (ExtractionInformation e in eis)
            {
                e.ExtractionCategory = ExtractionCategory.ProjectSpecific;
                
                if (e.GetRuntimeName().Equals("PrivateID"))
                    e.IsExtractionIdentifier = true;

                e.SaveToDatabase();
            }

            using (var con = _cohortDatabase.Server.GetConnection())
            {
                string insertSQL = "SET IDENTITY_INSERT " + definitionTableName + " ON ;" + Environment.NewLine;
                insertSQL += "INSERT INTO " + definitionTableName +
                         " (id,projectNumber,description,version) VALUES (" + cohortIDInTestData + "," +
                         projectNumberInTestData + ",'unitTestDataForCohort',1)";

                con.Open();
                using(var cmd = _cohortDatabase.Server.GetCommand(insertSQL, con))
                    cmd.ExecuteNonQuery();
            }
        }

        
        protected void EmptyCohortTables()
        {

            using (var con = _cohortDatabase.Server.GetConnection())
            {
                con.Open();

                //clear out old data
                using(var cmdDelete =
                    _cohortDatabase.Server.GetCommand(
                        "DELETE FROM " + cohortTableName + "; DELETE FROM " + definitionTableName + ";", con))
                    cmdDelete.ExecuteNonQuery();
            }
        }

        private void InsertIntoCohortTable(string privateID, string publicID)
        {
            _cohortKeysGenerated.Add(privateID,publicID);

            using (var con = _cohortDatabase.Server.GetConnection())
            {
                con.Open();

                string insertIntoList = "INSERT INTO Cohort(PrivateID,ReleaseID,cohortDefinition_id) VALUES ('" + privateID + "','" + publicID + "'," + cohortIDInTestData + ")";

                using(var insertRecord = _cohortDatabase.Server.GetCommand(insertIntoList, con))
                    Assert.AreEqual(1, insertRecord.ExecuteNonQuery());
            }
        }
        
    }
}
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataHelper;
using DataLoadEngine.DataFlowPipeline.Components.Anonymisation;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DataAccess;
using Tests.Common;

namespace AnonymisationTests
{
    public class ANOMigrationTests : TestsRequiringANOStore
    {
        private const string TableName = "ANOMigration";

        private TableInfo _tableInfo;
        private ColumnInfo[] _columnInfos;
        private ANOTable _anoConditionTable;

        #region setup
        [SetUp]
        public void SetupExampleTable()
        {
            var remnantTable = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable("ANOMigration");

            if (remnantTable.Exists())
                remnantTable.Drop();

            DeleteANOEndpoint();
            
            ANOTable remnantANO = CatalogueRepository.GetAllObjects<ANOTable>().SingleOrDefault(a => a.TableName.Equals("ANOCondition"));

            if (remnantANO != null)
                remnantANO.DeleteInDatabase();

            //cleanup
            foreach (var remnant in CatalogueRepository.GetAllObjects<TableInfo>().Where(t => t.GetRuntimeName().Equals(TableName)))
                remnant.DeleteInDatabase();

            const string sql = @"
CREATE TABLE [ANOMigration](
	[AdmissionDate] [datetime] NOT NULL,
	[DischargeDate] [datetime] NOT NULL,
	[Condition1] [varchar](4) NOT NULL,
	[Condition2] [varchar](4) NULL,
	[Condition3] [varchar](4) NULL,
	[Condition4] [varchar](4) NULL,
	[CHI] [varchar](10) NOT NULL
 CONSTRAINT [PK_ANOMigration] PRIMARY KEY CLUSTERED 
(
	[AdmissionDate] ASC,
	[Condition1] ASC,
	[CHI] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000001B300000000 AS DateTime), CAST(0x000001B600000000 AS DateTime), N'Z61', N'Z29', NULL, N'Z11', N'0809003082')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000021D00000000 AS DateTime), CAST(0x0000022600000000 AS DateTime), N'P024', N'Q230', NULL,N'Z11', N'1610007810')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000032900000000 AS DateTime), CAST(0x0000032A00000000 AS DateTime), N'L73', NULL, NULL, NULL, N'2407011022')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000004EA00000000 AS DateTime), CAST(0x000004EA00000000 AS DateTime), N'Y523', N'Z29', NULL, NULL, N'1104015472')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000060300000000 AS DateTime), CAST(0x0000060800000000 AS DateTime), N'F721', N'B871', NULL, NULL, N'0203025927')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000065300000000 AS DateTime), CAST(0x0000065700000000 AS DateTime), N'Z914', N'J398', NULL, NULL, N'2702024715')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000070100000000 AS DateTime), CAST(0x0000070800000000 AS DateTime), N'N009', N'V698', NULL, NULL, N'1610007810')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000077000000000 AS DateTime), CAST(0x0000077200000000 AS DateTime), N'E44', N'J050', N'Q560', NULL, N'1610007810')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000007E800000000 AS DateTime), CAST(0x000007EA00000000 AS DateTime), N'Q824', NULL, NULL, NULL, N'1110029231')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000087700000000 AS DateTime), CAST(0x0000087F00000000 AS DateTime), N'T020', NULL, NULL, NULL, N'2110021261')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000088A00000000 AS DateTime), CAST(0x0000089300000000 AS DateTime), N'G009', NULL, NULL, NULL, N'0706013071')
INSERT [ANOMigration] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000008CA00000000 AS DateTime), CAST(0x000008D100000000 AS DateTime), N'T47', N'H311', N'O037', NULL, N'1204057592')";

            var server = DiscoveredDatabaseICanCreateRandomTablesIn.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql,con).ExecuteNonQuery();
            }

            var table = DiscoveredDatabaseICanCreateRandomTablesIn.ExpectTable(TableName);
            TableInfoImporter importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out _tableInfo,out _columnInfos);

            //Configure the structure of the ANO transform we want - identifiers should have 3 characters and 2 ints and end with _C
            _anoConditionTable = new ANOTable(CatalogueRepository, ANOStore_ExternalDatabaseServer, "ANOCondition","C");
            _anoConditionTable.NumberOfCharactersToUseInAnonymousRepresentation = 3;
            _anoConditionTable.NumberOfIntegersToUseInAnonymousRepresentation = 2;
            _anoConditionTable.SaveToDatabase();
            _anoConditionTable.PushToANOServerAsNewTable("varchar(4)", new ThrowImmediatelyCheckNotifier());
        }

        private void DeleteANOEndpoint()
        {
            var remnantEndpointANOTable = DataAccessPortal.GetInstance()
                .ExpectDatabase(ANOStore_ExternalDatabaseServer, DataAccessContext.InternalDataProcessing)
                .ExpectTable("ANOCondition");

            if (remnantEndpointANOTable.Exists())
                remnantEndpointANOTable.Drop();
        }

        #endregion

        [TearDown]
        public void Cleanup()
        {
            DeleteANOEndpoint();

            var credentials = (DataAccessCredentials)_tableInfo.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);
            _tableInfo.DeleteInDatabase();

            if(credentials != null)
                credentials.DeleteInDatabase();

            _anoConditionTable.DeleteInDatabase();

        }

        [OneTimeTearDown]
        public void FinalTearDown()
        {
            //clear anostore default
            new ServerDefaults(CatalogueRepository).ClearDefault(ServerDefaults.PermissableDefaults.ANOStore);
            //delete the external server reference
            ANOStore_ExternalDatabaseServer.DeleteInDatabase();
        }

        [Test]
        public void PKsAreCorrect()
        {
            Assert.IsTrue(_columnInfos.Single(c=>c.GetRuntimeName().Equals("AdmissionDate")).IsPrimaryKey);
            Assert.IsTrue(_columnInfos.Single(c => c.GetRuntimeName().Equals("Condition1")).IsPrimaryKey);
            Assert.IsTrue(_columnInfos.Single(c => c.GetRuntimeName().Equals("CHI")).IsPrimaryKey);
        }

        [Test]
        public void ConvertPrimaryKeyColumn()
        {
            //The table we created above should have a column called Condition2 in it, we will migrate this data to ANO land
            ColumnInfo condition = _columnInfos.Single(c => c.GetRuntimeName().Equals("Condition1"));
            ColumnInfoToANOTableConverter converter = new ColumnInfoToANOTableConverter(condition, _anoConditionTable,DataAccessPortal.GetInstance());
            var ex = Assert.Throws<Exception>(()=>converter.ConvertFullColumnInfo((s) => true, new ThrowImmediatelyCheckNotifier())); //say  yes to everything it proposes 

            StringAssert.IsMatch(@"Could not perform transformation because column \[(.*)\]\.\.\[.*\]\.\[Condition1\] is not droppable",ex.Message);
        }


        [Test]
        [TestCase("Condition2")]
        [TestCase("Condition3")]
        [TestCase("Condition4")]
        public void ConvertNonPrimaryKeyColumn(string conditionColumn)
        {
            //Value and a list of the rows in which it was found on (e.g. the value 'Fish' was found on row 11, 31, 52 and 501
            Dictionary<object,List<int>> rowsObjectFoundIn = new Dictionary<object, List<int>>();

            var server = DataAccessPortal.GetInstance().ExpectServer(_tableInfo, DataAccessContext.DataLoad);
            //for each object found in the column, record all line numbers that object was seen on
            using (var con = server.GetConnection())
            {
                con.Open();
                DbCommand cmd = server.GetCommand("Select * from " + TableName, con);
                var r = cmd.ExecuteReader();

                for (int row = 0; row < 10000 && r.Read(); row++)
                    //we have seen it before
                    if (rowsObjectFoundIn.ContainsKey(r[conditionColumn]))
                        rowsObjectFoundIn[r[conditionColumn]].Add(row);
                    else
                        rowsObjectFoundIn.Add(r[conditionColumn], new List<int>(new[] {row}));
            }
            
            //The table we created above should have a column called Condition2 in it, we will migrate this data to ANO land
            ColumnInfo condition = _columnInfos.Single(c => c.GetRuntimeName().Equals(conditionColumn));
            ColumnInfoToANOTableConverter converter = new ColumnInfoToANOTableConverter(condition, _anoConditionTable, DataAccessPortal.GetInstance());
            converter.ConvertFullColumnInfo((s) => true,new AcceptAllCheckNotifier()); //say  yes to everything it proposes 

            //refresh the column infos
            ColumnInfo[] columnInfos = _tableInfo.ColumnInfos;

            //there should now be an ANO column in place of Condition2
            var ANOCondition2 = columnInfos.Single(c => c.GetRuntimeName().Equals("ANO" + conditionColumn));
            Assert.AreEqual("varchar(7)",ANOCondition2.GetRuntimeDataType(LoadStage.PostLoad));
            Assert.AreEqual("varchar(4)", ANOCondition2.GetRuntimeDataType(LoadStage.AdjustRaw));  //it should know that it has a different appearance and name in RAW

            Assert.AreEqual("ANO" + conditionColumn, ANOCondition2.GetRuntimeName(LoadStage.PostLoad));
            Assert.AreEqual(conditionColumn, ANOCondition2.GetRuntimeName(LoadStage.AdjustRaw));

            //the old Condition2 column shouldn't exist at all
            Assert.IsFalse(columnInfos.Any(c => c.GetRuntimeName().Equals(conditionColumn)));//this column should be gone

            //Now lets confirm that the data itself was successfully transformed and consistent such that A = X in all locations where it was A before and that Null = Null (i.e. the system didn't decided to anonymise the value Null to Y)
            using (var con = server.GetConnection())
            {
                con.Open();

                DbCommand cmd = server.GetCommand("Select * from " + TableName, con);
                var r = cmd.ExecuteReader();
                
                List<object> objectsFound = new List<object>();

                while (r.Read())
                    objectsFound.Add(r["ANO" + conditionColumn]);

                foreach (List<int> rowsWithIdenticalObjectExpectations in rowsObjectFoundIn.Values)
                    if (rowsWithIdenticalObjectExpectations.Count > 1)
                    {
                        int firstIdx = rowsWithIdenticalObjectExpectations.First();
                        var first = objectsFound[firstIdx];

                        foreach (int rowExpectedToHaveSameObject in rowsWithIdenticalObjectExpectations.Skip(1).ToArray())
                        {
                            Console.WriteLine("Expect row " + firstIdx + " and row " + rowExpectedToHaveSameObject + " to have the same value (because they had the same value before anonymising)");
                            Assert.AreEqual(first, objectsFound[rowExpectedToHaveSameObject]);
                            Console.WriteLine("They did, the ANO was " + (first == DBNull.Value?"DBNull.Value":first));
                        }
                    }
            }
        }
    }
}

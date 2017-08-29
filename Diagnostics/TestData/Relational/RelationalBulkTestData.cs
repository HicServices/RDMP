using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.DataHelper;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.DatabaseHelpers.Discovery.Microsoft;

namespace Diagnostics.TestData.Relational
{
    public class RelationalBulkTestData
    {
        private readonly CatalogueRepository _repository;
        public readonly DiscoveredDatabase Database;
        
        public const string BulkDataTable = "BulkData";

        /*                                          Equipment
         *                                         /
         *                                        /
         *                  Event_Agent----Agent ------ Qualifications
         *                /
         *        Event  / 
         *               \
         *                \ 
         *                   Report --- Informant
         * 
         * 
         * 
         * 
         * */

        Random r = new Random();
        public Catalogue CIATestEventCatalogue;
        public Catalogue CIATestAgentEquipmentCatalogue;
        public Catalogue CIATestEvent_AgentLinkTableCatalogue;
        public Catalogue CIATestAgentCatalogue;
        public Catalogue CIATestReportCatalogue;
        public Catalogue CIATestInformantCatalogue;

        public RelationalBulkTestData(CatalogueRepository repository, DiscoveredDatabase database, int? seed = null)
        {
            _repository = repository;
            Database = database;

            if(seed != null)
                r = new Random((int) seed);
        }

        public void SetupTestData()
        {
            DropTableIfExists("CIATestAgentEquipment");
            DropTableIfExists("CIATestEvent_AgentLinkTable");
            DropTableIfExists("CIATestAgent");

            DropTableIfExists("CIATestReport");
            DropTableIfExists("CIATestInformant");

            DropTableIfExists("CIATestEvent");

            using (var con = Database.Server.GetConnection())
            {
                con.Open();
                UsefulStuff.ExecuteBatchNonQuery(createTables,con);
            }
        }

        public void CommitToDatabase( CIATestEvent[] eventsToCommit,CIATestInformant[] allInformants)
        {
            using (var con = Database.Server.GetConnection())
            {
                con.Open();

                foreach (CIATestInformant informant in allInformants)
                    informant.CommitToDatabase(Database,con);

                foreach (CIATestEvent ciaTestEvent in eventsToCommit)
                    ciaTestEvent.CommitToDatabase(Database, con);
            }
            
        }
        private void DropTableIfExists(string name)
        {
            DiscoveredTable table = Database.ExpectTable(name);

            if(table.Exists())
                table.Drop();
        }

        public void GenerateDataTables(DateTime minDateToBeFoundInBatch, DateTime maxDateToBeFoundInBatch)
        {
            
        }

        public void ImportCatalogues()
        {
            Import("CIATestEvent", ref CIATestEventCatalogue);
            Import("CIATestAgentEquipment", ref CIATestAgentEquipmentCatalogue);
            Import("CIATestEvent_AgentLinkTable", ref CIATestEvent_AgentLinkTableCatalogue);
            Import("CIATestAgent", ref CIATestAgentCatalogue);
            Import("CIATestReport", ref CIATestReportCatalogue);
            Import("CIATestInformant", ref CIATestInformantCatalogue);
        }

        public void DeleteCatalogues()
        {
            foreach (ExtractionInformation information in forCleanupExtractionInformations)
                information.DeleteInDatabase();

            foreach (TableInfo info in forCleanupTableInfos)
            {
                var credentials = info.GetCredentialsIfExists(DataAccessContext.InternalDataProcessing);

                info.DeleteInDatabase();

                try
                {
                    if(credentials != null)
                        credentials.DeleteInDatabase();
                }
                catch (CredentialsInUseException)
                {
                }
            }

            foreach (Catalogue catalogue in forCleanupCatalogues)
                catalogue.DeleteInDatabase();
        }


        List<Catalogue>  forCleanupCatalogues = new List<Catalogue>();
        List<TableInfo> forCleanupTableInfos = new List<TableInfo>();
        List<ExtractionInformation> forCleanupExtractionInformations = new List<ExtractionInformation>();
        


        private void Import(string ciatestevent, ref Catalogue currentCatalogueBeingCreated)
        {

            TableInfoImporter importer = new TableInfoImporter(_repository, Database.ExpectTable(ciatestevent));
            ColumnInfo[] cols;
            TableInfo tableInfoCreated;

            importer.DoImport(out tableInfoCreated, out cols);
            ForwardEngineerCatalogue forwardEngineer = new ForwardEngineerCatalogue(tableInfoCreated, cols, true);
            CatalogueItem[] nvm1;
            ExtractionInformation[] nvm2;
            forwardEngineer.ExecuteForwardEngineering(out currentCatalogueBeingCreated, out nvm1, out nvm2);

            forCleanupCatalogues.Add(currentCatalogueBeingCreated);
            forCleanupTableInfos.Add(tableInfoCreated);
            forCleanupExtractionInformations.AddRange(nvm2);
        }


        public CIATestEvent[] GenerateEvents(DateTime startTime, DateTime endTime, int numberOfEvents, int numberOfAgents, int numberOfInformants, out CIATestInformant[] allInformants)
        {
            
            List<CIATestEvent> toReturn = new List<CIATestEvent>();
            List<CIATestAgent> agents = new List<CIATestAgent>();
            List<CIATestInformant> informants = new List<CIATestInformant>();

            if(numberOfAgents > 10000)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < numberOfAgents; i++)
                agents.Add(new CIATestAgent(r, agents));

            for(int i=0;i<numberOfInformants;i++)
                informants.Add(new CIATestInformant(r,informants));

            allInformants = informants.ToArray();
                 
            for (int i = 0; i < numberOfEvents; i++)
            {
                int numberOfAgentsOnEvent = r.Next(0, 3);
                HashSet<CIATestAgent> agentsOnCase = new HashSet<CIATestAgent>();

                for (int j = 0; j < numberOfAgentsOnEvent; j++)
                    agentsOnCase.Add(agents[r.Next(0, agents.Count)]);

                CIATestClearenceLevel clearence = CIATestClearenceLevel.LessSecret;

                if(r.Next(0, 2) == 1)
                    clearence = CIATestClearenceLevel.TopSecret;

                toReturn.Add(new CIATestEvent(r, agentsOnCase.ToArray(), clearence, toReturn,informants,startTime,endTime));
            }

            return toReturn.ToArray();
        }

        private const string createTables =
            @"
CREATE TABLE [dbo].[CIATestAgent](
	[PKAgentID] [int] NOT NULL,
	[AgentCodeName] [varchar](500) NOT NULL,
 CONSTRAINT [PK_CIATestAgent] PRIMARY KEY CLUSTERED 
(
	[PKAgentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CIATestAgentEquipment]    Script Date: 22/01/2016 14:37:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CIATestAgentEquipment](
	[PKFKAgentID] [int] NOT NULL,
	[PKName] [varchar](100) NOT NULL,
	[AmmoCurrent] [int] NULL,
	[AmmoMax] [int] NULL,
 CONSTRAINT [PK_CIATestAgentEquipment] PRIMARY KEY CLUSTERED 
(
	[PKFKAgentID] ASC,
	[PKName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CIATestEvent]    Script Date: 22/01/2016 14:37:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CIATestEvent](
	[PKAgencyCodename] [varchar](250) NOT NULL,
	[PKClearenceLevel] [varchar](250) NOT NULL,
	[EventName] [varchar](100) NOT NULL,
	[TypeOfEvent] [varchar](100) NOT NULL,
	[EstimatedEventDate] [datetime] NULL,
 CONSTRAINT [PK_CIATestEvent] PRIMARY KEY CLUSTERED 
(
	[PKAgencyCodename] ASC,
	[PKClearenceLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CIATestEvent_AgentLinkTable]    Script Date: 22/01/2016 14:37:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CIATestEvent_AgentLinkTable](
	[PKAgencyCodename] [varchar](250) NOT NULL,
	[PKClearenceLevel] [varchar](250) NOT NULL,
	[PKAgentID] [int] NOT NULL,
 CONSTRAINT [PK_CIATestEvent_AgentLinkTable] PRIMARY KEY CLUSTERED 
(
	[PKAgencyCodename] ASC,
	[PKClearenceLevel] ASC,
	[PKAgentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CIATestInformant]    Script Date: 22/01/2016 14:37:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CIATestInformant](
	[ID] [int] NOT NULL,
	[Name] [varchar](500) NOT NULL,
	[DateOfBirth] [datetime] NOT NULL,
 CONSTRAINT [PK_CIATestInformant] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CIATestReport]    Script Date: 22/01/2016 14:37:58 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CIATestReport](
	[PKID] [int] NOT NULL,
	[ReportText] [varchar](500) NOT NULL,
	[ReportDate] [datetime] NOT NULL,
	[PKFKAgencyCodename] [varchar](250) NOT NULL,
	[PKFKClearenceLevel] [varchar](250) NOT NULL,
	[CIATestInformantSignatory1] [int] NOT NULL,
	[CIATestInformantSignatory2] [int] NULL,
	[CIATestInformantSignatory3] [int] NULL,
 CONSTRAINT [PK_CIATestReport] PRIMARY KEY CLUSTERED 
(
	[PKID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
ALTER TABLE [dbo].[CIATestAgentEquipment]  WITH CHECK ADD  CONSTRAINT [FK_CIATestAgentEquipment_CIATestAgent] FOREIGN KEY([PKFKAgentID])
REFERENCES [dbo].[CIATestAgent] ([PKAgentID])
GO
ALTER TABLE [dbo].[CIATestAgentEquipment] CHECK CONSTRAINT [FK_CIATestAgentEquipment_CIATestAgent]
GO
ALTER TABLE [dbo].[CIATestEvent_AgentLinkTable]  WITH CHECK ADD  CONSTRAINT [FK_CIATestEvent_AgentLinkTable_CIATestAgent] FOREIGN KEY([PKAgentID])
REFERENCES [dbo].[CIATestAgent] ([PKAgentID])
GO
ALTER TABLE [dbo].[CIATestEvent_AgentLinkTable] CHECK CONSTRAINT [FK_CIATestEvent_AgentLinkTable_CIATestAgent]
GO
ALTER TABLE [dbo].[CIATestEvent_AgentLinkTable]  WITH CHECK ADD  CONSTRAINT [FK_CIATestEvent_AgentLinkTable_CIATestEvent] FOREIGN KEY([PKAgencyCodename], [PKClearenceLevel])
REFERENCES [dbo].[CIATestEvent] ([PKAgencyCodename], [PKClearenceLevel])
GO
ALTER TABLE [dbo].[CIATestEvent_AgentLinkTable] CHECK CONSTRAINT [FK_CIATestEvent_AgentLinkTable_CIATestEvent]
GO
ALTER TABLE [dbo].[CIATestReport]  WITH CHECK ADD  CONSTRAINT [FK_CIATestReport_CIATestEvent] FOREIGN KEY([PKFKAgencyCodename], [PKFKClearenceLevel])
REFERENCES [dbo].[CIATestEvent] ([PKAgencyCodename], [PKClearenceLevel])
GO
ALTER TABLE [dbo].[CIATestReport] CHECK CONSTRAINT [FK_CIATestReport_CIATestEvent]
GO
ALTER TABLE [dbo].[CIATestReport]  WITH CHECK ADD  CONSTRAINT [FK_CIATestReport_CIATestInformant] FOREIGN KEY([CIATestInformantSignatory1])
REFERENCES [dbo].[CIATestInformant] ([ID])
GO
ALTER TABLE [dbo].[CIATestReport] CHECK CONSTRAINT [FK_CIATestReport_CIATestInformant]
GO
ALTER TABLE [dbo].[CIATestReport]  WITH CHECK ADD  CONSTRAINT [FK_CIATestReport_CIATestInformant1] FOREIGN KEY([CIATestInformantSignatory2])
REFERENCES [dbo].[CIATestInformant] ([ID])
GO
ALTER TABLE [dbo].[CIATestReport] CHECK CONSTRAINT [FK_CIATestReport_CIATestInformant1]
GO
ALTER TABLE [dbo].[CIATestReport]  WITH CHECK ADD  CONSTRAINT [FK_CIATestReport_CIATestInformant2] FOREIGN KEY([CIATestInformantSignatory3])
REFERENCES [dbo].[CIATestInformant] ([ID])
GO
ALTER TABLE [dbo].[CIATestReport] CHECK CONSTRAINT [FK_CIATestReport_CIATestInformant2]
";

    }


    public enum CIATestClearenceLevel
    {
        TopSecret,
        LessSecret
    }

    public enum CIATestEventType
    {
        Defcon1,
        MinorThreat,
        DangerousJournalism
    }


}
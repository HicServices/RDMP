using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.DataProvider;
using Tests.Common;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.CommandLine.Interactive;
using FAnsi;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Logging;
using Rdmp.Core.Curation;
using System.IO;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    public class DataLoadChainerTests : DatabaseTests
    {
        [Test]
        public void dlc_RunWithNoLoadMetaData()
        {
            var provider = new DataLoadChainer();
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            provider.SetActivator(activator);
            Assert.Throws<Exception>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
        }

        [Test]
        public void dlc_RunWithBadLoadMetaData()
        {
            var fakeDataLoad = NSubstitute.Substitute.For<LoadMetadata>();
            var provider = new DataLoadChainer();
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            provider.SetActivator(activator);
            provider.DataLoad = fakeDataLoad;
            Assert.Throws<ArgumentException>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
        }

        [Test]
        public void dlc_RunWithLoadMetaData()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
if not exists (select * from sysobjects where name='DLCTest' and xtype='U')
    CREATE TABLE [DLCTest](
	    [AdmissionDate] [datetime] NOT NULL,
	    [DischargeDate] [datetime] NOT NULL,
	    [Condition1] [varchar](4) NOT NULL,
	    [Condition2] [varchar](4) NULL,
	    [Condition3] [varchar](4) NULL,
	    [Condition4] [varchar](4) NULL,
	    [CHI] [varchar](10) NOT NULL
     CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
    (
	    [AdmissionDate] ASC,
	    [Condition1] ASC,
	    [CHI] ASC
    )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY]

    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000001B300000000 AS DateTime), CAST(0x000001B600000000 AS DateTime), N'Z61', N'Z29', NULL, N'Z11', N'0809003082')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000021D00000000 AS DateTime), CAST(0x0000022600000000 AS DateTime), N'P024', N'Q230', NULL,N'Z11', N'1610007810')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000032900000000 AS DateTime), CAST(0x0000032A00000000 AS DateTime), N'L73', NULL, NULL, NULL, N'2407011022')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000004EA00000000 AS DateTime), CAST(0x000004EA00000000 AS DateTime), N'Y523', N'Z29', NULL, NULL, N'1104015472')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000060300000000 AS DateTime), CAST(0x0000060800000000 AS DateTime), N'F721', N'B871', NULL, NULL, N'0203025927')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000065300000000 AS DateTime), CAST(0x0000065700000000 AS DateTime), N'Z914', N'J398', NULL, NULL, N'2702024715')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000070100000000 AS DateTime), CAST(0x0000070800000000 AS DateTime), N'N009', N'V698', NULL, NULL, N'1610007810')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000077000000000 AS DateTime), CAST(0x0000077200000000 AS DateTime), N'E44', N'J050', N'Q560', NULL, N'1610007810')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000007E800000000 AS DateTime), CAST(0x000007EA00000000 AS DateTime), N'Q824', NULL, NULL, NULL, N'1110029231')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000087700000000 AS DateTime), CAST(0x0000087F00000000 AS DateTime), N'T020', NULL, NULL, NULL, N'2110021261')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x0000088A00000000 AS DateTime), CAST(0x0000089300000000 AS DateTime), N'G009', NULL, NULL, NULL, N'0706013071')
    INSERT [DLCTest] ([AdmissionDate], [DischargeDate], [Condition1], [Condition2], [Condition3], [Condition4], [CHI]) VALUES (CAST(0x000008CA00000000 AS DateTime), CAST(0x000008D100000000 AS DateTime), N'T47', N'H311', N'O037', NULL, N'1204057592')";

            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }

            var table = db.ExpectTable("DLCTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            var cata = new Catalogue(CatalogueRepository, "Mycata");
            var ci = new CatalogueItem(CatalogueRepository, cata, "MyCataItem");
            var ei = new ExtractionInformation(CatalogueRepository, ci, _columnInfos[0], "MyCataItem");
            var cata2 = ei.CatalogueItem.Catalogue;
            var ti = ei.ColumnInfo.TableInfo;
            ti.Server = server.Name;
            ti.Database = table.Database.GetWrappedName();
            ti.SaveToDatabase();
            var lmd = new LoadMetadata(CatalogueRepository, "MyLoad");
            lmd.LocationOfForLoadingDirectory = Path.GetTempPath();
            lmd.LocationOfForArchivingDirectory = Path.GetTempPath();
            lmd.LocationOfExecutablesDirectory = Path.GetTempPath();
            lmd.LocationOfCacheDirectory = Path.GetTempPath();
            lmd.SaveToDatabase();
            var loggingServer = CatalogueRepository.GetDefaultFor(PermissableDefaults.LiveLoggingServer_ID);
            var logManager = new LogManager(loggingServer);
            logManager.CreateNewLoggingTaskIfNotExists(lmd.Name);
            cata.LoggingDataTask = lmd.Name;
            cata.SaveToDatabase();
            lmd.LinkToCatalogue(cata2);
            var pc = new ProcessTask(CatalogueRepository, lmd, LoadStage.PostLoad);
            pc.ProcessTaskType = ProcessTaskType.DataProvider;
            pc.Path = "Rdmp.Core.DataLoad.Modules.DataProvider.DataLoadChainer";
            pc.SaveToDatabase();
            var pcs = new ProcessTaskArgument(CatalogueRepository, pc)
            {
                Name = "DataLoad",
                Value = lmd.ID.ToString()
            };
            pcs.SetType(typeof(LoadMetadata));
            pcs.SaveToDatabase();
            pcs = new ProcessTaskArgument(CatalogueRepository, pc)
            {
                Name = "AcceptAllCheckNotificationOnRun",
                Value = true.ToString()
            };
            pcs.SetType(typeof(Boolean));
            pcs.SaveToDatabase();

            var lmd2 = new LoadMetadata(CatalogueRepository, "MyLoad");
            lmd2.LocationOfForLoadingDirectory = Path.GetTempPath();
            lmd2.LocationOfForArchivingDirectory = Path.GetTempPath();
            lmd2.LocationOfExecutablesDirectory = Path.GetTempPath();
            lmd2.LocationOfCacheDirectory = Path.GetTempPath();
            lmd2.SaveToDatabase();
            lmd2.LinkToCatalogue(cata2);
            pc = new ProcessTask(CatalogueRepository, lmd2, LoadStage.GetFiles);
            pc.ProcessTaskType = ProcessTaskType.DataProvider;
            pc.Path = "Rdmp.Core.DataLoad.Modules.DataProvider.DoNothingDataProvider";
            pc.SaveToDatabase();

            var provider = new DataLoadChainer();
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            provider.SetActivator(activator);
            provider.DataLoad = lmd2;
            provider.AcceptAllCheckNotificationOnRun = true;
            Assert.DoesNotThrow(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
        }
    }
}

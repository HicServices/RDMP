using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using System.Text.RegularExpressions;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.ReusableLibraryCode.Checks;
using MongoDB.Driver.Core.Servers;
using System.Data;
using System.Diagnostics;
using static MongoDB.Driver.WriteConcern;
using FAnsi.Discovery;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class ExecuteCommandPerformRegexRedactionOnCatalogueTests : DatabaseTests
    {

        [Test]
        public void Redaction_BasicRedactionTest()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [datetime] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
                CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
                (
	               [DischargeDate] ASC,
	               [Condition1] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
              ) ON [PRIMARY]
              INSERT [RedactionTest]([DischargeDate],[Condition1],[Condition2])
              VALUES (CAST(0x000001B300000000 AS DateTime),N'1',N'1234TEST1234')           
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            var retrieveSQL = @"select [Condition2] from [RedactionTest]";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(fetchCmd);
                da.Fill(dt);
            }
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        }


        [Test]
        public void Redaction_OddStringLength()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [datetime] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
                CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
                (
	               [DischargeDate] ASC,
	               [Condition1] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
              ) ON [PRIMARY]
              INSERT [RedactionTest]([DischargeDate],[Condition1],[Condition2])
              VALUES (CAST(0x000001B300000000 AS DateTime),N'1',N'1234TESTT1234')           
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TESTT"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            var retrieveSQL = @"select [Condition2] from [RedactionTest]";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(fetchCmd);
                da.Fill(dt);
            }
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>>1234"));
        }

        [Test]
        public void Redaction_RedactionTooLong()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [datetime] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
                CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
                (
	               [DischargeDate] ASC,
	               [Condition1] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
              ) ON [PRIMARY]
              INSERT [RedactionTest]([DischargeDate],[Condition1],[Condition2])
              VALUES (CAST(0x000001B300000000 AS DateTime),N'1',N'1234TESTT1234')           
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "FARTOOLONG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            var ex = Assert.Throws<Exception>(() => cmd.Execute());
            Assert.That(ex.Message, Is.EqualTo("Redaction string 'FARTOOLONG' is longer than found match 'TEST'."));
        }

        [Test]
        public void Redaction_RedactAPK()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [datetime] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
                CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
                (
	               [DischargeDate] ASC,
	               [Condition1] ASC,
                   [Condition2] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
              ) ON [PRIMARY]
              INSERT [RedactionTest]([DischargeDate],[Condition1],[Condition2])
              VALUES (CAST(0x000001B300000000 AS DateTime),N'1',N'1234TEST1234')           
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            var retrieveSQL = @"select [Condition2] from [RedactionTest]";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(fetchCmd);
                da.Fill(dt);
            }
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
        }

        [Test]
        public void Redaction_NoPKS()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [datetime] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
              )
              INSERT [RedactionTest]([DischargeDate],[Condition1],[Condition2])
              VALUES (CAST(0x000001B300000000 AS DateTime),N'1',N'1234TEST1234')           
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            var ex = Assert.Throws<Exception>(() => cmd.Execute());
            Assert.That(ex.Message, Is.EqualTo($"Unable to identify any primary keys in table '{db.GetWrappedName()}.[dbo].[RedactionTest]'. Redactions cannot be performed on tables without primary keys"));

        }

        [Test]
        public void Redaction_MultipleInOneCell()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            const string sql = @"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [datetime] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
                CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
                (
	               [DischargeDate] ASC,
	               [Condition1] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
              ) ON [PRIMARY]
              INSERT [RedactionTest]([DischargeDate],[Condition1],[Condition2])
              VALUES (CAST(0x000001B300000000 AS DateTime),N'1',N'1234TEST1234TEST1234')           
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "DB", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            var retrieveSQL = @"select [Condition2] from [RedactionTest]";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(fetchCmd);
                da.Fill(dt);
            }
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<DB>1234<DB>1234"));
            var redactions = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("ReplacementValue", "<DB>");
            Assert.That(redactions.Length, Is.EqualTo(2));
            Assert.That(redactions[0].StartingIndex, Is.EqualTo(4));
            Assert.That(redactions[1].StartingIndex, Is.EqualTo(12));
        }


        private string GetInsert(int i)
        {
            return @$"
                ({i}, N'{i}', N'1234TEST1234')";
        }

        private void InsertIntoDB(DiscoveredDatabase db, int start)
        {
            var insertText = string.Join(',', Enumerable.Range(start, 1000).ToArray().Select(i => GetInsert(i)));
            string sql = $@"
                INSERT[RedactionTest]([DischargeDate], [Condition1], [Condition2])
                VALUES
                {insertText}
            ";
            var server = db.Server;
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
        }

        [Test]
        public void Redaction_BulkTest()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var server = db.Server;
            string sql = $@"
              CREATE TABLE [RedactionTest](
                [DischargeDate] [int] NOT NULL,
                [Condition1] [varchar](400) NOT NULL,
                [Condition2] [varchar](400) NOT NULL
                CONSTRAINT [PK_DLCTest] PRIMARY KEY CLUSTERED 
                (
	               [DischargeDate] ASC,
	               [Condition1] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
              ) ON [PRIMARY]
            ";
            using (var con = server.GetConnection())
            {
                con.Open();
                server.GetCommand(sql, con).ExecuteNonQuery();
            }
            var rowCount = 10000;
            foreach(var  i in Enumerable.Range(0,rowCount / 1000)) {
                InsertIntoDB(db, i * 1000);
            }
            var table = db.ExpectTable("RedactionTest");
            var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
            var importer = new TableInfoImporter(CatalogueRepository, table);
            importer.DoImport(out var _tableInfo, out var _columnInfos);
            foreach (var columnInfo in _columnInfos)
            {
                var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
                ci.SaveToDatabase();
                var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
                ei.SaveToDatabase();
            }
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            var timer = Stopwatch.StartNew();
            Assert.DoesNotThrow(() => cmd.Execute());
            timer.Stop();
            var x = timer.ElapsedMilliseconds / 1000;
            Assert.That(timer.ElapsedMilliseconds / 1000, Is.EqualTo(100));// currently ~100persecond
            var retrieveSQL = @"select [Condition2] from [RedactionTest]";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
            {
                using var da = server.GetDataAdapter(fetchCmd);
                da.Fill(dt);
            }
            Assert.That(dt.Rows.Count, Is.EqualTo(rowCount));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        }
    }
}

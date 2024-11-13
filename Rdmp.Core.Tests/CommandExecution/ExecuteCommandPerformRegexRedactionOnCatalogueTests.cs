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

        private DataTable GetRedactionTestDataTable()
        {
            DataTable dt = new();
            dt.Columns.Add("DischargeDate", typeof(DateTime));
            dt.Columns.Add("Condition1", typeof(string));
            dt.Columns.Add("Condition2", typeof(string));
            return dt;
        }


        private (Catalogue, ColumnInfo[]) CreateTable(DiscoveredDatabase db, DataTable dt, bool[] pks = null)
        {
            if (pks is null) pks = new[] { false, true, false };
            db.CreateTable("RedactionTest", dt,new DatabaseColumnRequest[] {
                new DatabaseColumnRequest("DischargeDate","datetime",false){IsPrimaryKey=pks[0]},
                new DatabaseColumnRequest("Condition1","varchar(400)",false){IsPrimaryKey=pks[1]},
                new DatabaseColumnRequest("Condition2","varchar(400)",false){IsPrimaryKey=pks[2]},
            });
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
            return (catalogue, _columnInfos);
        }

        private DataTable Retrieve(DiscoveredDatabase db)
        {
            var retrieveSQL = @"select [Condition2] from [RedactionTest]";
            var dt = new DataTable();
            dt.BeginLoadData();
            using (var fetchCmd = db.Server.GetCommand(retrieveSQL, db.Server.GetConnection()))
            {
                using var da = db.Server.GetDataAdapter(fetchCmd);
                da.Fill(dt);
            }
            return dt;
        }

        [Test]
        public void Redaction_BasicRedactionTest()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
            dt.Rows.Add(new object[] { DateTime.Now, '2', "1234TEST1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt);
            
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            dt = Retrieve(db);
            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
            Assert.That(dt.Rows[1].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        }

        [Test]
        public void Redaction_BasicRedactionTestWithLimit()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
            dt.Rows.Add(new object[] { DateTime.Parse("10-10-2010"), '2', "1234TEST1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt, new[] { true, true, false });
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList(), 1);
            Assert.DoesNotThrow(() => cmd.Execute());
            dt = Retrieve(db);
            Assert.That(dt.Rows.Count, Is.EqualTo(2));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
            Assert.That(dt.Rows[1].ItemArray[0], Is.EqualTo("1234TEST1234"));
        }


        [Test]
        public void Redaction_OddStringLength()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TESTT1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt);
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TESTT"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            dt = Retrieve(db);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>>1234"));
        }

        [Test]
        public void Redaction_RedactionTooLong()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt);
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
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt, new[] { true,true,true});
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            dt = Retrieve(db);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
        }

        [Test]
        public void Redaction_NoPKS()
        {
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt, new[] { false, false, false });
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            var ex = Assert.Throws<Exception>(() => cmd.Execute());
            Assert.That(ex.Message, Is.EqualTo($"Unable to identify any primary keys in table '{db.GetWrappedName()}.[dbo].[RedactionTest]'. Redactions cannot be performed on tables without primary keys"));

        }

        [Test]
        public void Redaction_MultipleInOneCell() { 
        
            var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
            var dt = GetRedactionTestDataTable();
            dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234TEST1234" });
            (Catalogue catalogue, ColumnInfo[] _columnInfos) = CreateTable(db, dt);
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
            var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "DB", "Replace TEST with GG");
            regexConfiguration.SaveToDatabase();
            var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration, _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
            Assert.DoesNotThrow(() => cmd.Execute());
            dt = Retrieve(db);
            Assert.That(dt.Rows.Count, Is.EqualTo(1));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<DB>1234<DB>1234"));
            var redactions = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("ReplacementValue", "<DB>");
            Assert.That(redactions.Length, Is.EqualTo(2));
            Assert.That(redactions[0].StartingIndex, Is.EqualTo(4));
            Assert.That(redactions[1].StartingIndex, Is.EqualTo(12));
        }


        private string GetInsert(int i)
        {
            var text = i % 7 == 0 ? "TEST" : "";
            return @$"
                ({i}, N'{i}', N'1234{text}1234')";
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
    }
}

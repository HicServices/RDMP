using FAnsi;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.Curation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tests.Common;
using Rdmp.Core.Validation.Constraints.Secondary.Predictor;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandRestoreRegexRedactedValueInCatalogueTests: DatabaseTests
{
    [Test]
    public void RedactionRestore_BasicTest()
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
        var redaction = CatalogueRepository.GetAllObjects<RegexRedaction>().Last();
        var revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redaction);
        Assert.DoesNotThrow(() => revert.Execute());
        var foundRedaction = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = new DataTable();
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
    public void RedactionRestore_RestoreTwice()
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
        var redaction = CatalogueRepository.GetAllObjects<RegexRedaction>().Last();
        var revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redaction);
        Assert.DoesNotThrow(() => revert.Execute());
        var foundRedaction = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = new DataTable();
        dt.BeginLoadData();
        using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
        {
            using var da = server.GetDataAdapter(fetchCmd);
            da.Fill(dt);
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
        Assert.DoesNotThrow(() => cmd.Execute());
        dt = new DataTable();
        dt.BeginLoadData();
        using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
        {
            using var da = server.GetDataAdapter(fetchCmd);
            da.Fill(dt);
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        redaction = CatalogueRepository.GetAllObjects<RegexRedaction>().Last();
        revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redaction);
        Assert.DoesNotThrow(() => revert.Execute());
        foundRedaction = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = new DataTable();
        dt.BeginLoadData();
        using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
        {
            using var da = server.GetDataAdapter(fetchCmd);
            da.Fill(dt);
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
    }

    //restore second in string
    [Test]
    public void RedactionRestore_MultipleInOneCell()
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
        //resore the second one
        var revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redactions[1]);
        Assert.DoesNotThrow(() => revert.Execute());
        var foundRedaction = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(1));
        dt = new DataTable();
        dt.BeginLoadData();
        using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
        {
            using var da = server.GetDataAdapter(fetchCmd);
            da.Fill(dt);
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<DB>1234TEST1234"));
        //resore the first one
        revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redactions[0]);
        Assert.DoesNotThrow(() => revert.Execute());
        foundRedaction = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = new DataTable();
        dt.BeginLoadData();
        using (var fetchCmd = server.GetCommand(retrieveSQL, server.GetConnection()))
        {
            using var da = server.GetDataAdapter(fetchCmd);
            da.Fill(dt);
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234TEST1234"));
    }

}

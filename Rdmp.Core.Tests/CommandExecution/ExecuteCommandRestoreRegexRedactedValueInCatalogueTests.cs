using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandRestoreRegexRedactedValueInCatalogueTests : DatabaseTests
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
        db.CreateTable("RedactionTest", dt, new[]
        {
            new DatabaseColumnRequest("DischargeDate", "datetime", false) { IsPrimaryKey = pks[0] },
            new DatabaseColumnRequest("Condition1", "varchar(400)", false) { IsPrimaryKey = pks[1] },
            new DatabaseColumnRequest("Condition2", "varchar(400)", false) { IsPrimaryKey = pks[2] }
        });
        var table = db.ExpectTable("RedactionTest");
        var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
        var importer = new TableInfoImporter(CatalogueRepository, table);
        importer.DoImport(out _, out var _columnInfos);
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
    public void RedactionRestore_BasicTest()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var dt = GetRedactionTestDataTable();
        dt.Rows.Add(DateTime.Now, '1', "1234TEST1234");
        var (catalogue, _columnInfos) = CreateTable(db, dt);

        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"),
            "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration,
            _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        var redaction = CatalogueRepository.GetAllObjects<RegexRedaction>().Last();
        var revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redaction);
        Assert.DoesNotThrow(() => revert.Execute());
        var foundRedaction = CatalogueRepository
            .GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
    }

    [Test]
    public void RedactionRestore_RestoreTwice()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var dt = GetRedactionTestDataTable();
        dt.Rows.Add(DateTime.Now, '1', "1234TEST1234");
        var (catalogue, _columnInfos) = CreateTable(db, dt);

        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"),
            "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration,
            _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        var redaction = CatalogueRepository.GetAllObjects<RegexRedaction>().Last();
        var revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redaction);
        Assert.DoesNotThrow(() => revert.Execute());
        var foundRedaction = CatalogueRepository
            .GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
        cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration,
            _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        redaction = CatalogueRepository.GetAllObjects<RegexRedaction>().Last();
        revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redaction);
        Assert.DoesNotThrow(() => revert.Execute());
        foundRedaction = CatalogueRepository
            .GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
    }

    //restore second in string
    [Test]
    public void RedactionRestore_MultipleInOneCell()
    {
        var db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        var dt = GetRedactionTestDataTable();
        dt.Rows.Add(DateTime.Now, '1', "1234TEST1234TEST1234");
        var (catalogue, _columnInfos) = CreateTable(db, dt);

        var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"),
            "DB", "Replace TEST with DB");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(activator, catalogue, regexConfiguration,
            _columnInfos.Where(c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<DB>1234<DB>1234"));
        var redactions = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("ReplacementValue", "<DB>");
        Assert.That(redactions.Length, Is.EqualTo(2));
        Assert.That(redactions[0].StartingIndex, Is.EqualTo(4));
        Assert.That(redactions[1].StartingIndex, Is.EqualTo(12));
        //restore the second one
        var revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redactions[1]);
        Assert.DoesNotThrow(() => revert.Execute());
        var foundRedaction = CatalogueRepository
            .GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(1));
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<DB>1234TEST1234"));
        //restore the first one
        revert = new ExecuteCommandRestoreRegexRedactedValueInCatalogue(activator, redactions[0]);
        Assert.DoesNotThrow(() => revert.Execute());
        foundRedaction = CatalogueRepository
            .GetAllObjectsWhere<RegexRedaction>("RedactionConfiguration_ID", regexConfiguration.ID).ToList();
        Assert.That(foundRedaction.Count, Is.EqualTo(0));
        dt = Retrieve(db);
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234TEST1234"));
    }
}
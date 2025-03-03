#nullable enable
using FAnsi;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation;
using System;
using System.Linq;
using Tests.Common;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.DataHelper.RegexRedaction;
using System.Text.RegularExpressions;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.Data;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;

namespace Rdmp.Core.Tests.CommandExecution;

public class ExecuteCommandPerformRegexRedactionOnCatalogueTests : DatabaseTests
{
    private static ThrowImmediatelyActivator? _activator;

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private static DiscoveredDatabase _db = null!;
#pragma warning disable NUnit1032
    // ReSharper disable once NullableWarningSuppressionIsUsed
    private static DataTable _dt = null!;
#pragma warning restore NUnit1032
    private static Catalogue? _catalogue;

    private static DataTable GetRedactionTestDataTable()
    {
        DataTable dt = new();
        dt.Columns.Add("DischargeDate", typeof(DateTime));
        dt.Columns.Add("Condition1", typeof(string));
        dt.Columns.Add("Condition2", typeof(string));
        return dt;
    }


    private (Catalogue, ColumnInfo[]) CreateTable(DiscoveredDatabase db, DataTable dt, bool[]? pks = null)
    {
        pks ??= new[] { false, true, false };
        var table = db.CreateTable("RedactionTest", dt, new DatabaseColumnRequest[]
        {
            new("DischargeDate", "datetime", false) { IsPrimaryKey = pks[0] },
            new("Condition1", "varchar(400)", false) { IsPrimaryKey = pks[1] },
            new("Condition2", "varchar(400)", false) { IsPrimaryKey = pks[2] },
        });
        var catalogue = new Catalogue(CatalogueRepository, "RedactionTest");
        var importer = new TableInfoImporter(CatalogueRepository, table);
        importer.DoImport(out _, out var columnInfos);
        foreach (var columnInfo in columnInfos)
        {
            var ci = new CatalogueItem(CatalogueRepository, catalogue, columnInfo.GetRuntimeName());
            ci.SaveToDatabase();
            var ei = new ExtractionInformation(CatalogueRepository, ci, columnInfo, "");
            ei.SaveToDatabase();
        }

        return (catalogue, columnInfos);
    }

    private static DataTable Retrieve(DiscoveredDatabase db)
    {
        var helper = db.Server.GetQuerySyntaxHelper();
        var retrieveSQL = $"select {helper.EnsureWrapped("Condition2")} from {helper.EnsureWrapped("RedactionTest")}";
        var dt = new DataTable();
        dt.BeginLoadData();
        using var fetchCmd = db.Server.GetCommand(retrieveSQL, db.Server.GetConnection());
        using var da = db.Server.GetDataAdapter(fetchCmd);
        da.Fill(dt);
        return dt;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _db = GetCleanedServer(DatabaseType.MicrosoftSQLServer);
        _dt = GetRedactionTestDataTable();
        _activator = new ThrowImmediatelyActivator(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _dt.Dispose();
    }

    [SetUp]
    public new void SetUp()
    {
        _dt.Rows.Clear();
    }

    [TearDown]
    public new void TearDown()
    {
        var t = _db.ExpectTable("RedactionTest");
        if (t.Exists()) t.Drop();
        _catalogue?.DeleteInDatabase();
    }

    [Test]
    public void Redaction_BasicRedactionTest()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
        _dt.Rows.Add(new object[] { DateTime.Now, '2', "1234TEST1234" });
        (_catalogue, var columnInfos) = CreateTable(_db, _dt);

        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());

        using var dt = Retrieve(_db);
        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows, Has.Count.EqualTo(2));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
            Assert.That(dt.Rows[1].ItemArray[0], Is.EqualTo("1234<GG>1234"));
        });
    }

    [Test]
    public void Redaction_BasicRedactionTestWithLimit()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
        _dt.Rows.Add(new object[] { DateTime.Parse("10-10-2010"), '2', "1234TEST1234" });
        (_catalogue, var columnInfos) = CreateTable(_db, _dt, new[] { true, true, false });

        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList(), 1);
        Assert.DoesNotThrow(() => cmd.Execute());
        using var dt = Retrieve(_db);
        Assert.Multiple(() =>
        {
            Assert.That(dt.Rows, Has.Count.EqualTo(2));
            Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>1234"));
            Assert.That(dt.Rows[1].ItemArray[0], Is.EqualTo("1234TEST1234"));
        });
    }


    [Test]
    public void Redaction_OddStringLength()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TESTT1234" });
        (_catalogue, var columnInfos) = CreateTable(_db, _dt);
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TESTT"), "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        using var dt = Retrieve(_db);
        Assert.That(dt.Rows, Has.Count.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<GG>>1234"));
    }

    [Test]
    public void Redaction_RedactionTooLong()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
        var (catalogue, columnInfos) = CreateTable(_db, _dt);
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "FARTOOLONG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList());
        var ex = Assert.Throws<Exception>(() => cmd.Execute());
        Assert.That(ex?.Message, Is.EqualTo("Redaction string 'FARTOOLONG' is longer than found match 'TEST'."));
    }

    [Test]
    public void Redaction_RedactAPK()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
        (_catalogue, var columnInfos) = CreateTable(_db, _dt, new[] { true,true,true});
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        using var dt = Retrieve(_db);
        Assert.That(dt.Rows, Has.Count.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234TEST1234"));
    }

    [Test]
    public void Redaction_NoPKS()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234" });
        (_catalogue, var columnInfos) = CreateTable(_db, _dt, new[] { false, false, false });
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "GG", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList());
        var ex = Assert.Throws<Exception>(() => cmd.Execute());
        Assert.That(ex?.Message, Is.EqualTo($"Unable to identify any primary keys in table '{_db.GetWrappedName()}.[dbo].[RedactionTest]'. Redactions cannot be performed on tables without primary keys"));
    }

    [Test]
    public void Redaction_MultipleInOneCell()
    {
        _dt.Rows.Add(new object[] { DateTime.Now, '1', "1234TEST1234TEST1234" });
        (_catalogue, var columnInfos) = CreateTable(_db, _dt);
        var regexConfiguration = new RegexRedactionConfiguration(CatalogueRepository, "TestReplacer", new Regex("TEST"), "DB", "Replace TEST with GG");
        regexConfiguration.SaveToDatabase();
        var cmd = new ExecuteCommandPerformRegexRedactionOnCatalogue(_activator, _catalogue, regexConfiguration, columnInfos.Where(static c => c.GetRuntimeName() == "Condition2").ToList());
        Assert.DoesNotThrow(() => cmd.Execute());
        using var dt = Retrieve(_db);
        Assert.That(dt.Rows, Has.Count.EqualTo(1));
        Assert.That(dt.Rows[0].ItemArray[0], Is.EqualTo("1234<DB>1234<DB>1234"));
        var redactions = CatalogueRepository.GetAllObjectsWhere<RegexRedaction>("ReplacementValue", "<DB>");
        Assert.That(redactions, Has.Length.EqualTo(2));
        Assert.That(redactions[0].StartingIndex, Is.EqualTo(4));
        Assert.That(redactions[1].StartingIndex, Is.EqualTo(12));
    }
}

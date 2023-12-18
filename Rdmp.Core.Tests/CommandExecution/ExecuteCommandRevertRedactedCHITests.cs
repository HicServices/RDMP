using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using System.Linq;
using Tests.Common;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.Data;
using System;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandRevertRedactedCHITests : DatabaseTests
{
    [Test]
    public void RevertRedactedCHI_Valid()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });
        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (1) {tbl.GetRuntimeName()} set SampleType = 'R1111111111'";
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var updateCmd = tbl.Database.Server.GetCommand(updateSQL, con);
            updateCmd.ExecuteNonQuery();
        }
        var cmd = new ExecuteCommandRedactCHIsFromCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        var dt = new DataTable();
        using (var con = tbl.Database.Server.GetConnection())
        {
            using (var findCmd = tbl.Database.Server.GetCommand($"select * from {tbl.GetRuntimeName()} where SampleType = 'R#########1'", con))
            {
                using var da = tbl.Database.Server.GetDataAdapter(findCmd);
                da.Fill(dt);
            }
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        var redactedChisCount = CatalogueRepository.GetAllObjects<RedactedCHI>().Length;
        var rCHI = CatalogueRepository.GetAllObjects<RedactedCHI>().First();
        var revertCmd = new ExecuteCommandRevertRedactedCHI(new ThrowImmediatelyActivator(RepositoryLocator), rCHI);
        Assert.DoesNotThrow(() => revertCmd.Execute());
        Assert.That(redactedChisCount, Is.EqualTo(CatalogueRepository.GetAllObjects<RedactedCHI>().Length +1));
        dt = new DataTable();
        using (var con = tbl.Database.Server.GetConnection())
        {
            using (var findCmd = tbl.Database.Server.GetCommand($"select * from {tbl.GetRuntimeName()} where SampleType = 'R1111111111'", con))
            {
                using var da = tbl.Database.Server.GetDataAdapter(findCmd);
                da.Fill(dt);
            }
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        dt.Dispose();
    }

    [Test]
    public void RevertRedactedCHI_InValid()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var pipes = new CataloguePipelinesAndReferencesCreation(RepositoryLocator, null, null);
        pipes.CreatePipelines(new PlatformDatabaseCreationOptions());
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });
        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (1) {tbl.GetRuntimeName()} set SampleType = 'R1111111111'";
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var updateCmd = tbl.Database.Server.GetCommand(updateSQL, con);
            updateCmd.ExecuteNonQuery();
        }
        var cmd = new ExecuteCommandRedactCHIsFromCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        var dt = new DataTable();
        using (var con = tbl.Database.Server.GetConnection())
        {
            using (var findCmd = tbl.Database.Server.GetCommand($"select * from {tbl.GetRuntimeName()} where SampleType = 'R#########1'", con))
            {
                using var da = tbl.Database.Server.GetDataAdapter(findCmd);
                da.Fill(dt);
            }
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        dt.Dispose();
        var redactedChisCount = CatalogueRepository.GetAllObjects<RedactedCHI>().Length;
        var rCHI = CatalogueRepository.GetAllObjects<RedactedCHI>().First();
        var revertCmd = new ExecuteCommandRevertRedactedCHI(new ThrowImmediatelyActivator(RepositoryLocator), rCHI);
        Assert.DoesNotThrow(() => revertCmd.Execute());
        Assert.DoesNotThrow(() => revertCmd.Execute());
        Assert.That(redactedChisCount, Is.EqualTo(CatalogueRepository.GetAllObjects<RedactedCHI>().Length + 1));
        dt = new DataTable();
        using (var con = tbl.Database.Server.GetConnection())
        {
            using (var findCmd = tbl.Database.Server.GetCommand($"select * from {tbl.GetRuntimeName()} where SampleType = 'R1111111111'", con))
            {
                using var da = tbl.Database.Server.GetDataAdapter(findCmd);
                da.Fill(dt);
            }
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(1));
        dt.Dispose();

    }
    }

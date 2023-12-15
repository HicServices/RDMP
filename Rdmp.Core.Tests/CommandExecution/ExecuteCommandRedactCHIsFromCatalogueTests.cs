using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.Data;
using System.IO;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandRedactCHIsFromCatalogueTests: DatabaseTests
{

    [Test]
    public void RedactCHIsFromCatalogue_ValidSingle()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });
        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (2) {tbl.GetRuntimeName()} set SampleType = 'F1111111111'";
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
            using (var findCmd = tbl.Database.Server.GetCommand($"select * from {tbl.GetRuntimeName()} where SampleType = 'F#########1'", con))
            {
                using var da = tbl.Database.Server.GetDataAdapter(findCmd);
                da.Fill(dt);
            }
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(2));
        dt.Dispose();
    }

    [Test]
    public void RedactCHIsFromCatalogue_Allowlist()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var pipes = new CataloguePipelinesAndReferencesCreation(RepositoryLocator, null, null);
        pipes.CreatePipelines(new PlatformDatabaseCreationOptions());
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });
        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (2) {tbl.GetRuntimeName()} set SampleType = 'F1111111111'";
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var updateCmd = tbl.Database.Server.GetCommand(updateSQL, con);
            updateCmd.ExecuteNonQuery();
        }
        var tempFileToCreate = Path.Combine(TestContext.CurrentContext.TestDirectory, "allowList.yaml");
        var allowList = File.Create(tempFileToCreate);
        allowList.Close();
        File.WriteAllLines(tempFileToCreate, new string[] { "Biochemistry:", "- SampleType" });
        var cmd = new ExecuteCommandRedactCHIsFromCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, tempFileToCreate);
        Assert.DoesNotThrow(() => cmd.Execute());
        var dt = new DataTable();
        using (var con = tbl.Database.Server.GetConnection())
        {
            using (var findCmd = tbl.Database.Server.GetCommand($"select * from {tbl.GetRuntimeName()} where SampleType = 'F#########1'", con))
            {
                using var da = tbl.Database.Server.GetDataAdapter(findCmd);
                da.Fill(dt);
            }
        }
        Assert.That(dt.Rows.Count, Is.EqualTo(0));
        dt.Dispose();
    }
}

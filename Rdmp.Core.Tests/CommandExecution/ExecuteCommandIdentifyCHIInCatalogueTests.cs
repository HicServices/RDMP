using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System.IO;
using System.Linq;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandIdentifyCHIInCatalogueTests : DatabaseTests
{

    [Test]
    public void IdentifyCHIInCatalogue_Correct()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var pipes = new CataloguePipelinesAndReferencesCreation(RepositoryLocator, null, null);
        pipes.CreatePipelines(new PlatformDatabaseCreationOptions());
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });
        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var cmd = new ExecuteCommandIdentifyCHIInCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, false, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(cmd.foundChis.Rows.Count, Is.EqualTo(0));
    }

    [Test]
    public void IdentifyCHIInCatalogue_FoundCHI()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });


        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (2) {tbl.GetRuntimeName()} set SampleType = '1111111111'";
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var updateCmd = tbl.Database.Server.GetCommand(updateSQL, con);
            updateCmd.ExecuteNonQuery();
        }

        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var cmd = new ExecuteCommandIdentifyCHIInCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, false, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(cmd.foundChis.Rows.Count, Is.EqualTo(2));
    }

    [Test]
    public void IdentifyCHIInCatalogue_FoundCHI_BailOuit()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });


        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (2) {tbl.GetRuntimeName()} set SampleType = '1111111111'";
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var updateCmd = tbl.Database.Server.GetCommand(updateSQL, con);
            updateCmd.ExecuteNonQuery();
        }

        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var cmd = new ExecuteCommandIdentifyCHIInCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, true, null);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(cmd.foundChis.Rows.Count, Is.EqualTo(1));
    }

    [Test]
    public void IdentifyCHIInCatalogue_FoundCHI_AllowList()
    {
        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
        var creator = new ExampleDatasetsCreation(new ThrowImmediatelyActivator(RepositoryLocator), RepositoryLocator);
        creator.Create(db, ThrowImmediatelyCheckNotifier.Quiet,
            new PlatformDatabaseCreationOptions { Seed = 500, DropDatabases = true });


        var tbl = db.DiscoverTables(true).Where(dt => dt.GetRuntimeName().Contains("Biochemistry")).First();
        var updateSQL = $"UPDATE top (2) {tbl.GetRuntimeName()} set SampleType = '1111111111'";
        using (var con = tbl.Database.Server.GetConnection())
        {
            con.Open();
            var updateCmd = tbl.Database.Server.GetCommand(updateSQL, con);
            updateCmd.ExecuteNonQuery();
        }

        var biochemistry = CatalogueRepository.GetAllObjects<Catalogue>().Where(c => c.Name == "Biochemistry").First();
        var tempFileToCreate = Path.Combine(TestContext.CurrentContext.TestDirectory, "allowList.yaml");
        var allowList = File.Create(tempFileToCreate);
        allowList.Close();
        File.WriteAllLines(tempFileToCreate, new string[] { "Biochemistry:", "- SampleType" });
        var cmd = new ExecuteCommandIdentifyCHIInCatalogue(new ThrowImmediatelyActivator(RepositoryLocator), biochemistry, false, tempFileToCreate);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(cmd.foundChis.Rows.Count, Is.EqualTo(0));
    }

}

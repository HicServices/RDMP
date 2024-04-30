using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

internal class ExecuteCommandConfirmRedactedCHITests: DatabaseTests
{
    [Test]
    public void ConfirmRedactedCHI_Valid() {
        var redactedChisCount = CatalogueRepository.GetAllObjects<RedactedCHI>().Length;
        var rCHI = new RedactedCHI(CatalogueRepository, "1111111111", 0, "[fake].[table]","pkValue","[pkColumn]","[columnName]");
        rCHI.SaveToDatabase();

        var cmd = new ExecuteCommandConfirmRedactedCHI(new ThrowImmediatelyActivator(RepositoryLocator), rCHI);
        Assert.DoesNotThrow(()=> cmd.Execute());
        Assert.That(redactedChisCount, Is.EqualTo(CatalogueRepository.GetAllObjects<RedactedCHI>().Length));
    }

    [Test]
    public void ConfirmRedactedCHI_InValid() {
        var redactedChisCount = CatalogueRepository.GetAllObjects<RedactedCHI>().Length;
        var rCHI = new RedactedCHI(CatalogueRepository, "1111111111", 0, "[fake].[table]", "pkValue", "[pkColumn]", "[columnName]");
        rCHI.DeleteInDatabase();

        var cmd = new ExecuteCommandConfirmRedactedCHI(new ThrowImmediatelyActivator(RepositoryLocator), rCHI);
        Assert.DoesNotThrow(() => cmd.Execute());
        Assert.That(redactedChisCount, Is.EqualTo(CatalogueRepository.GetAllObjects<RedactedCHI>().Length));
    }
}

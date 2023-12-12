//using NUnit.Framework;
//using Rdmp.Core.CommandExecution.AtomicCommands;
//using Rdmp.Core.CommandExecution;
//using Rdmp.Core.Curation.Data;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Tests.Common;

//namespace Rdmp.Core.Tests.CommandExecution;

//internal class ExecuteCommandRevertRedactedCHITests: DatabaseTests
//{
//    [Test]
//    public void RevertRedactedCHI_Valid()
//    {
//        var redactedChisCount = CatalogueRepository.GetAllObjects<RedactedCHI>().Length;
//        var rCHI = new RedactedCHI(CatalogueRepository, "1111111111", 0, "[fake],[table]", "pkValue", "[pkColumn]", "[columnName]");
//        rCHI.SaveToDatabase();
//        //needs a columinfo and read data to update

//        var cmd = new ExecuteCommandRevertRedactedCHI(new ThrowImmediatelyActivator(RepositoryLocator), rCHI);
//        Assert.DoesNotThrow(() => cmd.Execute());
//        Assert.That(redactedChisCount, Is.EqualTo(CatalogueRepository.GetAllObjects<RedactedCHI>().Length));
//    }

//    [Test]
//    public void RevertRedactedCHI_InValid()
//    {
//        var redactedChisCount = CatalogueRepository.GetAllObjects<RedactedCHI>().Length;
//        var rCHI = new RedactedCHI(CatalogueRepository, "1111111111", 0, "[fake],[table]", "pkValue", "[pkColumn]", "[columnName]");
//        rCHI.DeleteInDatabase();
//        var cmd = new ExecuteCommandRevertRedactedCHI(new ThrowImmediatelyActivator(RepositoryLocator), rCHI);
//        Assert.DoesNotThrow(() => cmd.Execute());
//        Assert.That(redactedChisCount, Is.EqualTo(CatalogueRepository.GetAllObjects<RedactedCHI>().Length));
//    }
//}

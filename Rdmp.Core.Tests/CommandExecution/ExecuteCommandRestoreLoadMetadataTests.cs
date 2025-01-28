using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Tests.Common;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Linq;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.CommandExecution;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class ExecuteCommandRestoreLoadMetadataTests : DatabaseTests
    {

        [Test]
        public void TestRestoreLoadMetadataVersion()
        {
            var lmd1 = new LoadMetadata(CatalogueRepository, "MyLmd");
            lmd1.Description = "Desc!";
            var cata = new Catalogue(CatalogueRepository, "myCata")
            {
                LoggingDataTask = "B"
            };
            cata.SaveToDatabase();
            lmd1.LinkToCatalogue(cata);
            var pt1 = new ProcessTask(CatalogueRepository, lmd1, LoadStage.Mounting)
            {
                ProcessTaskType = ProcessTaskType.Attacher,
                LoadStage = LoadStage.Mounting,
                Path = typeof(AnySeparatorFileAttacher).FullName
            };
            pt1.SaveToDatabase();

            pt1.CreateArgumentsForClassIfNotExists(typeof(AnySeparatorFileAttacher));
            var pta = pt1.ProcessTaskArguments.Single(pt => pt.Name == "Separator");
            pta.SetValue(",");
            pta.SaveToDatabase();
            LoadMetadata clonedLmd;
            clonedLmd = (LoadMetadata)lmd1.SaveNewVersion();
            Assert.That(clonedLmd.ProcessTasks.Count(), Is.EqualTo(1));
            Assert.That(clonedLmd.RootLoadMetadata_ID, Is.EqualTo(lmd1.ID));
            Assert.That(clonedLmd.Description, Is.EqualTo(lmd1.Description));
            Assert.That(clonedLmd.ProcessTasks.First().ProcessTaskArguments.First().Value, Is.EqualTo(lmd1.ProcessTasks.First().ProcessTaskArguments.First().Value));
            pt1.DeleteInDatabase();
            var fetchedlmd = CatalogueRepository.GetObjectByID<LoadMetadata>(lmd1.ID);
            Assert.That(fetchedlmd.ProcessTasks.Count(), Is.EqualTo(0));
            var activator = new ThrowImmediatelyActivator(RepositoryLocator);
            var cmd = new ExecuteCommandRestoreLoadMetadataVersion(activator, clonedLmd);
            Assert.DoesNotThrow(()=>cmd.Execute());
            fetchedlmd = CatalogueRepository.GetObjectByID<LoadMetadata>(lmd1.ID);
            Assert.That(fetchedlmd.ProcessTasks.Count(), Is.EqualTo(1));
        }
    }
}

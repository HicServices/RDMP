using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Tests.Common;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using System.Linq;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class ExecuteCommandCloneLoadMetadataTests: DatabaseTests
    {

        [Test]
        public void TestCloneLoadMetadata()
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
            clonedLmd = lmd1.Clone();
            Assert.That(clonedLmd.ProcessTasks.Count(), Is.EqualTo(1));
            Assert.That(clonedLmd.Description, Is.EqualTo(lmd1.Description));
            Assert.That(clonedLmd.ProcessTasks.First().ProcessTaskArguments.First().Value, Is.EqualTo(lmd1.ProcessTasks.First().ProcessTaskArguments.First().Value));
        }
    }
}

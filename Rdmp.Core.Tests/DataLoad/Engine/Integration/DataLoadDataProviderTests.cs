using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.DataProvider;
using NSubstitute;
using Tests.Common;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    public  class DataLoadDataProviderTests: DatabaseTests
    {
        [Test]
        public void RunWithNoLoadMetaData()
        {
            //var fakeDataLoad = Substitute.For<LoadMetadata>();
            var provider = new DataLoadChainer();
            Assert.Throws<Exception>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
            //provider.DataLoad = fakeDataLoad;
        }

        [Test]
        public void RunWithBadLoadMetaData()
        {
            var fakeDataLoad = Substitute.For<LoadMetadata>();
            var provider = new DataLoadChainer();
            provider.DataLoad = fakeDataLoad;
            Assert.Throws<ArgumentException>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
        }

        [Test]
        public void RunWithLoadMetaData()
        {
            var lmd = new LoadMetadata(CatalogueRepository,"dldplmd");
            lmd.SaveToDatabase();
            var cata = new Catalogue(CatalogueRepository, "myCata")
            {
                LoggingDataTask = "A"
            };
            cata.SaveToDatabase();
            var linkage2 = new LoadMetadataCatalogueLinkage(CatalogueRepository, lmd, cata);
            linkage2.SaveToDatabase();
            var provider = new DataLoadChainer();
            provider.DataLoad = lmd;
            Assert.Throws<ArgumentException>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
        }

        //[Test]
        //public void BadCatalogueConnectionString()
        //{
        //    var fakeDataLoad = Substitute.For<LoadMetadata>();
        //    var provider = new DataLoadDataProvider();
        //    provider.DataLoad = fakeDataLoad;
        //    UserSettings.CatalogueConnectionString = null;

        //    Assert.Throws<ArgumentException>(() => provider.Check(ThrowImmediatelyCheckNotifier.Quiet));
        //}
    }
}

using NUnit.Framework;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandLine
{
    class ExampleDatasetsCreationTests:DatabaseTests
    {
        /// <summary>
        /// Tests the creation of example datasets during first installation of RDMP or when running "rdmp.exe install [...] -e" from the CLI
        /// </summary>
        [Test]
        public void Test_ExampleDatasetsCreation()
        {
            //Should be empty RDMP metadata database
            Assert.AreEqual(0,CatalogueRepository.GetAllObjects<Catalogue>().Length);
            Assert.AreEqual(0,CatalogueRepository.GetAllObjects<AggregateConfiguration>().Length);

            //create all the stuff
            var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);
            var creator = new ExampleDatasetsCreation(RepositoryLocator);
            creator.Create(db,true,new ThrowImmediatelyCheckNotifier());

            //should have at least created some catalogues, graphs etc
            Assert.GreaterOrEqual(CatalogueRepository.GetAllObjects<Catalogue>().Length,4);
            Assert.GreaterOrEqual(CatalogueRepository.GetAllObjects<AggregateConfiguration>().Length,4);
        }
    }
}

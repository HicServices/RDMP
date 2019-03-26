using System.Linq;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using NUnit.Framework;

namespace DataExportLibrary.Tests
{
    public class ProjectCohortIdentificationConfigurationAssociationTests
    {
    
        [Test]
        public void TestOrphanCic()
        {
            var memory = new MemoryDataExportRepository();
            var cic = new CohortIdentificationConfiguration(memory, "Mycic");
            var p = new Project(memory,"my proj");
            p.AssociateWithCohortIdentification(cic);

            //fetch the instance
            var cicAssoc = memory.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>().Single();
            
            //relationship from p should resolve to the association link
            Assert.AreEqual(cicAssoc,p.ProjectCohortIdentificationConfigurationAssociations[0]);
            
            //relationship from p should resolve to the cic
            Assert.AreEqual(cic, p.GetAssociatedCohortIdentificationConfigurations()[0]);

            //make the assoc an orphan
            cic.DeleteInDatabase();
            cicAssoc.ClearAllInjections();

            //assoc should still exist
            Assert.AreEqual(cicAssoc, p.ProjectCohortIdentificationConfigurationAssociations[0]);
            Assert.IsNull(p.ProjectCohortIdentificationConfigurationAssociations[0].CohortIdentificationConfiguration);
            
            //relationship from p should resolve to the cic
            Assert.IsEmpty( p.GetAssociatedCohortIdentificationConfigurations());

        }
    }
}
using MathNet.Numerics;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CohortCreationCommands;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.CohortCreation
{
    public class CohortIdentificationConfigurationTemplateTests: DatabaseTests
    {
        private  static readonly Random random = new();
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private CohortIdentificationConfiguration CreateBaseCIC(string name)
        {
            var cic = new CohortIdentificationConfiguration(CatalogueRepository, name);
            cic.CreateRootContainerIfNotExists();
            cic.SaveToDatabase();
            return cic;
        }
        [Test]
        public void CreateTemplateFromCIC_Test()
        {
            var cohortName = RandomString(10);
            var cic = CreateBaseCIC(cohortName);
            var cmd = new ExecuteCommandCreateCohortIdentificationConfigurationTemplate(new ThrowImmediatelyActivator(RepositoryLocator), cic);
            Assert.DoesNotThrow(()=>cmd.Execute());
            var foundTemplate = RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", $"{cohortName} Template");
            Assert.That(foundTemplate.Length,Is.EqualTo(1));
        }

        [Test]
        public void CreateTemplateFromCICWithProject_Test()
        {
            var cohortName = RandomString(10);
            var cic = CreateBaseCIC(cohortName);
            var project = new Project(DataExportRepository, RandomString(10));
            project.SaveToDatabase();
            var cmd = new ExecuteCommandCreateCohortIdentificationConfigurationTemplate(new ThrowImmediatelyActivator(RepositoryLocator), cic);
            cmd.SetTarget(project);
            Assert.DoesNotThrow(() => cmd.Execute());
            var foundTemplate = RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", $"{cohortName} Template");
            Assert.That(foundTemplate.Length, Is.EqualTo(1));
            var associations = RepositoryLocator.DataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>().Where(a => a.CohortIdentificationConfiguration_ID == foundTemplate[0].ID).ToList();
            Assert.That(associations.Count, Is.EqualTo(1));
            Assert.That(associations[0].Project.Name, Is.EqualTo(project.Name));
        }

        [Test]
        public void CreateCICFromTemplate_Test()
        {
            var cohortName = RandomString(10);
            var cic = CreateBaseCIC(cohortName);
            var ciccmd = new ExecuteCommandCreateCohortIdentificationConfigurationTemplate(new ThrowImmediatelyActivator(RepositoryLocator), cic);
            Assert.DoesNotThrow(() => ciccmd.Execute());
            var template = RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", $"{cohortName} Template").First();

            var cmd = new ExecuteCommandUseTemplateCohortIdentificationConfiguration(new ThrowImmediatelyActivator(RepositoryLocator), template);
            Assert.DoesNotThrow(() => cmd.Execute());
            var cics = RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", $"{cohortName}");
            Assert.That(cics.Length, Is.EqualTo(2));
        }

        [Test]
        public void CreateCICFromTemplateWithProject_Test()
        {
            var cohortName = RandomString(10);
            var cic = CreateBaseCIC(cohortName);
            var ciccmd = new ExecuteCommandCreateCohortIdentificationConfigurationTemplate(new ThrowImmediatelyActivator(RepositoryLocator), cic);
            Assert.DoesNotThrow(() => ciccmd.Execute());
            var template = RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", $"{cohortName} Template").First();
            var project = new Project(DataExportRepository, RandomString(10));
            project.SaveToDatabase();
            var cmd = new ExecuteCommandUseTemplateCohortIdentificationConfiguration(new ThrowImmediatelyActivator(RepositoryLocator), template);
            cmd.SetTarget(project);
            Assert.DoesNotThrow(() => cmd.Execute());
            var cics = RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<CohortIdentificationConfiguration>("Name", $"{cohortName}");
            Assert.That(cics.Length, Is.EqualTo(2));
            var associations = RepositoryLocator.DataExportRepository.GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>().Where(a => cics.Select(c => c.ID).Contains(a.CohortIdentificationConfiguration_ID)).ToList();
            Assert.That(associations.Count, Is.EqualTo(1));
            Assert.That(associations[0].Project.Name, Is.EqualTo(project.Name));
        }
    }
}

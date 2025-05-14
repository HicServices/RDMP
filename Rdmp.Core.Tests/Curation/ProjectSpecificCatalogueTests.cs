using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Checks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Common;

namespace Rdmp.Core.Tests.Curation
{
    class ProjectSpecificCatalogueTests : DatabaseTests
    {
        private Catalogue _catalogue;
        private Project _project1;
        private Project _project2;
        private ConsoleInputManager _activator;

        public void SetupTests(int projectCount = 0, int projectSpecificCount = 0, int extractionCount = 0)
        {
            _activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);

            _catalogue = new Catalogue(CatalogueRepository, "Dataset1");
            _catalogue.SaveToDatabase();
            var _t1 = new TableInfo(CatalogueRepository, "T1");
            _t1.SaveToDatabase();
            var _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c1.SaveToDatabase();
            var _ci1 = new CatalogueItem(CatalogueRepository, _catalogue, "PrivateIdentifierA");
            _ci1.SaveToDatabase();
            var _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Core,
                IsExtractionIdentifier = true
            };
            _extractionInfo1.SaveToDatabase();
            var eds = new ExtractableDataSet(_activator.RepositoryLocator.DataExportRepository, _catalogue);
            eds.SaveToDatabase();
            if (projectCount > 0)
            {
                _project1 = new Project(DataExportRepository, "Project1");
                _project1.SaveToDatabase();
            }
            if (projectCount > 1)
            {
                _project2 = new Project(DataExportRepository, "Project1");
                _project2.SaveToDatabase();
            }
            if (projectSpecificCount > 0)
            {
                var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator);
                cmd.SetTarget(_project1);
                cmd.SetTarget(_catalogue);
                Assert.DoesNotThrow(() => cmd.Execute());
            }
            if (projectSpecificCount > 1)
            {
                var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator);
                cmd.SetTarget(_project2);
                cmd.SetTarget(_catalogue);
                Assert.DoesNotThrow(() => cmd.Execute());
            }
            if (extractionCount > 0)
            {
                var ec = new ExtractionConfiguration(DataExportRepository, _project1, "ec1");
                Assert.DoesNotThrow(() => ec.AddDatasetToConfiguration(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First()));
                ec.SaveToDatabase();
            }
            if (extractionCount > 1)
            {
                var ec = new ExtractionConfiguration(DataExportRepository, _project2, "ec2");
                Assert.DoesNotThrow(() => ec.AddDatasetToConfiguration(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First()));
                ec.SaveToDatabase();
            }

        }

        private void MakeProjectSpecific(Catalogue catalogue, Project project, List<int> projectIdsToIgnore = null, bool shouldThrow = false)
        {
            var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator);
            cmd.projectIdsToIgnore = projectIdsToIgnore ?? [];
            cmd.SetTarget(project);
            cmd.SetTarget(catalogue);
            if (shouldThrow) Assert.Throws<ImpossibleCommandException>(() => cmd.Execute());
            else Assert.DoesNotThrow(() => cmd.Execute());
        }

        private void MakeNotProjectSpecific(Catalogue catalogue, Project project, bool shouldThrow = false)
        {
            var cmd = new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator,catalogue,project);
            if (shouldThrow) Assert.Throws<ImpossibleCommandException>(() => cmd.Execute());
            else Assert.DoesNotThrow(() => cmd.Execute());
        }

        [Test]
        public void MakeCatalogueProjectSpecificTest()
        {
            SetupTests(1, 0, 0);
            MakeProjectSpecific(_catalogue, _project1);
        }

        [Test]
        public void MakeCatalogueNotProjectSpecificTest()
        {
            SetupTests(1,1,0);
            MakeNotProjectSpecific(_catalogue, _project1);
        }


        [Test]
        public void MakeCatalogueProjectSpecificWhenInExtractionTest()
        {
            SetupTests(1, 0,1);
            MakeProjectSpecific(_catalogue, _project1);
        }

        [Test]
        public void MakeCatalogueNotProjectSpecificWhenInExtractionTest()
        {
            SetupTests(1, 1, 1);
            MakeNotProjectSpecific(_catalogue, _project1);
        }

        [Test]
        public void MakeCatalogueProjectSpecificWithTwoProjectsTest()
        {
            SetupTests(2, 0, 0);
            MakeProjectSpecific(_catalogue, _project1, new List<int>() { _project2.ID });
            MakeProjectSpecific(_catalogue, _project2, new List<int>() { _project1.ID });
        }

        [Test]
        public void MakeCatalogueNotProjectSpecificWithTwoProjectsTest()
        {
            SetupTests(2, 2, 0);
            MakeNotProjectSpecific(_catalogue, _project1);
            MakeNotProjectSpecific(_catalogue, _project2);
        }

        [Test]
        public void MakeSingleCatalogueNotProjectSpecificWithTwoProjectsTest()
        {
            SetupTests(2, 2, 0);
            MakeNotProjectSpecific(_catalogue, _project1);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID",_catalogue.ID).Count, Is.EqualTo(1));
        }

        [Test]
        public void MakeSingleCatalogueNotProjectSpecificWithTwoProjectsAndOneExtractionInOtherProjectTest()
        {
            SetupTests(2, 2, 1);
            MakeNotProjectSpecific(_catalogue, _project2);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
        }



        [Test]
        public void MakeSingleCatalogueNotProjectSpecificWithTwoProjectsAndOneExtractionTest()
        {
            SetupTests(2, 2, 1);
            MakeNotProjectSpecific(_catalogue, _project1,true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(2));
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInOtherProjectExtractionTest()
        {
            SetupTests(2, 0, 1);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Project_ID, Is.EqualTo(null));
            MakeProjectSpecific(_catalogue, _project2,null,true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.False);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Project_ID, Is.EqualTo(null));
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInOtherProjectAllowExtractionTest()
        {
            SetupTests(2, 0, 1);
            MakeProjectSpecific(_catalogue, _project2, new List<int>() { _project1.ID });
            MakeProjectSpecific(_catalogue, _project1, new List<int>() { _project1.ID });
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(2));
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInExtractionsTest()
        {
            SetupTests(2, 0, 2);
            MakeProjectSpecific(_catalogue, _project2, null,true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.False);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Project_ID, Is.EqualTo(null));
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInAnExtractionInEachProjectTest()
        {
            SetupTests(2, 0, 2);
            MakeProjectSpecific(_catalogue, _project2, new List<int>() { _project1.ID });
            MakeProjectSpecific(_catalogue, _project1, new List<int>() { _project1.ID });
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(2));
        }
    }
}

using Microsoft.Data.SqlClient;
using NPOI.OpenXmlFormats.Spreadsheet;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
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
        private ExtractableDataSet _eds1;
        private ConsoleInputManager _activator;

        public void SetupTests(int projectCount = 0, int projectSpecificCount = 0, int extractionCount = 0)
        {
            _activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet);

            _catalogue = new Catalogue(CatalogueRepository, "Dataset1");
            _catalogue.SaveToDatabase();
            var _t1 = new TableInfo(CatalogueRepository, "T1");
            _t1.SaveToDatabase();
            _activator.Publish(_t1);
            var _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c1.SaveToDatabase();
            _activator.Publish(_c1);
            var _ci1 = new CatalogueItem(CatalogueRepository, _catalogue, "PrivateIdentifierA");
            _ci1.SaveToDatabase();
            _activator.Publish(_ci1);
            var _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Core,
                IsExtractionIdentifier = true
            };
            _extractionInfo1.SaveToDatabase();
            _activator.Publish(_extractionInfo1);
            if (projectCount > 0)
            {
                _project1 = new Project(DataExportRepository, "Project1");
                _project1.SaveToDatabase();
                _activator.Publish(_project1);
                
            }
            if (projectCount > 1)
            {
                _project2 = new Project(DataExportRepository, "Project1");
                _project2.SaveToDatabase();
                _activator.Publish(_project2);

            }
            _eds1 = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).FirstOrDefault() ?? new ExtractableDataSet(_activator.RepositoryLocator.DataExportRepository, _catalogue);
            _eds1.SaveToDatabase();
            _activator.Publish(_eds1);
            if (projectSpecificCount > 0)
            {
                var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator);
                cmd.SetTarget(_project1);
                cmd.SetTarget(_catalogue);
                Assert.DoesNotThrow(() => cmd.Execute());
                _eds1 = _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).FirstOrDefault();
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

        private void MakeProjectSpecific(Catalogue catalogue, Project project, List<int> projectIdsToIgnore = null, bool shouldThrow = false,bool force=false)
        {
            var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(_activator,catalogue,project,force);
            cmd.SetTarget(project);
            cmd.SetTarget(catalogue);
            if (shouldThrow) Assert.That(ProjectSpecificCatalogueManager.CanMakeCatalogueProjectSpecific(_activator.RepositoryLocator.DataExportRepository, catalogue,project,projectIdsToIgnore??new List<int>()), Is.False);
            else Assert.DoesNotThrow(() => cmd.Execute());
        }

        private void MakeNotProjectSpecific(Catalogue catalogue, ExtractableDataSet eds, Project project, bool shouldThrow = false)
        {
            var cmd = new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(_activator, catalogue, eds);
            if (shouldThrow) Assert.That(ProjectSpecificCatalogueManager.CanMakeCatalogueNonProjectSpecific(_activator.RepositoryLocator.DataExportRepository, catalogue, eds,project), Is.False);
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
            SetupTests(1, 1, 0);
            MakeNotProjectSpecific(_catalogue, _eds1,_project1);
        }


        [Test]
        public void MakeCatalogueProjectSpecificWhenInExtractionTest()
        {
            SetupTests(1, 0, 1);
            MakeProjectSpecific(_catalogue, _project1);
        }

        [Test]
        public void MakeCatalogueNotProjectSpecificWhenInExtractionTest()
        {
            SetupTests(1, 1, 1);
            MakeNotProjectSpecific(_catalogue, _eds1, _project1);
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
            MakeNotProjectSpecific(_catalogue, _eds1, _project1);
            MakeNotProjectSpecific(_catalogue, _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(_catalogue).FirstOrDefault(eds => eds.Projects.Select(p => p.ID).Contains(_project2.ID)),_project2);
        }

        [Test]
        public void MakeSingleCatalogueNotProjectSpecificWithTwoProjectsTest()
        {
            SetupTests(2, 2, 0);
            MakeNotProjectSpecific(_catalogue, _eds1, _project1);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Where(eds => eds.Projects.Any()).Count, Is.EqualTo(1));
        }

        [Test]
        public void MakeSingleCatalogueNotProjectSpecificWithTwoProjectsAndOneExtractionInOtherProjectTest()
        {
            SetupTests(2, 2, 1);
            MakeNotProjectSpecific(_catalogue, _activator.RepositoryLocator.DataExportRepository.GetAllObjectsWithParent<ExtractableDataSet>(_catalogue).FirstOrDefault(eds => eds.Projects.Contains(_project2)),_project1);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Where(eds => eds.Projects.Any()).Count, Is.EqualTo(1));
        }



        [Test]
        public void MakeSingleCatalogueNotProjectSpecificWithTwoProjectsAndOneExtractionTest()
        {
            SetupTests(2, 2, 1);
            MakeNotProjectSpecific(_catalogue, _eds1, _project1,true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Projects.Count, Is.EqualTo(2));
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInOtherProjectExtractionTest()
        {
            SetupTests(2, 0, 1);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Projects.Any(), Is.False);
            MakeProjectSpecific(_catalogue, _project2, null, true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.False);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Projects.Any(), Is.False);
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInOtherProjectAllowExtractionTest()
        {
            SetupTests(2, 0, 1);
            MakeProjectSpecific(_catalogue, _project1, new List<int>() { _project2.ID });
            MakeProjectSpecific(_catalogue, _project2, new List<int>() { _project1.ID });
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Projects.Count, Is.EqualTo(2));
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInExtractionsTest()
        {
            SetupTests(2, 0, 2);
            MakeProjectSpecific(_catalogue, _project2, null, true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.False);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).Count, Is.EqualTo(1));
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Projects.Any(), Is.False);
        }

        [Test]
        public void MakeSingleCatalogueProjectSpecificWhenAlreadyInAnExtractionInEachProjectTest()
        {
            SetupTests(2, 0, 2);
            MakeProjectSpecific(_catalogue, _project2, new List<int>() { _project1.ID },false,true);
            MakeProjectSpecific(_catalogue, _project1, new List<int>() { _project1.ID },false,true);
            Assert.That(_activator.RepositoryLocator.CatalogueRepository.GetObjectByID<Catalogue>(_catalogue.ID).IsProjectSpecific(_activator.RepositoryLocator.DataExportRepository), Is.True);
            Assert.That(_activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", _catalogue.ID).First().Projects.Count, Is.EqualTo(2));
        }
    }
}
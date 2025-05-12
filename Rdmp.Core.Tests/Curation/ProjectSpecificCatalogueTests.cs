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
    class ProjectSpecificCatalogueTests: DatabaseTests
    {

        [Test]
        public void MakeCatalogueProjectSpecific()
        {

            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };
            var catalogue = new Catalogue(CatalogueRepository, "Dataset1");
            catalogue.SaveToDatabase();
            var _t1 = new TableInfo(CatalogueRepository, "T1");
            _t1.SaveToDatabase();
            var _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c1.SaveToDatabase();
            var _ci1 = new CatalogueItem(CatalogueRepository, catalogue, "PrivateIdentifierA");
            _ci1.SaveToDatabase();
            var _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Core,
                IsExtractionIdentifier = true
            };
            _extractionInfo1.SaveToDatabase();
            var eds = new ExtractableDataSet(activator.RepositoryLocator.DataExportRepository, catalogue);
            eds.SaveToDatabase();
            var project = new Project(DataExportRepository, "Project1");
            var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.SetTarget(project);
            cmd.SetTarget(catalogue);
            Assert.DoesNotThrow(() => cmd.Execute());

        }
        [Test]
        public void MakeCatalogueMultipleProjectSpecific()
        {

            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };
            var catalogue = new Catalogue(CatalogueRepository, "Dataset1");
            catalogue.SaveToDatabase();
            var _t1 = new TableInfo(CatalogueRepository, "T1");
            _t1.SaveToDatabase();
            var _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c1.SaveToDatabase();
            var _ci1 = new CatalogueItem(CatalogueRepository, catalogue, "PrivateIdentifierA");
            _ci1.SaveToDatabase();
            var _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Core,
                IsExtractionIdentifier = true
            };
            _extractionInfo1.SaveToDatabase();
            var eds = new ExtractableDataSet(activator.RepositoryLocator.DataExportRepository, catalogue);
            eds.SaveToDatabase();
            var project = new Project(DataExportRepository, "Project1");
            var project2 = new Project(DataExportRepository, "Project2");
            var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.SetTarget(project);
            cmd.SetTarget(catalogue);
            Assert.DoesNotThrow(() => cmd.Execute());
            cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.SetTarget(project2);
            cmd.SetTarget(catalogue);
            Assert.DoesNotThrow(() => cmd.Execute());

        }

        [Test]
        public void MakeCatalogueMultipleProjectSpecificAfterExtraction()
        {

            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };
            var catalogue = new Catalogue(CatalogueRepository, "Dataset1");
            catalogue.SaveToDatabase();
            var _t1 = new TableInfo(CatalogueRepository, "T1");
            _t1.SaveToDatabase();
            var _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c1.SaveToDatabase();
            var _ci1 = new CatalogueItem(CatalogueRepository, catalogue, "PrivateIdentifierA");
            _ci1.SaveToDatabase();
            var _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Core,
                IsExtractionIdentifier = true
            };
            _extractionInfo1.SaveToDatabase();
            var eds = new ExtractableDataSet(activator.RepositoryLocator.DataExportRepository, catalogue);
            eds.SaveToDatabase();
            var project = new Project(DataExportRepository, "Project1");
            var project2 = new Project(DataExportRepository, "Project2");

            var ec = new ExtractionConfiguration(DataExportRepository, project, "ec1");
            Assert.DoesNotThrow(() => ec.AddDatasetToConfiguration(activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Catalogue_ID", catalogue.ID).First()));
            ec.SaveToDatabase();

            var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.SetTarget(project2);
            cmd.SetTarget(catalogue);
            var ex = Assert.Throws<ImpossibleCommandException>(() => cmd.Execute());
            Assert.That(ex?.Message,  Is.EqualTo("Command is marked as IsImpossible and should not be Executed.  Reason is 'Cannot make Dataset1 Project Specific because it is already a part of ExtractionConfiguration ec1 (Project=Project1) and possibly others'"));
            cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.projectIdsToIgnore.Add(project.ID);
            cmd.SetTarget(project2);
            cmd.SetTarget(catalogue);
            Assert.DoesNotThrow(() => cmd.Execute());
        }

        //2 project specific w/ext, try ot remove
        [Test]
        public void RemoveProjectSpecificWhenUsedInMultipleProjectSpecificCatalogues()
        {
            var activator = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
            { DisallowInput = true };
            var catalogue = new Catalogue(CatalogueRepository, "Dataset1");
            catalogue.SaveToDatabase();
            var _t1 = new TableInfo(CatalogueRepository, "T1");
            _t1.SaveToDatabase();
            var _c1 = new ColumnInfo(CatalogueRepository, "PrivateIdentifierA", "varchar(10)", _t1);
            _c1.SaveToDatabase();
            var _ci1 = new CatalogueItem(CatalogueRepository, catalogue, "PrivateIdentifierA");
            _ci1.SaveToDatabase();
            var _extractionInfo1 = new ExtractionInformation(CatalogueRepository, _ci1, _c1, _c1.ToString())
            {
                Order = 123,
                ExtractionCategory = ExtractionCategory.Core,
                IsExtractionIdentifier = true
            };
            _extractionInfo1.SaveToDatabase();
            var eds = new ExtractableDataSet(activator.RepositoryLocator.DataExportRepository, catalogue);
            eds.SaveToDatabase();
            var project = new Project(DataExportRepository, "Project1");
            project.SaveToDatabase();
            var project2 = new Project(DataExportRepository, "Project2");
            project2.SaveToDatabase();
            var cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.SetTarget(project);
            cmd.SetTarget(catalogue);
            Assert.DoesNotThrow(() => cmd.Execute());
            cmd = new ExecuteCommandMakeCatalogueProjectSpecific(activator);
            cmd.SetTarget(project2);
            cmd.SetTarget(catalogue);
            Assert.DoesNotThrow(() => cmd.Execute());

            var ec = new ExtractionConfiguration(DataExportRepository, project, "ec1");
            Assert.DoesNotThrow(() => ec.AddDatasetToConfiguration(activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Project_ID", project.ID).First()));
            ec.SaveToDatabase();

            ec = new ExtractionConfiguration(DataExportRepository, project2, "ec2");
            Assert.DoesNotThrow(() => ec.AddDatasetToConfiguration(activator.RepositoryLocator.DataExportRepository.GetAllObjectsWhere<ExtractableDataSet>("Project_ID", project2.ID).First()));
            ec.SaveToDatabase();
            var returnCmd = new ExecuteCommandMakeProjectSpecificCatalogueNormalAgain(activator, catalogue, project);
            var ex = Assert.Throws<ImpossibleCommandException>(() => returnCmd.Execute());
            Assert.That(ex?.Message, Is.EqualTo(""));
        }

        //try to make catalogue project specific to the same project twice
    }
}

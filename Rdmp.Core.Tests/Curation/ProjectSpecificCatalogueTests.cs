using NUnit.Framework;
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
    }
}

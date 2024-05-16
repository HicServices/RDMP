// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands.CatalogueCreationCommands;
using Rdmp.Core.CommandLine.DatabaseCreation;
using Rdmp.Core.Curation.Data.Pipelines;
using System.IO;
using System.Linq;
using NUnit.Framework.Legacy;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class ExcelDatabaseTests : DatabaseTests
{
    [Test]
    public void TestLoadingFileWithTrailingDotsInHeader()
    {
        var trailingDotsFile = Path.Combine(TestContext.CurrentContext.TestDirectory, "TrailingDots....xlsx");
        FileAssert.Exists(trailingDotsFile);

        var db = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        // Create the 'out of the box' RDMP pipelines (which includes an excel bulk importer pipeline)
        var creator = new CataloguePipelinesAndReferencesCreation(
            RepositoryLocator, UnitTestLoggingConnectionString, DataQualityEngineConnectionString);
        creator.CreatePipelines(new PlatformDatabaseCreationOptions());

        // find the excel loading pipeline
        var pipe = CatalogueRepository.GetAllObjects<Pipeline>().OrderByDescending(p => p.ID)
            .FirstOrDefault(p => p.Name.Contains("BULK INSERT: Excel File"));

        // run an import of the file using the pipeline
        var cmd = new ExecuteCommandCreateNewCatalogueByImportingFile(
            new ThrowImmediatelyActivator(RepositoryLocator),
            new FileInfo(trailingDotsFile),
            null, db, pipe, null);

        cmd.Execute();

        var tbl = db.ExpectTable("TrailingDots");
        Assert.That(tbl.Exists());

        var cols = tbl.DiscoverColumns();
        Assert.That(cols, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(cols[0].GetRuntimeName(), Is.EqualTo("Field1"));
            Assert.That(cols[1].GetRuntimeName(), Is.EqualTo("Field2"));

            Assert.That(tbl.GetRowCount(), Is.EqualTo(2));
        });
    }
}
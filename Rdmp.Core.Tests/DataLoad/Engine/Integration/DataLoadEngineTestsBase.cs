// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Modules.Attachers;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

/// <summary>
/// Base class for tests that want to run data loads contains helper methods for setting up a valid DLE load configuration and running it
/// </summary>
internal class DataLoadEngineTestsBase : DatabaseTests
{
    protected static void AssertHasDataLoadRunId(DataRow row)
    {
        var o = row[SpecialFieldNames.DataLoadRunID];

        Assert.That(o, Is.Not.Null, "A row which was expected to have a hic_dataLoadRunID had null instead");
        Assert.Multiple(() =>
        {
            Assert.That(o, Is.Not.EqualTo(DBNull.Value),
                    "A row which was expected to have a hic_dataLoadRunID had DBNull.Value instead");
            Assert.That((int)o, Is.GreaterThanOrEqualTo(0));
        });

        var d = row[SpecialFieldNames.ValidFrom];
        Assert.That(d, Is.Not.Null, "A row which was expected to have a hic_validFrom had null instead");
        Assert.Multiple(() =>
        {
            Assert.That(d, Is.Not.EqualTo(DBNull.Value),
                    "A row which was expected to have a hic_validFrom had DBNull.Value instead");

            //expect validFrom to be after 2 hours ago (to handle UTC / BST nonsense)
            Assert.That((DateTime)d, Is.GreaterThanOrEqualTo(DateTime.Now.Subtract(new TimeSpan(2, 0, 0))));
        });
    }

    protected void CreateCSVProcessTask(LoadMetadata lmd, ITableInfo ti, string regex)
    {
        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.Mounting)
        {
            Path = typeof(AnySeparatorFileAttacher).FullName,
            ProcessTaskType = ProcessTaskType.Attacher,
            Name = $"Load {ti.GetRuntimeName()}"
        };
        pt.SaveToDatabase();

        pt.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();
        pt.SetArgumentValue("FilePattern", regex);
        pt.SetArgumentValue("Separator", ",");
        pt.SetArgumentValue("TableToLoad", ti);

        pt.Check(ThrowImmediatelyCheckNotifier.Quiet);
    }

    protected static LoadDirectory SetupLoadDirectory(LoadMetadata lmd)
    {
        var projectDirectory =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                "MyLoadDir", true);
        lmd.LocationOfForLoadingDirectory = Path.Combine(projectDirectory.RootPath.FullName, lmd.DefaultForLoadingPath);
        lmd.LocationOfForArchivingDirectory = Path.Combine(projectDirectory.RootPath.FullName, lmd.DefaultForArchivingPath);
        lmd.LocationOfExecutablesDirectory = Path.Combine(projectDirectory.RootPath.FullName, lmd.DefaultExecutablesPath);
        lmd.LocationOfCacheDirectory = Path.Combine(projectDirectory.RootPath.FullName, lmd.DefaultCachePath);
        lmd.SaveToDatabase();

        return projectDirectory;
    }

    protected ITableInfo Import(DiscoveredTable tbl, LoadMetadata lmd, LogManager logManager)
    {
        logManager.CreateNewLoggingTaskIfNotExists(lmd.Name);

        //import TableInfos
        var importer = new TableInfoImporter(CatalogueRepository, tbl);
        importer.DoImport(out var ti, out var cis);

        //create Catalogue
        var forwardEngineer = new ForwardEngineerCatalogue(ti, cis);
        forwardEngineer.ExecuteForwardEngineering(out var cata, out var cataItems, out var eis);

        //make the catalogue use the load configuration
        cata.LoggingDataTask = lmd.Name;
        Assert.That(cata.LiveLoggingServer_ID, Is.Not.Null); //catalogue should have one of these because of system defaults
        cata.SaveToDatabase();
        lmd.LinkToCatalogue(cata);
        return ti;
    }
}
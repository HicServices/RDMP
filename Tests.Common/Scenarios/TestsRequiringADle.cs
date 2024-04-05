// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BadMedicine;
using BadMedicine.Datasets;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.Attachers;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Tests.Common.Scenarios;

/// <summary>
/// Scenario where you want to run a full DLE load of records into a table
/// </summary>
public class TestsRequiringADle : TestsRequiringA
{
    protected int RowsBefore;
    protected int RowsNow => LiveTable.GetRowCount();

    protected LoadMetadata TestLoadMetadata;
    protected ICatalogue TestCatalogue;
    protected LoadDirectory LoadDirectory;

    public DiscoveredTable LiveTable { get; private set; }
    public DiscoveredDatabase Database { get; private set; }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        Database = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var rootFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        var subdir = rootFolder.CreateSubdirectory("TestsRequiringADle");
        LoadDirectory = LoadDirectory.CreateDirectoryStructure(rootFolder, subdir.FullName, true);

        Clear(LoadDirectory);

        LiveTable = CreateDataset<Demography>(Database, 500, 5000, new Random(190));
        LiveTable.CreatePrimaryKey(new DiscoveredColumn[]
        {
            LiveTable.DiscoverColumn("chi"),
            LiveTable.DiscoverColumn("dtCreated"),
            LiveTable.DiscoverColumn("hb_extract")
        });

        TestCatalogue = Import(LiveTable);
        RowsBefore = 5000;

        TestLoadMetadata = new LoadMetadata(CatalogueRepository, "Loading Test Catalogue");
        TestLoadMetadata.LocationOfForLoadingDirectory = Path.Combine(LoadDirectory.RootPath.FullName, TestLoadMetadata.DefaultForLoadingPath);
        TestLoadMetadata.LocationOfForArchivingDirectory = Path.Combine(LoadDirectory.RootPath.FullName, TestLoadMetadata.DefaultForArchivingPath);
        TestLoadMetadata.LocationOfExecutablesDirectory = Path.Combine(LoadDirectory.RootPath.FullName, TestLoadMetadata.DefaultExecutablesPath);
        TestLoadMetadata.LocationOfCacheDirectory = Path.Combine(LoadDirectory.RootPath.FullName, TestLoadMetadata.DefaultCachePath);

        TestLoadMetadata.SaveToDatabase();


        //make the load load the table
        TestCatalogue.SaveToDatabase();
        TestLoadMetadata.LinkToCatalogue(TestCatalogue);
        CreateFlatFileAttacher(TestLoadMetadata, "*.csv", TestCatalogue.GetTableInfoList(false).Single(), ",");

        //Get DleRunner to run pre load checks (includes trigger creation etc)
        var runner = new DleRunner(new DleOptions
        { LoadMetadata = TestLoadMetadata.ID.ToString(), Command = CommandLineActivity.check });
        runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
            new GracefulCancellationToken());
    }

    /// <summary>
    /// Creates a <see cref="AnySeparatorFileAttacher"/> parcelled into a <see cref="ProcessTask"/> that reads CSV files in ForLoading
    /// in the mounting stage of the load
    /// </summary>
    /// <param name="lmd">The load to create the <see cref="ProcessTask"/> in</param>
    /// <param name="pattern">File pattern to load e.g. *.csv</param>
    /// <param name="ti">The table to load (must be part of the <paramref name="lmd"/></param>
    /// <param name="separator">The separator of the files e.g. ','</param>
    /// <param name="ignoreColumns">Columns to ignore in the load</param>
    /// <returns></returns>
    protected ProcessTask CreateFlatFileAttacher(LoadMetadata lmd, string pattern, ITableInfo ti,
        string separator = ",", string ignoreColumns = "hic_dataLoadRunID")
    {
        var csvProcessTask = new ProcessTask(CatalogueRepository, lmd, LoadStage.Mounting);
        var args = csvProcessTask.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();
        csvProcessTask.Path = typeof(AnySeparatorFileAttacher).FullName;
        csvProcessTask.ProcessTaskType = ProcessTaskType.Attacher;
        csvProcessTask.SaveToDatabase();

        var filePattern = args.Single(a => a.Name == "FilePattern");
        filePattern.SetValue(pattern);
        filePattern.SaveToDatabase();

        var tableToLoad = args.Single(a => a.Name == "TableToLoad");
        tableToLoad.SetValue(ti);
        tableToLoad.SaveToDatabase();

        var separatorArg = args.Single(a => a.Name == "Separator");
        separatorArg.SetValue(separator);
        separatorArg.SaveToDatabase();

        var ignoreDataLoadRunIDCol = args.Single(a => a.Name == "IgnoreColumns");
        ignoreDataLoadRunIDCol.SetValue(ignoreColumns);
        ignoreDataLoadRunIDCol.SaveToDatabase();

        return csvProcessTask;
    }


    /// <summary>
    /// Creates a new demography file ready for loading in the ForLoading directory of the load with the specified number of <paramref name="rows"/>
    /// </summary>
    /// <param name="filename">Filename to generate in ForLoading e.g. "bob.csv" (cannot be relative)</param>
    /// <param name="rows"></param>
    /// <param name="r">Seed random to ensure tests are reproducible</param>
    protected FileInfo CreateFileInForLoading(string filename, int rows, Random r)
    {
        var fi = new FileInfo(Path.Combine(LoadDirectory.ForLoading.FullName, Path.GetFileName(filename)));

        var demog = new Demography(r);
        var people = new PersonCollection();
        people.GeneratePeople(500, r);

        demog.GenerateTestDataFile(people, fi, rows);

        return fi;
    }

    /// <summary>
    /// Creates a new file in the ForLoading directory of the <see cref="LoadDirectory"/> with the specified <paramref name="contents"/>
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="contents"></param>
    /// <returns></returns>
    protected FileInfo CreateFileInForLoading(string filename, string[] contents)
    {
        var fi = new FileInfo(Path.Combine(LoadDirectory.ForLoading.FullName, Path.GetFileName(filename)));
        File.WriteAllLines(fi.FullName, contents);
        return fi;
    }

    public void RunDLE(int timeoutInMilliseconds)
    {
        RunDLE(TestLoadMetadata, timeoutInMilliseconds, false);
    }

    /// <summary>
    /// Runs the data load engine for the given <paramref name="lmd"/>
    /// </summary>
    /// <param name="lmd"></param>
    /// <param name="timeoutInMilliseconds"></param>
    /// <param name="checks">True to run the pre load checks with accept all proposed fixes</param>
    public void RunDLE(LoadMetadata lmd, int timeoutInMilliseconds, bool checks)
    {
        var timeout = new CancellationTokenSource(timeoutInMilliseconds).Token;
        if (checks)
        {
            //Get DleRunner to run pre load checks (includes trigger creation etc)
            var checker = new DleRunner(new DleOptions
            { LoadMetadata = lmd.ID.ToString(), Command = CommandLineActivity.check });
            checker.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
                new GracefulCancellationToken(timeout, timeout));
        }

        var runner = new DleRunner(new DleOptions
        { LoadMetadata = lmd.ID.ToString(), Command = CommandLineActivity.run });
        runner.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, ThrowImmediatelyCheckNotifier.Quiet,
            new GracefulCancellationToken(timeout, timeout));
    }
}
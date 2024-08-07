// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;
using Rdmp.Core.Repositories;
using Tests.Common.Helpers;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.CommandLine.AutomationLoopTests;

public class EndToEndDLECacheTest : TestsRequiringADle
{
    [Test]
    public void RunEndToEndDLECacheTest()
    {
        MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
        MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

        const int timeoutInMilliseconds = 120000;

        var lmd = TestLoadMetadata;

        var lp = new LoadProgress(CatalogueRepository, lmd)
        {
            DataLoadProgress = new DateTime(2001, 1, 1),
            DefaultNumberOfDaysToLoadEachTime = 10
        };
        lp.SaveToDatabase();

        var cp = new CacheProgress(CatalogueRepository, lp)
        {
            CacheFillProgress = new DateTime(2001, 1, 11) //10 days available to load
        };
        cp.SaveToDatabase();

        var assembler = new TestDataPipelineAssembler("RunEndToEndDLECacheTest pipe", CatalogueRepository);
        assembler.ConfigureCacheProgressToUseThePipeline(cp);

        //setup the cache process task
        var pt = new ProcessTask(CatalogueRepository, lmd, LoadStage.GetFiles)
        {
            Path = typeof(BasicCacheDataProvider).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider
        };
        pt.SaveToDatabase();
        pt.CreateArgumentsForClassIfNotExists<BasicCacheDataProvider>();

        var attacher = lmd.ProcessTasks.Single(p => p.ProcessTaskType == ProcessTaskType.Attacher);
        var patternArgument = (ProcessTaskArgument)attacher.GetAllArguments().Single(a => a.Name.Equals("FilePattern"));
        patternArgument.SetValue("*.csv");
        patternArgument.SaveToDatabase();

        //take the forLoading file
        var csvFile = CreateFileInForLoading("bob.csv", 10, new Random(5000));

        //and move it to the cache and give it a date in the range we expect for the cached data
        csvFile.MoveTo(Path.Combine(LoadDirectory.Cache.FullName, "2001-01-09.csv"));

        RunDLE(timeoutInMilliseconds);

        Assert.Multiple(() =>
        {
            Assert.That(RowsNow - RowsBefore, Is.EqualTo(10));

            Assert.That(LoadDirectory.Cache.GetFiles(), Is.Empty);
            Assert.That(LoadDirectory.ForLoading.GetFiles(), Is.Empty);
            Assert.That(LoadDirectory.ForArchiving.GetFiles(), Has.Length.EqualTo(1));
        });

        var archiveFile = LoadDirectory.ForArchiving.GetFiles()[0];
        Assert.That(archiveFile.Extension, Is.EqualTo(".zip"));


        //load progress should be updated to the largest date in the cache (2001-01-09)
        lp.RevertToDatabaseState();
        Assert.That(new DateTime(2001, 01, 09), Is.EqualTo(lp.DataLoadProgress));

        cp.DeleteInDatabase();
        lp.DeleteInDatabase();

        assembler.Destroy();
    }
}
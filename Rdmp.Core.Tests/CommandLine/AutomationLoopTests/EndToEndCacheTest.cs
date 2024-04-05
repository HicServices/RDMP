// Copyright (c) The University of Dundee 2018-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Rdmp.Core.Caching;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Helpers;

namespace Rdmp.Core.Tests.CommandLine.AutomationLoopTests;

public class EndToEndCacheTest : DatabaseTests
{
    private Catalogue _cata;
    private LoadMetadata _lmd;
    private LoadProgress _lp;
    private CacheProgress _cp;

    private const int NumDaysToCache = 5;

    private TestDataPipelineAssembler _testPipeline;
    private LoadDirectory _LoadDirectory;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
        MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

        _lmd = new LoadMetadata(CatalogueRepository, "Ive got a lovely bunch o' coconuts");
        _LoadDirectory =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory),
                @"EndToEndCacheTest", true);
        _lmd.LocationOfForLoadingDirectory = _LoadDirectory.RootPath.FullName + _lmd.DefaultForLoadingPath;
        _lmd.LocationOfForArchivingDirectory = _LoadDirectory.RootPath.FullName + _lmd.DefaultForArchivingPath;
        _lmd.LocationOfExecutablesDirectory = _LoadDirectory.RootPath.FullName + _lmd.DefaultExecutablesPath;
        _lmd.LocationOfCacheDirectory = _LoadDirectory.RootPath.FullName + _lmd.DefaultCachePath;
        _lmd.SaveToDatabase();

        Clear(_LoadDirectory);

        _cata = new Catalogue(CatalogueRepository, "EndToEndCacheTest")
        {
        };
        _cata.SaveToDatabase();

        var linkage = new LoadMetadataCatalogueLinkage(CatalogueRepository, _lmd, _cata);
        linkage.SaveToDatabase();

        _lp = new LoadProgress(CatalogueRepository, _lmd);
        _cp = new CacheProgress(CatalogueRepository, _lp);

        _lp.OriginDate = new DateTime(2001, 1, 1);
        _lp.SaveToDatabase();

        _testPipeline =
            new TestDataPipelineAssembler($"EndToEndCacheTestPipeline{Guid.NewGuid()}", CatalogueRepository);
        _testPipeline.ConfigureCacheProgressToUseThePipeline(_cp);

        _cp.CacheFillProgress = DateTime.Now.AddDays(-NumDaysToCache);
        _cp.SaveToDatabase();

        _cp.SaveToDatabase();
    }


    [Test]
    public void FireItUpManually()
    {
        MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
        MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

        var cachingHost = new CachingHost(CatalogueRepository)
        {
            CacheProgress = _cp
        };

        cachingHost.Start(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken());

        // should be numDaysToCache days in cache
        Assert.That(_LoadDirectory.Cache.GetFiles("*.csv"), Has.Length.EqualTo(NumDaysToCache));

        // make sure each file is named as expected
        var cacheFiles = _LoadDirectory.Cache.GetFiles().Select(fi => fi.Name).ToArray();
        for (var i = -NumDaysToCache; i < 0; i++)
        {
            var filename = $"{DateTime.Now.AddDays(i):yyyyMMdd}.csv";
            Assert.That(cacheFiles, Does.Contain(filename), filename + " not found");
        }
    }

    [Test]
    public void RunEndToEndCacheTest()
    {
        var t = Task.Factory.StartNew(() =>
        {
            Assert.That(_LoadDirectory.Cache.GetFiles("*.csv"), Is.Empty);

            var auto = new CacheRunner(new CacheOptions
            { CacheProgress = _cp.ID.ToString(), Command = CommandLineActivity.run });
            auto.Run(RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet,
                ThrowImmediatelyCheckNotifier.Quiet, new GracefulCancellationToken());
        });

        Assert.That(t.Wait(60000));
    }
}
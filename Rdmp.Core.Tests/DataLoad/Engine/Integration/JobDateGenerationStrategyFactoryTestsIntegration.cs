// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Microsoft.Data.SqlClient;
using System.IO;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling.Exceptions;
using Rdmp.Core.DataLoad.Engine.LoadProcess.Scheduling.Strategy;
using Rdmp.Core.DataLoad.Modules.DataProvider;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;
using Tests.Common.Helpers;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class JobDateGenerationStrategyFactoryTestsIntegration : DatabaseTests
{
    private CacheProgress _cp;
    private LoadProgress _lp;
    private LoadMetadata _lmd;
    private DiscoveredServer _server;
    private JobDateGenerationStrategyFactory _factory;

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        MEF.AddTypeToCatalogForTesting(typeof(TestDataWriter));
        MEF.AddTypeToCatalogForTesting(typeof(TestDataInventor));

        _lmd = new LoadMetadata(CatalogueRepository, "JobDateGenerationStrategyFactoryTestsIntegration");
        _lp = new LoadProgress(CatalogueRepository, _lmd)
        {
            DataLoadProgress = new DateTime(2001, 1, 1)
        };

        _lp.SaveToDatabase();

        _cp = new CacheProgress(CatalogueRepository, _lp);


        _server = new DiscoveredServer(new SqlConnectionStringBuilder("server=localhost;initial catalog=fish"));
        _factory = new JobDateGenerationStrategyFactory(new SingleLoadProgressSelectionStrategy(_lp));
    }

    [Test]
    public void CacheProvider_None()
    {
        var ex = Assert.Throws<CacheDataProviderFindingException>(() =>
            _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet));
        Assert.That(ex.Message, Does.StartWith("LoadMetadata JobDateGenerationStrategyFactoryTestsIntegration does not have ANY process tasks of type ProcessTaskType.DataProvider"));
    }


    [Test]
    public void CacheProvider_NonCachingOne()
    {
        var pt = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles)
        {
            Path = typeof(DoNothingDataProvider).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider,
            Name = "DoNothing"
        };
        pt.SaveToDatabase();

        var ex = Assert.Throws<CacheDataProviderFindingException>(() =>
            _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet));
        Assert.That(ex.Message, Does.StartWith("LoadMetadata JobDateGenerationStrategyFactoryTestsIntegration has some DataProviders tasks but none of them wrap classes that implement ICachedDataProvider"));
    }


    [Test]
    public void CacheProvider_TwoCachingOnes()
    {
        var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles)
        {
            Path = typeof(TestCachedFileRetriever).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider,
            Name = "Cache1"
        };
        pt1.SaveToDatabase();

        var pt2 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles)
        {
            Path = typeof(TestCachedFileRetriever).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider,
            Name = "Cache2"
        };
        pt2.SaveToDatabase();

        var ex = Assert.Throws<CacheDataProviderFindingException>(() =>
            _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet));
        Assert.That(
            ex.Message, Is.EqualTo("LoadMetadata JobDateGenerationStrategyFactoryTestsIntegration has multiple cache DataProviders tasks (Cache1,Cache2), you are only allowed 1"));
    }

    [Test]
    public void CacheProvider_NoPipeline()
    {
        var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles)
        {
            Path = typeof(TestCachedFileRetriever).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider,
            Name = "Cache1"
        };
        pt1.SaveToDatabase();

        _cp.CacheFillProgress = new DateTime(1999, 1, 1);
        _cp.Name = "MyTestCp";
        _cp.SaveToDatabase();

        pt1.CreateArgumentsForClassIfNotExists<TestCachedFileRetriever>();

        var projDir =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "delme",
                true);
        _lmd.LocationOfForLoadingDirectory = projDir.RootPath.FullName + _lmd.DefaultForLoadingPath;
        _lmd.LocationOfForArchivingDirectory = projDir.RootPath.FullName + _lmd.DefaultForArchivingPath;
        _lmd.LocationOfExecutablesDirectory = projDir.RootPath.FullName + _lmd.DefaultExecutablesPath;
        _lmd.LocationOfCacheDirectory = projDir.RootPath.FullName + _lmd.DefaultCachePath;
        _lmd.SaveToDatabase();
        try
        {
            var ex = Assert.Throws<Exception>(() => _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet));
            Assert.That(ex.Message, Is.EqualTo("CacheProgress MyTestCp does not have a Pipeline configured on it"));
        }
        finally
        {
            projDir.RootPath.Delete(true);
        }
    }

    [Test]
    public void CacheProvider_NoCacheProgress()
    {
        var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles)
        {
            Path = typeof(BasicCacheDataProvider).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider,
            Name = "Cache1"
        };
        pt1.SaveToDatabase();

        var projDir =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "delme",
                true);
        _lmd.LocationOfForLoadingDirectory = projDir.RootPath.FullName + _lmd.DefaultForLoadingPath;
        _lmd.LocationOfForArchivingDirectory = projDir.RootPath.FullName + _lmd.DefaultForArchivingPath;
        _lmd.LocationOfExecutablesDirectory = projDir.RootPath.FullName + _lmd.DefaultExecutablesPath;
        _lmd.LocationOfCacheDirectory = projDir.RootPath.FullName + _lmd.DefaultCachePath; _lmd.SaveToDatabase();

        var pipeAssembler = new TestDataPipelineAssembler("CacheProvider_Normal", CatalogueRepository);
        pipeAssembler.ConfigureCacheProgressToUseThePipeline(_cp);

        try
        {
            var ex = Assert.Throws<InvalidOperationException>(() =>
                _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet));
            Assert.That(
                ex.Message, Is.EqualTo($"Caching has not begun for this CacheProgress ({_cp.ID}), so there is nothing to load and this strategy should not be used."));
        }
        finally
        {
            _cp.Pipeline_ID = null;
            pipeAssembler.Destroy();
            projDir.RootPath.Delete(true);
        }
    }

    [Test]
    public void CacheProvider_Normal()
    {
        var pt1 = new ProcessTask(CatalogueRepository, _lmd, LoadStage.GetFiles)
        {
            Path = typeof(BasicCacheDataProvider).FullName,
            ProcessTaskType = ProcessTaskType.DataProvider,
            Name = "Cache1"
        };
        pt1.SaveToDatabase();

        _cp.CacheFillProgress = new DateTime(2010, 1, 1);
        _cp.SaveToDatabase();

        var projDir =
            LoadDirectory.CreateDirectoryStructure(new DirectoryInfo(TestContext.CurrentContext.TestDirectory), "delme",
                true);
        _lmd.LocationOfForLoadingDirectory = projDir.RootPath.FullName + _lmd.DefaultForLoadingPath;
        _lmd.LocationOfForArchivingDirectory = projDir.RootPath.FullName + _lmd.DefaultForArchivingPath;
        _lmd.LocationOfExecutablesDirectory = projDir.RootPath.FullName + _lmd.DefaultExecutablesPath;
        _lmd.LocationOfCacheDirectory = projDir.RootPath.FullName + _lmd.DefaultCachePath;
        _lmd.SaveToDatabase();

        var pipeAssembler = new TestDataPipelineAssembler("CacheProvider_Normal", CatalogueRepository);
        pipeAssembler.ConfigureCacheProgressToUseThePipeline(_cp);

        try
        {
            var strategy = _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet);
            Assert.That(strategy.GetType(), Is.EqualTo(typeof(SingleScheduleCacheDateTrackingStrategy)));

            var dates = strategy.GetDates(10, false);
            Assert.That(dates, Is.Empty); //zero dates to load because no files in cache

            File.WriteAllText(Path.Combine(projDir.Cache.FullName, "2001-01-02.zip"),
                "bobbobbobyobyobyobbzzztproprietarybitztreamzippy");
            File.WriteAllText(Path.Combine(projDir.Cache.FullName, "2001-01-03.zip"),
                "bobbobbobyobyobyobbzzztproprietarybitztreamzippy");
            File.WriteAllText(Path.Combine(projDir.Cache.FullName, "2001-01-05.zip"),
                "bobbobbobyobyobyobbzzztproprietarybitztreamzippy");

            strategy = _factory.Create(_lp, ThrowImmediatelyDataLoadEventListener.Quiet);
            Assert.That(strategy.GetType(), Is.EqualTo(typeof(SingleScheduleCacheDateTrackingStrategy)));
            dates = strategy.GetDates(10, false);
            Assert.That(dates, Has.Count.EqualTo(3)); //zero dates to load because no files in cache
        }
        finally
        {
            _cp.Pipeline_ID = null;
            pipeAssembler.Destroy();
            projDir.RootPath.Delete(true);
        }
    }
}
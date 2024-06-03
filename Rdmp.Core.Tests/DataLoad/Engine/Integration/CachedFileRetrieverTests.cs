// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using FAnsi.Discovery;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad;
using Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.Logging;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class CachedFileRetrieverTests : DatabaseTests
{
    private readonly ILoadProgress _lpMock;

    public CachedFileRetrieverTests()
    {
        var cpMock = Substitute.For<ICacheProgress>();
        _lpMock = Substitute.For<ILoadProgress>();
        _lpMock.CacheProgress.Returns(cpMock);
    }

    [Test(Description =
        "RDMPDEV-185: Tests the scenario where the files in ForLoading do not match the files that are expected given the job specification. In this case the load process should not continue, otherwise the wrong data will be loaded.")]
    public void AttemptToLoadDataWithFilesInForLoading_DisagreementBetweenCacheAndForLoading()
    {
        var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempDir = Directory.CreateDirectory(tempDirPath);

        try
        {
            // Different file in ForLoading than exists in cache
            var loadDirectory = LoadDirectory.CreateDirectoryStructure(tempDir, "CachedFileRetriever");
            var cachedFilePath = Path.Combine(loadDirectory.Cache.FullName, "2016-01-02.zip");
            File.WriteAllText(cachedFilePath, "");
            File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "2016-01-01.zip"), "");

            // Set SetUp retriever
            var cacheLayout = new ZipCacheLayoutOnePerDay(loadDirectory.Cache, new NoSubdirectoriesCachePathResolver());

            var retriever = new TestCachedFileRetriever
            {
                ExtractFilesFromArchive = false,
                LoadProgress = _lpMock,
                Layout = cacheLayout
            };

            // Set SetUp job
            var job = CreateTestJob(loadDirectory);
            job.DatesToRetrieve = new List<DateTime>
            {
                new(2016, 01, 02)
            };

            // Should fail after determining that the files in ForLoading do not match the job specification
            var ex = Assert.Throws<InvalidOperationException>(() =>
                retriever.Fetch(job, new GracefulCancellationToken()));
            Assert.That(
                ex.Message, Does.StartWith("The files in ForLoading do not match what this job expects to be loading from the cache."),
                ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace);
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    [Test(Description =
        "RDMPDEV-185: Tests the scenario where the files in ForLoading match the files that are expected given the job specification, e.g. a load has after the cache has been populated and a subsequent load with *exactly the same parameters* has been triggered. In this case the load can proceed.")]
    public void AttemptToLoadDataWithFilesInForLoading_AgreementBetweenForLoadingAndCache()
    {
        var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempDir = Directory.CreateDirectory(tempDirPath);

        try
        {
            // File in cache is the same file as in ForLoading (20160101.zip)
            var loadDirectory = LoadDirectory.CreateDirectoryStructure(tempDir, "CachedFileRetriever");
            var cachedFilePath = Path.Combine(loadDirectory.Cache.FullName, "2016-01-01.zip");
            File.WriteAllText(cachedFilePath, "");
            File.WriteAllText(Path.Combine(loadDirectory.ForLoading.FullName, "2016-01-01.zip"), "");


            // Set SetUp retriever
            var cacheLayout = new ZipCacheLayoutOnePerDay(loadDirectory.Cache, new NoSubdirectoriesCachePathResolver());

            var retriever = new TestCachedFileRetriever
            {
                ExtractFilesFromArchive = false,
                LoadProgress = _lpMock,
                Layout = cacheLayout
            };

            // Set SetUp job
            var job = CreateTestJob(loadDirectory);
            job.DatesToRetrieve = new List<DateTime>
            {
                new(2016, 01, 01)
            };

            // Should complete successfully, the file in ForLoading matches the job specification
            retriever.Fetch(job, new GracefulCancellationToken());

            // And ForLoading should still have the file in it (i.e. it hasn't mysteriously disappeared)
            Assert.That(File.Exists(Path.Combine(loadDirectory.ForLoading.FullName, "2016-01-01.zip")));
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    [Test(Description = "RDMPDEV-185: Tests the default scenario where there are no files in ForLoading.")]
    public void AttemptToLoadDataWithoutFilesInForLoading()
    {
        var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var tempDir = Directory.CreateDirectory(tempDirPath);

        try
        {
            // File in cache only, no files in ForLoading
            var loadDirectory = LoadDirectory.CreateDirectoryStructure(tempDir, "CachedFileRetriever");
            var cachedFilePath = Path.Combine(loadDirectory.Cache.FullName, "2016-01-01.zip");
            File.WriteAllText(cachedFilePath, "");


            // Set SetUp retriever
            var cacheLayout = new ZipCacheLayoutOnePerDay(loadDirectory.Cache, new NoSubdirectoriesCachePathResolver());

            var retriever = new TestCachedFileRetriever
            {
                ExtractFilesFromArchive = false,
                LoadProgress = _lpMock,
                Layout = cacheLayout
            };

            // Set SetUp job
            var job = CreateTestJob(loadDirectory);
            job.DatesToRetrieve = new List<DateTime>
            {
                new(2016, 01, 01)
            };

            // Should complete successfully, there are no files in ForLoading to worry about
            retriever.Fetch(job, new GracefulCancellationToken());

            // And the retriever should have copied the cached archive file into ForLoading
            Assert.That(File.Exists(Path.Combine(loadDirectory.ForLoading.FullName, "2016-01-01.zip")));
        }
        finally
        {
            tempDir.Delete(true);
        }
    }

    private ScheduledDataLoadJob CreateTestJob(ILoadDirectory directory)
    {
        var catalogue = Substitute.For<ICatalogue>();
        catalogue.GetTableInfoList(false).Returns(Array.Empty<TableInfo>());
        catalogue.GetLookupTableInfoList().Returns(Array.Empty<TableInfo>());
        catalogue.LoggingDataTask.Returns("TestLogging");

        var logManager = Substitute.For<ILogManager>();
        var loadMetadata = Substitute.For<ILoadMetadata>();
        loadMetadata.GetAllCatalogues().Returns(new[] { catalogue });

        var j = new ScheduledDataLoadJob(RepositoryLocator, "Test job", logManager, loadMetadata, directory,
            ThrowImmediatelyDataLoadEventListener.Quiet, null)
        {
            LoadProgress = _lpMock
        };
        return j;
    }
}

internal class TestCachedFileRetriever : CachedFileRetriever
{
    public ICacheLayout Layout;

    public override void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public override ExitCodeType Fetch(IDataLoadJob dataLoadJob, GracefulCancellationToken cancellationToken)
    {
        var scheduledJob = ConvertToScheduledJob(dataLoadJob);
        GetDataLoadWorkload(scheduledJob);
        ExtractJobs(scheduledJob);

        return ExitCodeType.Success;
    }

    protected override ICacheLayout CreateCacheLayout(ICacheProgress cacheProgress, IDataLoadEventListener listener) =>
        Layout;
}
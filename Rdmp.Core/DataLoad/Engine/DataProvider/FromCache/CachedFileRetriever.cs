// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;

/// <summary>
///     Fetches all the ILoadProgresss in the ILoadMetadata, it then selects the first scheduled task which has work to be
///     done (e.g. data is cached but not yet loaded).
///     Cached data is unzipped to the forLoading directory.  The Dispose method (which should be called after the entire
///     DataLoad has completed successfully) will clear
///     out the cached file(s) that were loaded and update the schedule to indicate the successful loading of data
/// </summary>
public abstract class CachedFileRetriever : ICachedDataProvider
{
    [DemandsInitialization(
        "The LoadProgress (which must also have a CacheProgress with a valid Caching Pipeline associated with it)",
        mandatory: true)]
    public ILoadProgress LoadProgress { get; set; }

    [DemandsInitialization("Whether to unarchive the files into the ForLoading folder, or just copy them as is")]
    public bool ExtractFilesFromArchive { get; set; }

    public abstract void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo);
    public abstract ExitCodeType Fetch(IDataLoadJob dataLoadJob, GracefulCancellationToken cancellationToken);

    #region Events

    public event CacheFileNotFoundHandler CacheFileNotFound;

    protected virtual void OnCacheFileNotFound(string message, Exception ex)
    {
        CacheFileNotFound?.Invoke(this, message, ex);
    }

    #endregion

    protected ICacheLayout CreateCacheLayout(ScheduledDataLoadJob job)
    {
        var cacheProgress = job.LoadProgress.CacheProgress ??
                            throw new NullReferenceException("cacheProgress cannot be null");
        return CreateCacheLayout(cacheProgress, job);
    }

    protected virtual ICacheLayout CreateCacheLayout(ICacheProgress cacheProgress, IDataLoadEventListener listener)
    {
        var pipelineFactory = new CachingPipelineUseCase(cacheProgress);
        var destination = pipelineFactory.CreateDestinationOnly(listener);
        return destination.CreateCacheLayout();
    }

    protected static ScheduledDataLoadJob ConvertToScheduledJob(IDataLoadJob dataLoadJob)
    {
        var scheduledJob = dataLoadJob as ScheduledDataLoadJob ??
                           throw new Exception(
                               "CachedFileRetriever can only be used in conjunction with a ScheduledDataLoadJob");
        return scheduledJob;
    }

    protected Dictionary<DateTime, FileInfo> GetDataLoadWorkload(ScheduledDataLoadJob job)
    {
        var cacheLayout = CreateCacheLayout(job);
        cacheLayout.CheckCacheFilesAvailability(job);

        _workload = new Dictionary<DateTime, FileInfo>();
        foreach (var date in job.DatesToRetrieve)
        {
            var fileInfo = cacheLayout.GetArchiveFileInfoForDate(date, job);

            if (fileInfo == null)
                OnCacheFileNotFound(
                    $"Could not find cached file for date '{date}' for CacheLayout.ArchiveType {cacheLayout.ArchiveType} in cache at {job.LoadDirectory.Cache.FullName}",
                    null);
            else if (!fileInfo.Exists)
                OnCacheFileNotFound(
                    $"Could not find cached file '{fileInfo.FullName}' for date {date} in cache at {job.LoadDirectory.Cache.FullName}",
                    null);

            _workload.Add(date, fileInfo);
        }

        return _workload;
    }

    private Dictionary<DateTime, FileInfo> _workload;

    private static string[] GetPathsRelativeToDirectory(FileInfo[] absoluteFilePaths, DirectoryInfo directory)
    {
        var relativeFilePaths = new List<string>();
        foreach (var path in absoluteFilePaths)
        {
            if (!path.FullName.StartsWith(directory.FullName))
                throw new InvalidOperationException(
                    $"The file must be within {directory.FullName} (or a subdirectory thereof)");

            relativeFilePaths.Add(path.FullName.Replace(directory.FullName, ""));
        }

        return relativeFilePaths.ToArray();
    }

    private bool FilesInForLoadingMatchWorkload(ILoadDirectory directory)
    {
        var filesInForLoading = GetPathsRelativeToDirectory(
            directory.ForLoading.EnumerateFiles("*", SearchOption.AllDirectories).ToArray(), directory.ForLoading);
        var filesFromCache = GetPathsRelativeToDirectory(_workload.Values.ToArray(), directory.Cache);

        return filesInForLoading.OrderBy(t => t).SequenceEqual(filesFromCache.OrderBy(t => t));
    }

    protected void ExtractJobs(IDataLoadJob dataLoadJob)
    {
        // check to see if forLoading has anything in it and bail if it does
        if (dataLoadJob.LoadDirectory.ForLoading.EnumerateFileSystemInfos().Any())
        {
            // RDMPDEV-185
            // There are files in ForLoading, but do they match what we would expect to find? Need to make sure that they aren't from a different dataset and/or there is the expected number of files
            // We should already have a _workload
            if (_workload == null)
                throw new InvalidOperationException(
                    "The workload has not been initialised, don't know what files are to be retrieved from the cache");

            if (!FilesInForLoadingMatchWorkload(dataLoadJob.LoadDirectory))
                throw new InvalidOperationException(
                    "The files in ForLoading do not match what this job expects to be loading from the cache. Please delete the files in ForLoading before re-attempting the data load.");

            dataLoadJob.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning, "ForLoading already has files, skipping extraction"));
            return;
        }

        var layout = CreateCacheLayout((ScheduledDataLoadJob)dataLoadJob);

        //extract all the jobs into the forLoading directory
        foreach (var job in _workload)
        {
            if (job.Value == null)
                continue;

            if (ExtractFilesFromArchive)
            {
                var extractor = CreateExtractor(layout.ArchiveType);
                extractor.Extract(job, dataLoadJob.LoadDirectory.ForLoading, dataLoadJob);
            }
            else
            {
                dataLoadJob.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Archive identified:{job.Value.FullName}"));

                // just copy the archives across
                var relativePath = GetPathRelativeToCacheRoot(dataLoadJob.LoadDirectory.Cache, job.Value);
                var absolutePath = Path.Combine(dataLoadJob.LoadDirectory.ForLoading.FullName, relativePath);
                if (!Directory.Exists(absolutePath))
                    Directory.CreateDirectory(absolutePath);

                var destFileName = Path.Combine(absolutePath, job.Value.Name);
                job.Value.CopyTo(destFileName);
            }
        }
    }

    private static string GetPathRelativeToCacheRoot(DirectoryInfo cacheRoot, FileInfo fileInCache)
    {
        return fileInCache
            .Directory.FullName.Replace(cacheRoot.FullName, "").TrimStart(Path.DirectorySeparatorChar);
    }

    private static IArchivedFileExtractor CreateExtractor(CacheArchiveType cacheArchiveType)
    {
        return cacheArchiveType switch
        {
            CacheArchiveType.None => throw new Exception("At this stage a cache archive type must be specified"),
            CacheArchiveType.Zip => new ZipExtractor(),
            _ => throw new ArgumentOutOfRangeException(nameof(cacheArchiveType))
        };
    }

    public static bool Validate(ILoadDirectory destination)
    {
        if (destination.Cache == null)
            throw new NullReferenceException(
                $"Destination {destination.RootPath.FullName} does not have a 'Cache' folder");

        return !destination.Cache.Exists ? throw new DirectoryNotFoundException(destination.Cache.FullName) : true;
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
        try
        {
            if (LoadProgress == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("A LoadProgress must be selected for a Cache to run",
                    CheckResult.Fail));
                return;
            }

            var cp = LoadProgress.CacheProgress;

            if (cp == null)
            {
                notifier.OnCheckPerformed(
                    new CheckEventArgs(
                        "LoadProgress must have a CacheProgress associated with it to support CachedFileRetrieval",
                        CheckResult.Fail));
                return;
            }

            var layout = CreateCacheLayout(cp, new FromCheckNotifierToDataLoadEventListener(notifier));

            if (layout == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("CacheLayout created was null!", CheckResult.Fail));
                return;
            }

            notifier.OnCheckPerformed(new CheckEventArgs($"Archive type is:{layout.ArchiveType}", CheckResult.Success));
            notifier.OnCheckPerformed(new CheckEventArgs($"DateFormat is:{layout.DateFormat}", CheckResult.Success));
            notifier.OnCheckPerformed(new CheckEventArgs($"Granularity is:{layout.CacheFileGranularity}",
                CheckResult.Success));

            notifier.OnCheckPerformed(new CheckEventArgs($"CacheLayout is:{layout}", CheckResult.Success));

            var filesFound = layout.CheckCacheFilesAvailability(new FromCheckNotifierToDataLoadEventListener(notifier));

            notifier.OnCheckPerformed(new CheckEventArgs($"Files Found In Cache:{filesFound}",
                filesFound ? CheckResult.Success : CheckResult.Warning));

            var d = layout.GetLoadCacheDirectory(new FromCheckNotifierToDataLoadEventListener(notifier));

            if (d == null)
            {
                notifier.OnCheckPerformed(new CheckEventArgs("Cache Directory was null!", CheckResult.Fail));
                return;
            }

            notifier.OnCheckPerformed(new CheckEventArgs($"Cache Directory Is:{d.FullName}", CheckResult.Success));
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Checking failed on {this}", CheckResult.Fail, ex));
        }
    }
}

public delegate void CacheFileNotFoundHandler(object sender, string message, Exception ex);
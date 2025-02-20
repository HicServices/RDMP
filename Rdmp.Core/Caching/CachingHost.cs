// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rdmp.Core.Caching.Pipeline;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching;

/// <summary>
/// Boot straps a
/// </summary>
public class CachingHost
{
    /// <summary>
    /// The cacheable tasks that the host will be running
    /// </summary>
    public ICacheProgress CacheProgress { get; set; }

    /// <summary>
    /// True if the host is attempting to back fill the cache with failed date ranges from the past
    /// </summary>
    public bool RetryMode { get; set; }

    private readonly ICatalogueRepository _repository;

    // this is more because we can't retrieve CacheWindows from LoadProgress (yet)
    private List<PermissionWindowCacheDownloader> _downloaders;

    /// <summary>
    /// True to shut down once the <see cref="PermissionWindow"/> for the <see cref="CacheProgress"/> is exceeded.  False
    /// to sleep until it becomes permissible again.
    /// </summary>
    public bool TerminateIfOutsidePermissionWindow { get; set; }

    /// <summary>
    /// Creates a new <see cref="CachingHost"/> connected to the RDMP <paramref name="repository"/>.
    /// </summary>
    /// <param name="repository"></param>
    public CachingHost(ICatalogueRepository repository)
    {
        _repository = repository;
        RetryMode = false;
    }

    /// <summary>
    /// Runs the first (which must be the only) <see cref="CacheProgress"/>
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="cancellationToken"></param>
    public void Start(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        if (CacheProgress == null)
            throw new InvalidOperationException("No CacheProgress has been set");

        listener.OnNotify(this,
            new NotifyEventArgs(ProgressEventType.Information,
                $"CachingHost started for {CacheProgress} (CacheFillProgress={CacheProgress.CacheFillProgress}, CacheLagPeriod={CacheProgress.CacheLagPeriod}, ChunkPeriod={CacheProgress.ChunkPeriod}, Pipeline={CacheProgress.Pipeline})"));

        var permissionWindow = CacheProgress.PermissionWindow;

        _downloaders = new List<PermissionWindowCacheDownloader>
        {
            new(permissionWindow, new List<ICacheProgress>(new[] { CacheProgress }), new RoundRobinPipelineExecution())
        };

        RetrieveNewDataForCache(listener, cancellationToken);
    }

    private void RetrieveNewDataForCache(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Retrieving new data"));

        var combinedToken = cancellationToken.CreateLinkedSource().Token;

        // Start a task for each cache download permission window and wait until completion
        var tasks =
            _downloaders.Select(
                    downloader =>
                        Task.Run(() => DownloadUntilFinished(downloader, listener, cancellationToken), combinedToken))
                .ToArray();

        try
        {
            Task.WaitAll(tasks);
        }
        catch (AggregateException e)
        {
            var operationCanceledException = e.GetExceptionIfExists<OperationCanceledException>();
            if (operationCanceledException != null)
            {
                listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Operation cancelled", e));
            }
            else
            {
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Error, "Exception in downloader task whilst caching data",
                        e));
                throw;
            }
        }
    }

    private void DownloadUntilFinished(PermissionWindowCacheDownloader downloader, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = RetryMode
                    ? downloader.RetryDownload(listener, cancellationToken)
                    : downloader.Download(listener, cancellationToken);

                switch (result)
                {
                    case RetrievalResult.NotPermitted:

                        if (TerminateIfOutsidePermissionWindow)
                        {
                            listener.OnNotify(this,
                                new NotifyEventArgs(ProgressEventType.Information,
                                    "Download not permitted at this time so exiting"));

                            return;
                        }

                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information,
                                "Download not permitted at this time, sleeping for 60 seconds"));

                        // Sleep for a while, but keep one eye open for cancellation requests
                        const int sleepTime = 60000;
                        const int cancellationCheckInterval = 1000;
                        var elapsedTime = 0;
                        while (elapsedTime < sleepTime)
                        {
                            Task.Delay(cancellationCheckInterval).Wait();
                            cancellationToken.ThrowIfCancellationRequested();
                            elapsedTime += cancellationCheckInterval;
                        }

                        break;
                    case RetrievalResult.Complete:
                        listener.OnNotify(this,
                            new NotifyEventArgs(ProgressEventType.Information, "Download completed successfully."));
                        return;
                    default:
                        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                            $"Download ended: {result}"));
                        return;
                }
            }
        }
        catch (OperationCanceledException)
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Cache download cancelled: {downloader}"));
        }
    }
}
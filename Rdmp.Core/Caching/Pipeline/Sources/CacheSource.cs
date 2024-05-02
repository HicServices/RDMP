// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Caching.Requests.FetchRequestProvider;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Pipeline.Sources;

/// <summary>
///     Abstract base class for a pipeline component which makes time based fetch requests to produce time specific packets
///     of data which will be packaged into files
///     and stored further down the caching pipeline.  Use the Request property to determine which dates/times you are
///     supposed to handle within DoGetChunk.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class CacheSource<T> : ICacheSource, IPluginDataFlowSource<T>,
    IPipelineRequirement<ICatalogueRepository> where T : class, ICacheChunk
{
    public ICacheFetchRequestProvider RequestProvider { get; set; }
    public IPermissionWindow PermissionWindow { get; set; }

    protected T Chunk;
    protected ICacheFetchRequest Request;

    protected ICatalogueRepository CatalogueRepository;

    /// <summary>
    ///     Enforces behaviour required for logging unsuccessful cache requests and providing implementation-independent
    ///     checks, so that the plugin author
    ///     doesn't need to remember to call Request[Succeeded|Failed] or do general checks.  Plugin author provides
    ///     implementation-specific caching in
    ///     the 'DoGetChunk' function.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public virtual T GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
    {
        Request = RequestProvider.GetNext(listener);

        // If GetNext returns null, there are no further failures to process and we're done
        if (Request == null)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "The RequestProvider has no more requests to provide (RequestProvider.GetNext returned null)"));
            return null;
        }

        if (Request.CacheProgress == null)
            throw new InvalidOperationException(
                "The request has no CacheProgress item (in this case to determine when the lag period begins)");

        // Check if we will stray into the lag period with this fetch and if so signal we are finished
        var cacheLagPeriod = Request.CacheProgress.GetCacheLagPeriod();
        if (cacheLagPeriod != null && cacheLagPeriod.TimeIsWithinPeriod(Request.End))
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Information,
                    "The Request is for a time within the Cache Lag Period. This means we are up-to-date and can stop now."));
            return null;
        }

        Chunk = DoGetChunk(Request, listener, cancellationToken);

        if (Chunk is { Request: null } && Request != null)
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error,
                    "DoGetChunk completed and set a Chunk Successfully but the Chunk.Request was null.  Try respecting the Request property in your class when creating your Chunk."));

        return Chunk;
    }

    /// <summary>
    ///     Handles the current <paramref name="request" /> returning an appropriate <see cref="ICacheChunk" /> for the time
    ///     range specified.
    /// </summary>
    /// <param name="request">The period of time we want to fetch</param>
    /// <param name="listener">For auditing progress during the fetch</param>
    /// <param name="cancellationToken">Indicates if user is trying to cancel the process</param>
    public abstract T DoGetChunk(ICacheFetchRequest request, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken);

    public void PreInitialize(ICacheFetchRequestProvider value, IDataLoadEventListener listener)
    {
        RequestProvider = value;
    }

    public void PreInitialize(IPermissionWindow value, IDataLoadEventListener listener)
    {
        PermissionWindow = value;
    }

    public abstract void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny);
    public abstract void Abort(IDataLoadEventListener listener);
    public abstract T TryGetPreview();
    public abstract void Check(ICheckNotifier notifier);

    public void PreInitialize(ICatalogueRepository value, IDataLoadEventListener listener)
    {
        CatalogueRepository = value;
    }
}
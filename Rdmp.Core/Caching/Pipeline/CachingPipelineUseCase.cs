// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Caching.Pipeline.Sources;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Caching.Requests.FetchRequestProvider;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Pipeline;

/// <summary>
/// Describes the use case under which a caching is attempted for a given ICacheProgress.  This involves working out the ICacheFetchRequestProvider,
/// PermissionWindow etc.  Since the use case is used both for creating an engine for execution and for determining which IPipelines are compatible
/// with the use case the class can be used at execution and design time.  Therefore it is legal to define the use case even when the ICacheProgress does
/// not have a configured caching pipeline e.g. to facilitate the user selecting/creating an appropriate pipeline in the first place (set throwIfNoPipeline
/// to false under such circumstances).
/// </summary>
public sealed class CachingPipelineUseCase : PipelineUseCase
{
    private readonly ICacheProgress _cacheProgress;
    private readonly IPipeline _pipeline;

    /// <summary>
    /// Class for helping you to construct a caching pipeline engine instance with the correct context and initialization objects
    /// </summary>
    /// <param name="cacheProgress">The cache that will be run</param>
    /// <param name="ignorePermissionWindow">Set to true to ignore the CacheProgress.PermissionWindow (if any)</param>
    /// <param name="providerIfAny">The strategy for figuring out what dates to load the cache with e.g. failed cache fetches or new jobs from head of que?</param>
    /// <param name="throwIfNoPipeline"></param>
    public CachingPipelineUseCase(ICacheProgress cacheProgress, bool ignorePermissionWindow = false,
        ICacheFetchRequestProvider providerIfAny = null, bool throwIfNoPipeline = true)
    {
        IPermissionWindow permissionWindow;
        _cacheProgress = cacheProgress;
        var providerIfAny1 = providerIfAny;

        //if there is no permission window or we are ignoring it
        if (ignorePermissionWindow || cacheProgress.PermissionWindow_ID == null)
            permissionWindow = new SpontaneouslyInventedPermissionWindow(_cacheProgress);
        else
            permissionWindow = cacheProgress.PermissionWindow;

        providerIfAny1 ??= new CacheFetchRequestProvider(_cacheProgress)
        {
            PermissionWindow = permissionWindow
        };

        _pipeline = _cacheProgress.Pipeline;

        if (_pipeline == null && throwIfNoPipeline)
            throw new Exception($"CacheProgress {_cacheProgress} does not have a Pipeline configured on it");

        AddInitializationObject(_cacheProgress.Repository);

        // Get the LoadDirectory for the engine initialization
        var lmd = _cacheProgress.LoadProgress.LoadMetadata;

        if (string.IsNullOrWhiteSpace(lmd.LocationOfForLoadingDirectory))
        {
            if (throwIfNoPipeline)
                throw new Exception(
                    $"LoadMetadata '{lmd}' does not have a Load Directory specified, cannot create ProcessingPipelineUseCase without one");
        }
        else
        {

            AddInitializationObject(new LoadDirectory(lmd.LocationOfForLoadingDirectory,lmd.LocationOfForArchivingDirectory,lmd.LocationOfExecutablesDirectory,lmd.LocationOfCacheDirectory));
        }

        AddInitializationObject(providerIfAny1);
        AddInitializationObject(permissionWindow);

        GenerateContext();
    }

    protected override IDataFlowPipelineContext GenerateContextImpl()
    {
        //create the context using the standard context factory
        var contextFactory = new DataFlowPipelineContextFactory<ICacheChunk>();
        var context = contextFactory.Create(PipelineUsage.None);

        //adjust context: we want a destination requirement of ICacheFileSystemDestination so that we can load from the cache using the pipeline endpoint as the source of the data load
        context.MustHaveDestination = typeof(ICacheFileSystemDestination); //we want this freaky destination type
        context.MustHaveSource = typeof(ICacheSource);

        return context;
    }


    public ICacheFileSystemDestination CreateDestinationOnly(IDataLoadEventListener listener)
    {
        // get the current destination
        var destination = GetEngine(_pipeline, listener).DestinationObject ??
                          throw new Exception($"{_cacheProgress} does not have a DestinationComponent in its Pipeline");
        return destination as ICacheFileSystemDestination ?? throw new NotSupportedException(
            $"{_cacheProgress} pipeline destination is not an ICacheFileSystemDestination, it was {_cacheProgress.GetType().FullName}");
    }

    public IDataFlowPipelineEngine GetEngine(IDataLoadEventListener listener) => GetEngine(_pipeline, listener);


    /// <summary>
    /// Design time types
    /// </summary>
    private CachingPipelineUseCase() : base(new Type[]
    {
        typeof(ICacheFetchRequestProvider),
        typeof(IPermissionWindow),
        typeof(LoadDirectory),
        typeof(ICatalogueRepository)
    })
    {
        GenerateContext();
    }

    public static CachingPipelineUseCase DesignTime() => new();
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Caching.Requests;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Pipeline.Destinations;

/// <summary>
///     Time period for which cache chunks are stored / fetched.  Some caching tasks produce so many file system entries it
///     is nessesary to subdivide the cache by Hour.
/// </summary>
public enum CacheFileGranularity
{
    Hour,
    Day
}

/// <summary>
///     Abstract implementation of ICacheFileSystemDestination. Includes checks for CacheLayout construction and read/write
///     permissions to Cache directory.  To implement
///     this class you should implement an ICacheLayout (or use an existing one) and then use ProcessPipelineData to
///     populate the CacheDirectory with data according to the
///     ICacheLayout
/// </summary>
public abstract class CacheFilesystemDestination : ICacheFileSystemDestination, IPluginDataFlowComponent<ICacheChunk>,
    IDataFlowDestination<ICacheChunk>
{
    [DemandsInitialization(
        "Root directory for the cache. This overrides the default LoadDirectory cache location. This might be needed if you are caching a very large data set which needs its own dedicated storage resource, for example.")]
    public DirectoryInfo CacheDirectory { get; set; }

    public abstract ICacheChunk ProcessPipelineData(ICacheChunk toProcess, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken);

    public void PreInitialize(ILoadDirectory value, IDataLoadEventListener listener)
    {
        // CacheDirectory overrides LoadDirectory, so only set CacheDirectory if it is null (i.e. no alternative cache location has been configured in the destination component)
        CacheDirectory ??= value.Cache ??
                           throw new Exception(
                               "For some reason the LoadDirectory does not have a Cache specified and the FilesystemDestination component does not have an override CacheDirectory specified");
    }

    /// <summary>
    ///     Use CacheDirectory to create a new layout, this method should only be called after PreInitialize
    /// </summary>
    /// <returns></returns>
    public abstract ICacheLayout CreateCacheLayout();

    public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
    {
    }

    public abstract void Abort(IDataLoadEventListener listener);

    public static void Stop()
    {
    }

    public static void Abort()
    {
    }

    public bool SilentRunning { get; set; }

    public virtual void Check(ICheckNotifier notifier)
    {
        if (CacheDirectory == null)
            throw new InvalidOperationException(
                "CacheDirectory is null, ensure that pre-initialize has been called with a valid object before checking.");

        // If we have an overridden cache directory, ensure we can reach it and write to it
        if (CacheDirectory != null)
            try
            {
                var tempFilename = Path.Combine(CacheDirectory.FullName, ".test.txt");
                var sw = File.CreateText(tempFilename);
                sw.Close();
                sw.Dispose();
                File.Delete(tempFilename);
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Confirmed could write to/delete from the overridden CacheDirectory: {CacheDirectory.FullName}",
                    CheckResult.Success));
            }
            catch (Exception e)
            {
                notifier.OnCheckPerformed(new CheckEventArgs(
                    $"Could not write to the overridden CacheDirectory: {CacheDirectory.FullName}", CheckResult.Fail,
                    e));
            }

        // Check CacheLayout creation
        var cacheLayout = CreateCacheLayout();
        notifier.OnCheckPerformed(cacheLayout == null
            ? new CheckEventArgs(
                "The CacheLayout object in CacheFilesystemDestination is not being constructed correctly",
                CheckResult.Fail)
            : new CheckEventArgs("CacheLayout object in CacheFilesystemDestination is OK", CheckResult.Success));
    }
}
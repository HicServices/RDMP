// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.Caching.Unit;

public interface IFileDataFlowDestination : TestIFileDataFlowComponent;

public interface TestIFileDataFlowComponent
{
    IList<FileInfo> ProcessPipelineData(IList<FileInfo> toProcess, IDataLoadEventListener listener);
    void Dispose(IDataLoadEventListener listener);
}

// this is really a destination?
public class MoveToDirectory : IFileDataFlowDestination
{
    public DirectoryInfo DestinationDirectory { get; set; }

    public IList<FileInfo> ProcessPipelineData(IList<FileInfo> toProcess, IDataLoadEventListener listener)
    {
        if (DestinationDirectory.Parent == null)
            throw new Exception("The destination directory has no parent so a new set of filepaths cannot be created.");

        var movedFiles = new List<FileInfo>();
        foreach (var fileInfo in toProcess.ToList())
        {
            var filePath = Path.Combine(DestinationDirectory.Parent.FullName, "...?");
            fileInfo.MoveTo(filePath);
            movedFiles.Add(new FileInfo(filePath));
        }

        return movedFiles;
    }

    public void Dispose(IDataLoadEventListener listener)
    {
        throw new NotImplementedException();
    }
}

public class FilesystemCacheDestination : IFileDataFlowDestination, IPipelineRequirement<CacheProgress>,
    IPipelineRequirement<DirectoryInfo>
{
    public CacheProgress CacheProgress { get; set; }
    public DirectoryInfo CacheDirectory { get; set; }

    public IList<FileInfo> ProcessPipelineData(IList<FileInfo> toProcess, IDataLoadEventListener listener)
    {
        var layout = new ZipCacheLayoutOnePerDay(CacheDirectory, new NoSubdirectoriesCachePathResolver());

        var moveComponent = new MoveToDirectory
        {
            DestinationDirectory = layout.GetLoadCacheDirectory(listener)
        };

        moveComponent.ProcessPipelineData(toProcess, listener);

        // would be in CacheLayout, with it being a component
        // ? where does the date come from?
        // either going to be CacheFillProgress or CacheFillProgress + period, depending on fetch logic
        return CacheProgress.CacheFillProgress == null
            ? throw new Exception(
                "Should throw, but currently on first cache it is valid for the CacheFIllProgress to be null")
            : toProcess;
    }

    public void Dispose(IDataLoadEventListener listener)
    {
        throw new NotImplementedException();
    }

    public void PreInitialize(CacheProgress cacheProgress, IDataLoadEventListener listener)
    {
        CacheProgress = cacheProgress;
    }

    public void PreInitialize(DirectoryInfo cacheDirectory, IDataLoadEventListener listener)
    {
        CacheDirectory = cacheDirectory;
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Layouts;

/// <summary>
///     'static' information about the cache layout, as opposed to the resolver which will give information for specific
///     cache configurations
///     Cache layout is effectively based on date with load schedule-specific sub directories with dataset-specific layout
///     information provided through the Resolver
/// </summary>
public interface ICacheLayout
{
    //Readonly fields you should set in your constructor
    string DateFormat { get; }
    CacheArchiveType ArchiveType { get; }
    CacheFileGranularity CacheFileGranularity { get; }

    //Consider taking these as parameters to your constructor - see CacheLayout abstract class for how you should probably implement this interface
    ILoadCachePathResolver Resolver { get; }
    DirectoryInfo RootDirectory { get; }

    // some interface for looking up filename
    DateTime? GetEarliestDateToLoadAccordingToFilesystem(IDataLoadEventListener listener);
    DateTime? GetMostRecentDateToLoadAccordingToFilesystem(IDataLoadEventListener listener);
    void CreateIfNotExists(IDataLoadEventListener listener);
    bool CheckExists(DateTime archiveDate, IDataLoadEventListener listener);
    FileInfo GetArchiveFileInfoForDate(DateTime archiveDate, IDataLoadEventListener listener);
    DirectoryInfo GetLoadCacheDirectory(IDataLoadEventListener listener);

    // requested by DLE
    Queue<DateTime> GetSortedDateQueue(IDataLoadEventListener listener);
    bool CheckCacheFilesAvailability(IDataLoadEventListener listener);
}
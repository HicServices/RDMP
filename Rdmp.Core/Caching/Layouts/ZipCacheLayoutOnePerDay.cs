// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Caching.Layouts;

/// <summary>
///     Alternative cache layout to BasicCacheLayout in which files are expected to be in a zip file instead of a directory
///     (e.g. in .\Data\Cache\2001-01-01.zip, .\Data\Cache\2001-01-02.zip etc)
/// </summary>
public class ZipCacheLayoutOnePerDay : CacheLayout
{
    public ZipCacheLayoutOnePerDay(DirectoryInfo rootCacheDirectory, ILoadCachePathResolver resolver)
        : base(rootCacheDirectory, "yyyy-MM-dd", CacheArchiveType.Zip, CacheFileGranularity.Day, resolver)
    {
    }
}
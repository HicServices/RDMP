// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Curation;
using Rdmp.Core.DataFlowPipeline.Requirements;

namespace Rdmp.Core.Caching.Pipeline.Destinations;

/// <summary>
///     Interface for minimum requirements of a cache destination pipeline component.  See abstract base class
///     CacheFilesystemDestination for details on what this is.  This
///     interface exists so that DLE (and other) processes can process the pipeline destination (e.g. to read files out of
///     it again) without having to know the exact caching
///     context etc.  Any ICacheFileSystemDestination must be able to CreateCacheLayout based solely on an ILoadDirectory
/// </summary>
public interface ICacheFileSystemDestination : IPipelineRequirement<ILoadDirectory>
{
    ICacheLayout CreateCacheLayout();
}
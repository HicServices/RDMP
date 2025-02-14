// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Caching.Requests.FetchRequestProvider;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline.Requirements;

namespace Rdmp.Core.Caching.Pipeline.Sources;

/// <summary>
/// Interface for abstract base CacheSource (See CacheSource for description).  All CacheSources should be exposed to and consider both the ICacheFetchRequestProvider
/// (which tells you what date/time you are supposed to be fetching) and IPermissionWindow (which tells you what real time window you can make requests during e.g. only
/// attempt to cache data between 9am and 5pm at night)
/// </summary>
public interface ICacheSource : IPipelineRequirement<ICacheFetchRequestProvider>,
    IPipelineRequirement<IPermissionWindow>;
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Caching.Requests;

/// <summary>
///     An instruction for an ICacheSource to request a specific date/time range of data.  The ICacheFetchRequest will also
///     be available in the ICacheChunk which is the T
///     flow object of a caching pipeline (See CachingPipelineUseCase) this means that the destination can ensure that the
///     data read goes into the correct sections of the
///     file system.
/// </summary>
public interface ICacheFetchRequest
{
    IRepository Repository { get; set; }

    void SaveCacheFillProgress(DateTime cacheFillProgress);

    DateTime Start { get; set; }
    DateTime End { get; }
    TimeSpan ChunkPeriod { get; set; }
    IPermissionWindow PermissionWindow { get; set; }
    ICacheProgress CacheProgress { get; set; }
    bool IsRetry { get; }

    void RequestFailed(Exception e);
    void RequestSucceeded();
    ICacheFetchRequest GetNext();
}
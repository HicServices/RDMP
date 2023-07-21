// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;

/// <summary>
///     MEF discoverable plugin implementation of IDataProvider intended to read from the ILoadProgress cache directory
///     (e.g. and unzip into
///     LoadDirectory.ForLoading) during data loading.  This is only required if you want to be able to change the way you
///     interact with
///     your cache (e.g. if you have a proprietary archive format).  In general you should try to ensure any caches you
///     create are compatible with
///     BasicCacheDataProvider and just use that instead.
/// </summary>
public interface ICachedDataProvider : IPluginDataProvider
{
    ILoadProgress LoadProgress { get; set; }
}
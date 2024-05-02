// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Caching.Requests;

/// <summary>
///     Base interface for templated Cache pipelines, you should inherit from this class and add whatever properties you
///     will use in your data classes e.g FileInfo[] property if your
///     cache involves downloading lots of files to fulfil the request
/// </summary>
public interface ICacheChunk
{
    ICacheFetchRequest Request { get; }
}
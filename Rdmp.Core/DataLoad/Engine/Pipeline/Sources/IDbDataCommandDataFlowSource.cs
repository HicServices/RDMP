// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Data;
using Rdmp.Core.DataFlowPipeline;

namespace Rdmp.Core.DataLoad.Engine.Pipeline.Sources;

/// <summary>
///     Reads records in Batches (of size BatchSize) from the remote database (DbConnectionStringBuilder builder) by
///     executing the specified _sql.
/// </summary>
public interface IDbDataCommandDataFlowSource : IDataFlowSource<DataTable>
{
    /// <summary>
    ///     Reads and returns a single row.  GetChunk must have been called at least once to function
    /// </summary>
    /// <returns></returns>
    DataRow ReadOneRow();
}
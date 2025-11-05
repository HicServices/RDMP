// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;

/// <summary>
/// Range of strategies to eliminate identical record duplication when extracting data sets from RDMP
/// </summary>
public enum DistinctStrategy
{
    /// <summary>
    /// Do not distinct the records extracted
    /// </summary>
    None = 0,

    /// <summary>
    /// Apply a DISTINCT keyword to the SELECT statement
    /// </summary>
    SqlDistinct,

    /// <summary>
    /// Apply an ORDER BY release id and apply the DISTINCT in memory as batches are read
    /// </summary>
    OrderByAndDistinctInMemory,
    /// <summary>
    /// Perform a GROUP BY on columns marked as extraction primary keys
    /// </summary>
    DistinctByDestinationPKs
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Curation.Data.Defaults;

/// <summary>
///     Fields that can be set or fetched from the ServerDefaults table in the Catalogue Database
/// </summary>
public enum PermissableDefaults
{
    /// <summary>
    ///     Null value/representation
    /// </summary>
    None = 0,

    /// <summary>
    ///     Relational logging database to store logs in while loading, running DQE, extracting etc
    /// </summary>
    LiveLoggingServer_ID,

    /// <summary>
    ///     Server to split sensitive identifiers off to during load (e.g. IdentifierDumper)
    /// </summary>
    IdentifierDumpServer_ID,

    /// <summary>
    ///     Server to store the results of running the DQE on datasets over time
    /// </summary>
    DQE,

    /// <summary>
    ///     Server to store cached results of <see cref="AggregateConfiguration" /> which are not sensitive and could be shown
    ///     on a website etc
    /// </summary>
    WebServiceQueryCachingServer_ID,

    /// <summary>
    ///     The RAW bubble server in data loads
    /// </summary>
    RAWDataLoadServer,

    /// <summary>
    ///     Server to store substituted ANO/Identifiable mappings for sensitive fields during data load e.g. GPCode, CHI, etc.
    /// </summary>
    ANOStore,

    /// <summary>
    ///     Server to store cached identifier lists of <see cref="AggregateConfiguration" />  which are part of a
    ///     <see cref="CohortIdentificationConfiguration" /> in order
    ///     to speed up performance of UNION/INTERSECT/EXCEPT section of the cohort building query.
    /// </summary>
    CohortIdentificationQueryCachingServer_ID
}
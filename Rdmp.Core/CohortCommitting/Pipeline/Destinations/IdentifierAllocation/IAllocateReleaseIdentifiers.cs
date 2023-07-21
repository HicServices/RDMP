// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;

/// <summary>
///     Class responsible for allocating Release Identifiers for a Cohort that is being committed (see
///     <see cref="BasicCohortDestination" />) when the user has not supplied any in
///     the file/cohort he is uploading.
/// </summary>
public interface IAllocateReleaseIdentifiers
{
    /// <summary>
    ///     Return a new (or existing) anonymous mapping for the provided <paramref name="privateIdentifier" />.  This will be
    ///     called for
    ///     novel identifiers only in a given batch being processed so you do not need to track your return values.
    /// </summary>
    /// <param name="privateIdentifier"></param>
    /// <returns></returns>
    object AllocateReleaseIdentifier(object privateIdentifier);

    /// <summary>
    ///     Called before any allocation, lets you know what <see cref="IProject" /> etc is involved in the cohort creation
    ///     attempt.
    /// </summary>
    /// <param name="request"></param>
    void Initialize(ICohortCreationRequest request);
}
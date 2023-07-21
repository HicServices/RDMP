// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.CohortCommitting.Pipeline.Destinations.IdentifierAllocation;

/// <summary>
///     Allocates a Guid for each private identifier supplied.  This will not keep track of duplicates (every call results
///     in a new guid regardless of the input).
/// </summary>
public class GuidReleaseIdentifierAllocator : IAllocateReleaseIdentifiers
{
    /// <summary>
    ///     Generates a new unique identifier as a string (does not do any form of lookup - every call is a new guid)
    /// </summary>
    /// <param name="privateIdentifier"></param>
    /// <returns></returns>
    public object AllocateReleaseIdentifier(object privateIdentifier)
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    ///     Does nothing
    /// </summary>
    /// <param name="request"></param>
    public void Initialize(ICohortCreationRequest request)
    {
    }
}
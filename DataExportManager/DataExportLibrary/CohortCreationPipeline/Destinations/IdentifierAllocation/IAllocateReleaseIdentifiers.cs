// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel.Composition;
using DataExportLibrary.Interfaces.Pipeline;

namespace DataExportLibrary.CohortCreationPipeline.Destinations.IdentifierAllocation
{
    /// <summary>
    /// Class responsible for allocating Release Identifiers for a Cohort that is being committed (see <see cref="BasicCohortDestination"/>) when the user has not supplied any in 
    /// the file/cohort he is uploading.
    /// </summary>
    [InheritedExport(typeof(IAllocateReleaseIdentifiers))]
    public interface IAllocateReleaseIdentifiers
    {
        object AllocateReleaseIdentifier(object privateIdentifier);
        void Initialize(ICohortCreationRequest request);
    }
}
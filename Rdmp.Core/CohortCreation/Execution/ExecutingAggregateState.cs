// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.CohortCreation.Execution;

/// <summary>
///     Describes the state of a cohort set or container in a <see cref="CohortCompiler" />
/// </summary>
public enum CompilationState
{
    /// <summary>
    ///     The set / container has been built but not executed yet
    /// </summary>
    NotScheduled,

    /// <summary>
    ///     The set / container SQL is being built (this is also the stage at which external APIs e.g. are running - see
    ///     <see cref="Catalogue.IsApiCall()" />)
    /// </summary>
    Building,

    /// <summary>
    ///     The set / container has been queued for execution but has not been sent to the server yet
    /// </summary>
    Scheduled,

    /// <summary>
    ///     The set / container is executing on the endpoint server
    /// </summary>
    Executing,

    /// <summary>
    ///     The set / container has finished executing successfully and should have a final cohort identifier count
    /// </summary>
    Finished,

    /// <summary>
    ///     The set / container has crashed.  This could be during building, execution or the caching stage
    /// </summary>
    Crashed
}
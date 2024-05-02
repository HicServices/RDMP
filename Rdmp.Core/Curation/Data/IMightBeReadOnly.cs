// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Marks an object which may be in a state that means the user should not edit it e.g. a filter that is part of an
///     extraction configuration that has been released and frozen
/// </summary>
public interface IMightBeReadOnly
{
    /// <summary>
    ///     Returns true if changes to the container should be forbidden e.g. because the parent object is frozen (like
    ///     <see cref="CohortIdentificationConfiguration.Frozen" />)
    /// </summary>
    /// <returns></returns>
    bool ShouldBeReadOnly(out string reason);
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Records the fact that a given Cohort Identification Configuration (query that identifies a cohort) is associated
///     with a given Project.  You can have multiple
///     associated configurations in a given project (e.g. cases, controls, time based etc).  You can also associate the
///     same configuration with multiple Projects if
///     you need to.
/// </summary>
public interface IProjectCohortIdentificationConfigurationAssociation : IMapsDirectlyToDatabaseTable, IMasqueradeAs,
    IDeletableWithCustomMessage, IMightBeReadOnly
{
    /// <summary>
    ///     The <see cref="IProject" /> to which the <see cref="CohortIdentificationConfiguration_ID" /> is associated with.
    /// </summary>
    int Project_ID { get; set; }

    /// <summary>
    ///     The <see cref="CohortIdentificationConfiguration" /> which is associated with the given <see cref="Project_ID" />.
    /// </summary>
    int CohortIdentificationConfiguration_ID { get; set; }

    /// <inheritdoc cref="Project_ID" />
    [NoMappingToDatabase]
    IProject Project { get; }

    /// <inheritdoc cref="CohortIdentificationConfiguration_ID" />
    [NoMappingToDatabase]
    CohortIdentificationConfiguration CohortIdentificationConfiguration { get; }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.QueryBuilding.Options;

/// <summary>
///     Builds <see cref="IAggregateBuilderOptions" /> from the current state of <see cref="AggregateConfiguration" />s.
/// </summary>
public class AggregateBuilderOptionsFactory
{
    /// <summary>
    ///     Creates an <see cref="IAggregateBuilderOptions" /> appropriate to the <see cref="AggregateConfiguration" />.  These
    ///     options indicate whether
    ///     it is functioning as a graph or cohort set and therefore which parts of the <see cref="AggregateBuilder" /> are
    ///     eligible to be modified.
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IAggregateBuilderOptions Create(AggregateConfiguration config)
    {
        var cohortIdentificationConfiguration = config.GetCohortIdentificationConfigurationIfAny();

        return cohortIdentificationConfiguration != null
            ? new AggregateBuilderCohortOptions(cohortIdentificationConfiguration.GetAllParameters())
            : new AggregateBuilderBasicOptions();
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Cohort.Joinables;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
///     <see cref="ICombineToMakeCommand" /> for an object of type <see cref="AggregateConfiguration" />
/// </summary>
public class AggregateConfigurationCombineable : ICombineToMakeCommand
{
    /// <summary>
    ///     The object selected for combining
    /// </summary>
    public AggregateConfiguration Aggregate { get; }

    /// <summary>
    ///     The <see cref="CohortIdentificationConfiguration" /> that the <see cref="Aggregate" /> belongs to if it is part of
    ///     cohort building
    /// </summary>
    public CohortIdentificationConfiguration CohortIdentificationConfigurationIfAny { get; }

    /// <summary>
    ///     The SET container (EXCEPT / UNION / INTERSECT) that the <see cref="Aggregate" /> is in if it is part of a
    ///     <see cref="CohortIdentificationConfiguration" />
    /// </summary>
    public CohortAggregateContainer ContainerIfAny { get; set; }

    /// <summary>
    ///     Comprehensive list of all <see cref="CohortAggregateContainer" /> in the tree hierarchy of the
    ///     <see cref="Aggregate" /> <see cref="CohortIdentificationConfigurationIfAny" />
    /// </summary>
    public List<CohortAggregateContainer> AllContainersInTreeIfPartOfOne { get; }

    /// <summary>
    ///     True if the <see cref="Aggregate" /> is <see cref="AggregateConfiguration.IsJoinablePatientIndexTable" />
    /// </summary>
    public bool IsPatientIndexTable { get; set; }

    /// <summary>
    ///     If the <see cref="Aggregate" /> is <see cref="IsPatientIndexTable" /> then this is the
    ///     <see cref="JoinableCohortAggregateConfiguration" />
    ///     declaration which makes it one (and links to the users of the patient index table - if any)
    /// </summary>
    public JoinableCohortAggregateConfiguration JoinableDeclarationIfAny { get; set; }

    /// <summary>
    ///     If the <see cref="Aggregate" /> is <see cref="IsPatientIndexTable" /> then this is all the users that join to it
    /// </summary>
    public AggregateConfiguration[] JoinableUsersIfAny { get; set; }

    /// <summary>
    ///     True if the <see cref="Aggregate" /> has an <see cref="ExtendedProperty" /> declaring it as a reusable template
    /// </summary>
    public bool IsTemplate { get; set; }

    /// <summary>
    ///     Creates a new instance, populates <see cref="Aggregate" /> and discovers all other state cached fields (e.g.
    ///     <see cref="JoinableDeclarationIfAny" /> etc).
    /// </summary>
    /// <param name="aggregate"></param>
    public AggregateConfigurationCombineable(AggregateConfiguration aggregate)
    {
        Aggregate = aggregate;

        IsPatientIndexTable = Aggregate.IsJoinablePatientIndexTable();

        IsTemplate = aggregate.CatalogueRepository.GetExtendedProperties(ExtendedProperty.IsTemplate, aggregate)
            .Any(p => Equals(p.Value, "true"));

        //is the aggregate part of cohort identification
        CohortIdentificationConfigurationIfAny = Aggregate.GetCohortIdentificationConfigurationIfAny();

        //assume no join users
        JoinableUsersIfAny = Array.Empty<AggregateConfiguration>();

        //unless there's a cic
        if (CohortIdentificationConfigurationIfAny != null)
        {
            //with this aggregate as a joinable
            JoinableDeclarationIfAny = CohortIdentificationConfigurationIfAny.GetAllJoinables()
                .SingleOrDefault(j => j.AggregateConfiguration_ID == Aggregate.ID);

            //then get the joinable users if any and use that array
            if (JoinableDeclarationIfAny != null)
                JoinableUsersIfAny = JoinableDeclarationIfAny.Users.Select(u => u.AggregateConfiguration).ToArray();
        }

        //if so we should find out all the containers in the tree (Containers are INTERSECT\EXCEPT\UNION)
        AllContainersInTreeIfPartOfOne = new List<CohortAggregateContainer>();

        //if it is part of cohort identification
        if (CohortIdentificationConfigurationIfAny != null)
        {
            //find all the containers so we can prevent us being copied into one of them
            var root = CohortIdentificationConfigurationIfAny.RootCohortAggregateContainer;
            AllContainersInTreeIfPartOfOne.Add(root);
            AllContainersInTreeIfPartOfOne.AddRange(root.GetAllSubContainersRecursively());
        }

        ContainerIfAny = Aggregate.GetCohortAggregateContainerIfAny();
    }

    public string GetSqlString()
    {
        return null;
    }
}
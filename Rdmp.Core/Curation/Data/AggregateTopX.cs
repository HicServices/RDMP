// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Allows you to limit the number of rows returned by an aggregate graph built by AggregateBuilder (or the number of
///     PIVOT lines in a graph).  If your AggregateConfiguration has no pivot
///     (and no axis) then the SELECT query that is generated will have a 'TOP X' and its 'ORDER BY' will be decided by
///     this class.  The most common use of this is to limit the results according
///     to the count column e.g. 'Only graph the top 10 most prescribed drugs'.  You can change the direction of the TopX
///     to turn it into 'Only graph the 10 LEAST prescribed drugs' or you can
///     change your count(*) SQL on the Aggregate to AVERAGE(dose) and then you would have 'Top 10 most prescribed drugs by
///     average prescription amount... or something like that anyway'.
///     <para>
///         Finally you can pick an AggregateDimension for the TopX to apply to other than the count column e.g. top 10
///         drug names by ascending would give
///         Asprin, Aardvarksprin, A... etc
///     </para>
/// </summary>
public class AggregateTopX : DatabaseEntity, IAggregateTopX
{
    private int _topX;
    private int? _orderByDimensionIfAny_ID;
    private AggregateTopXOrderByDirection _orderByDirection;
    private int _aggregateConfigurationID;

    #region Database Properties

    /// <summary>
    ///     ID of the aggregate graph that this topX applies to
    /// </summary>
    public int AggregateConfiguration_ID
    {
        get => _aggregateConfigurationID;
        set => SetField(ref _aggregateConfigurationID, value);
    }

    /// <inheritdoc />
    public int TopX
    {
        get => _topX;
        set => SetField(ref _topX, value);
    }

    /// <summary>
    ///     The dimension which the top X applies to, if null it will be the count / sum etc column (The AggregateCountColumn)
    /// </summary>
    public int? OrderByDimensionIfAny_ID
    {
        get => _orderByDimensionIfAny_ID;
        set => SetField(ref _orderByDimensionIfAny_ID, value);
    }

    /// <inheritdoc />
    public AggregateTopXOrderByDirection OrderByDirection
    {
        get => _orderByDirection;
        set => SetField(ref _orderByDirection, value);
    }

    #endregion


    #region Relationships

    /// <inheritdoc cref="OrderByDimensionIfAny_ID" />
    [NoMappingToDatabase]
    public AggregateDimension OrderByDimensionIfAny => OrderByDimensionIfAny_ID == null
        ? null
        : Repository.GetObjectByID<AggregateDimension>(OrderByDimensionIfAny_ID.Value);

    /// <inheritdoc cref="OrderByDimensionIfAny_ID" />
    [NoMappingToDatabase]
    public IColumn OrderByColumn => OrderByDimensionIfAny;

    #endregion

    public AggregateTopX()
    {
        OrderByDirection = AggregateTopXOrderByDirection.Descending;
    }

    /// <summary>
    ///     Creates an instance by reading it out of the database for the provided reader
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal AggregateTopX(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        AggregateConfiguration_ID = (int)r["AggregateConfiguration_ID"];
        TopX = (int)r["TopX"];
        OrderByDimensionIfAny_ID = ObjectToNullableInt(r["OrderByDimensionIfAny_ID"]);
        OrderByDirection = (AggregateTopXOrderByDirection)Enum.Parse(typeof(AggregateTopXOrderByDirection),
            r["OrderByDirection"].ToString());
    }

    /// <summary>
    ///     Defines that the given AggregateConfiguration should only return the top X records / pivot categories.  You can
    ///     only have a single AggregateTopX declared
    ///     for a given AggregateConfiguration (enforced with database constraints).
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="forConfiguration"></param>
    /// <param name="topX"></param>
    public AggregateTopX(ICatalogueRepository repository, AggregateConfiguration forConfiguration, int topX)
    {
        if (forConfiguration.GetTopXIfAny() != null)
            throw new Exception($"AggregateConfiguration {forConfiguration} already has a TopX");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "AggregateConfiguration_ID", forConfiguration.ID },
            { "TopX", topX },
            { nameof(OrderByDirection), AggregateTopXOrderByDirection.Descending }
        });
    }
}
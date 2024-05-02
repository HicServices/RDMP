// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Cohort.Joinables;

/// <summary>
///     Indicates that a given AggregateConfiguration in a CohortIdentificationConfiguration is implicitly joined against a
///     'PatientIndexTable' See JoinableCohortAggregateConfiguration
/// </summary>
public class JoinableCohortAggregateConfigurationUse : DatabaseEntity
{
    #region Database Properties

    private int _joinableCohortAggregateConfigurationID;
    private int _aggregateConfigurationID;
    private ExtractionJoinType _joinType;

    /// <summary>
    ///     Specifies the patient index table against which the <see cref="AggregateConfiguration_ID" /> should be joined with
    ///     at query generation time
    /// </summary>
    public int JoinableCohortAggregateConfiguration_ID
    {
        get => _joinableCohortAggregateConfigurationID;
        set => SetField(ref _joinableCohortAggregateConfigurationID, value);
    }

    /// <summary>
    ///     Specifies the <see cref="AggregateConfiguration" /> which should be joined with the referenced patient index table
    ///     (See <see cref="JoinableCohortAggregateConfiguration_ID" />)
    ///     at query generation time
    /// </summary>
    public int AggregateConfiguration_ID
    {
        get => _aggregateConfigurationID;
        set => SetField(ref _aggregateConfigurationID, value);
    }

    /// <summary>
    ///     Determines how the cohort set <see cref="AggregateConfiguration" /> will be joined against the patient index table
    ///     referenced by the <see cref="JoinableCohortAggregateConfiguration_ID" />
    ///     <para>
    ///         The cohort aggregate is always the 'left' table and the patient index table is the 'right' table.  The join
    ///         is performed on the patient identifier column
    ///     </para>
    /// </summary>
    public ExtractionJoinType JoinType
    {
        get => _joinType;
        set => SetField(ref _joinType, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="JoinableCohortAggregateConfiguration_ID" />
    [NoMappingToDatabase]
    public JoinableCohortAggregateConfiguration JoinableCohortAggregateConfiguration =>
        Repository.GetObjectByID<JoinableCohortAggregateConfiguration>(JoinableCohortAggregateConfiguration_ID);

    /// <inheritdoc cref="AggregateConfiguration_ID" />
    [NoMappingToDatabase]
    public AggregateConfiguration AggregateConfiguration =>
        Repository.GetObjectByID<AggregateConfiguration>(AggregateConfiguration_ID);

    #endregion

    public JoinableCohortAggregateConfigurationUse()
    {
    }

    internal JoinableCohortAggregateConfigurationUse(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        if (Enum.TryParse(r["JoinType"].ToString(), true, out ExtractionJoinType joinType))
            JoinType = joinType;

        JoinableCohortAggregateConfiguration_ID = Convert.ToInt32(r["JoinableCohortAggregateConfiguration_ID"]);
        AggregateConfiguration_ID = Convert.ToInt32(r["AggregateConfiguration_ID"]);
    }

    internal JoinableCohortAggregateConfigurationUse(ICatalogueRepository repository, AggregateConfiguration user,
        JoinableCohortAggregateConfiguration joinable)
    {
        if (repository.GetAllObjectsWhere<JoinableCohortAggregateConfiguration>("AggregateConfiguration_ID", user.ID)
            .Any())
            throw new NotSupportedException(
                $"Cannot add user {user} because that AggregateConfiguration is itself a JoinableCohortAggregateConfiguration");

        if (user.Catalogue.IsApiCall())
            throw new NotSupportedException(
                "API calls cannot join with PatientIndexTables (The API call must be self contained)");

        if (user.AggregateDimensions.Count(u => u.IsExtractionIdentifier) != 1)
            throw new NotSupportedException(
                $"Cannot configure AggregateConfiguration {user} as join user because it does not contain exactly 1 IsExtractionIdentifier dimension");

        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "JoinableCohortAggregateConfiguration_ID", joinable.ID },
            { "AggregateConfiguration_ID", user.ID },
            { "JoinType", ExtractionJoinType.Left.ToString() }
        });
    }

    /// <summary>
    ///     Translates <see cref="ExtractionJoinType" /> into an SQL keyword (LEFT / RIGHT etc).
    /// </summary>
    /// <returns></returns>
    public string GetJoinDirectionSQL()
    {
        return JoinType switch
        {
            ExtractionJoinType.Left => "LEFT",
            ExtractionJoinType.Right => "RIGHT",
            ExtractionJoinType.Inner => "INNER",
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    private const string ToStringPrefix = "JOIN Against:";
    private string _toStringName;

    /// <inheritdoc />
    public override string ToString()
    {
        return _toStringName ?? GetCachedName();
    }

    private string GetCachedName()
    {
        _toStringName =
            ToStringPrefix + JoinableCohortAggregateConfiguration.AggregateConfiguration.Name; //cached answer
        return _toStringName;
    }

    /// <summary>
    ///     Gets the table alias for the index table in the join sql query e.g. if the alias was ix123 then the query built
    ///     would be something like
    ///     <code>'select chi from Tbl1 INNER JOIN TblPatIndx ix123 on Tbl1.chi = ix123.chi where ix123.date > GETDATE()'</code>
    /// </summary>
    /// <returns></returns>
    public string GetJoinTableAlias()
    {
        return $"ix{JoinableCohortAggregateConfiguration_ID}";
    }
}
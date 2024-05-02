// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Data.Common;
using FAnsi.Discovery.QuerySyntax;
using FAnsi.Discovery.QuerySyntax.Aggregation;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Aggregation;

/// <summary>
///     Each AggregateConfiguration graph can have a single AggregateDimension defined as a date axis, this specifies the
///     start/end and increment of the aggregate e.g.
///     PrescribedDate dimension may have an axis defining it as running from 2001-2009 in increments of 1 month.
///     <para>For this to work the AggregateDimension output data should be of type a date also.</para>
/// </summary>
public class AggregateContinuousDateAxis : DatabaseEntity, IQueryAxis
{
    #region Database Properties

    private int _aggregateDimensionID;
    private string _startDate;
    private string _endDate;
    private AxisIncrement _axisIncrement;

    /// <summary>
    ///     The column (<see cref="AggregateDimension" /> in the AggregateConfiguration which this axis is defined on.  The
    ///     AggregateContinuousDateAxis defines
    ///     the axis (e.g.  2001-01-01 to GetDate() by Month ) while the AggregateDimension_ID is the pointer to the column on
    ///     which the axis applies within the
    ///     query.
    /// </summary>
    public int AggregateDimension_ID
    {
        get => _aggregateDimensionID;
        set => SetField(ref _aggregateDimensionID, value);
    }

    /// <summary>
    ///     The date or scalar function determining what date the graph axis should start at.  This could be as simple as
    ///     '2001-01-01' or complex like dateadd(yy, -1, GetDate())
    /// </summary>
    public string StartDate
    {
        get => _startDate;
        set => SetField(ref _startDate, value);
    }

    /// <summary>
    ///     The date or scalar function determining what date the graph axis should end at.  This could be as simple as
    ///     '2001-01-01' or complex GetDate()
    /// </summary>
    public string EndDate
    {
        get => _endDate;
        set => SetField(ref _endDate, value);
    }

    /// <summary>
    ///     Defines the increment of the axis which will be continuous buckets between <see cref="StartDate" /> and
    ///     <see cref="EndDate" />.
    /// </summary>
    public AxisIncrement AxisIncrement
    {
        get => _axisIncrement;
        set => SetField(ref _axisIncrement, value);
    }

    #endregion

    #region Relationships

    /// <inheritdoc cref="AggregateDimension_ID" />
    [NoMappingToDatabase]
    public AggregateDimension AggregateDimension => Repository.GetObjectByID<AggregateDimension>(AggregateDimension_ID);

    #endregion

    public AggregateContinuousDateAxis()
    {
    }

    /// <summary>
    ///     Defines that the specified column (<see cref="AggregateDimension" />) should function as the continuous axis of an
    ///     <see cref="AggregateConfiguration" /> graph.
    ///     For example if you are graphing the number of prescriptions given out each month then the axis would be applied to
    ///     the 'PrescribedDate' <see cref="AggregateDimension" />
    /// </summary>
    /// <remarks>
    ///     To use this you will first have to create an AggregateConfiguration and setup the count(*)/sum(*) etc stuff
    ///     and then add a new AggregateDimension <see cref="AggregateConfiguration.AddDimension" />
    /// </remarks>
    /// <param name="repository"></param>
    /// <param name="dimension"></param>
    public AggregateContinuousDateAxis(ICatalogueRepository repository, AggregateDimension dimension)
    {
        var todaysDateFunction = dimension.AggregateConfiguration.GetQuerySyntaxHelper()
            .GetScalarFunctionSql(MandatoryScalarFunctions.GetTodaysDate);

        repository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "AggregateDimension_ID", dimension.ID },
                { "EndDate", todaysDateFunction }
            });
    }

    /// <inheritdoc />
    internal AggregateContinuousDateAxis(ICatalogueRepository repository, DbDataReader r) : base(repository, r)
    {
        AggregateDimension_ID = int.Parse(r["AggregateDimension_ID"].ToString());
        StartDate = r["StartDate"].ToString();
        EndDate = r["EndDate"].ToString();
        AxisIncrement = (AxisIncrement)r["AxisIncrement"];
    }

    public override string ToString()
    {
        return "Axis";
    }
}
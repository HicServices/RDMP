// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data.Common;

namespace Rdmp.Core.DataExport.Data;

/// <inheritdoc />
public class ExternalCohortDefinitionData : IExternalCohortDefinitionData
{
    /// <summary>
    ///     Reads the externally held cohort descriptive data into memory from the <paramref name="r" />
    /// </summary>
    /// <param name="r"></param>
    /// <param name="tableName"></param>
    public ExternalCohortDefinitionData(DbDataReader r, string tableName)
    {
        ExternalProjectNumber = Convert.ToInt32(r["projectNumber"]);
        ExternalDescription = r["description"].ToString();
        ExternalVersion = Convert.ToInt32(r["version"]);
        ExternalCohortTableName = tableName;
        ExternalCohortCreationDate = ObjectToNullableDateTime(r["dtCreated"]);
    }

    private ExternalCohortDefinitionData()
    {
    }

    /// <inheritdoc />
    public int ExternalProjectNumber { get; set; }

    /// <inheritdoc />
    public string ExternalDescription { get; set; }

    /// <inheritdoc />
    public int ExternalVersion { get; set; }

    /// <inheritdoc />
    public string ExternalCohortTableName { get; set; }

    /// <inheritdoc />
    public DateTime? ExternalCohortCreationDate { get; set; }

    /// <summary>
    ///     Returns null for null or DBNull.Value otherwise the <see cref="DateTime" /> held in <paramref name="o" />
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static DateTime? ObjectToNullableDateTime(object o)
    {
        return o == null || o == DBNull.Value ? null : (DateTime)o;
    }

    /// <summary>
    ///     Describes the lack of available external data for an <see cref="ExtractableCohort" /> because the data has
    ///     been deleted from the cohort database
    /// </summary>
    public static IExternalCohortDefinitionData Orphan { get; } = new ExternalCohortDefinitionData
    {
        ExternalProjectNumber = -1,
        ExternalDescription = "Orphan Cohort",
        ExternalVersion = -1,
        ExternalCohortTableName = null,
        ExternalCohortCreationDate = null
    };
}
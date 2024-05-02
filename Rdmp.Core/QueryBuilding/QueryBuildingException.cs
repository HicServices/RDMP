// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     Thrown when there is a problem with QueryBuilding e.g. being unable to find a compatible IJoin (JoinInfo) between
///     two TableInfos required by a query (based on the columns
///     chosen for SELECT).
/// </summary>
public class QueryBuildingException : Exception
{
    /// <summary>
    ///     List of objects thought to be responsible for the query generation failing
    /// </summary>
    public List<IMapsDirectlyToDatabaseTable> ProblemObjects { get; }


    /// <inheritdoc cref="QueryBuildingException(string)" />
    public QueryBuildingException(string message, IEnumerable<IMapsDirectlyToDatabaseTable> problemObjects,
        Exception innerException = null) : base(message, innerException)
    {
        ProblemObjects = new List<IMapsDirectlyToDatabaseTable>(problemObjects);
    }

    /// <inheritdoc cref="QueryBuildingException(string)" />
    public QueryBuildingException(string message, Exception innerException) : base(message, innerException)
    {
        ProblemObjects = new List<IMapsDirectlyToDatabaseTable>();
    }

    /// <summary>
    ///     Creates a new Exception for when there is a problem with QueryBuilding e.g. being unable to find a compatible IJoin
    ///     (JoinInfo) between two TableInfos required by a query
    /// </summary>
    public QueryBuildingException(string message) : base(message)
    {
        ProblemObjects = new List<IMapsDirectlyToDatabaseTable>();
    }

    /// <summary>
    ///     Type unsafe overload of the IEnumerable'IMapsDirectlyToDatabaseTable' constructor, objects that are not of type
    ///     IMapsDirectlyToDatabaseTable will be ignored, use if you are slopy coding and have objects of interface type which
    ///     might be concrete IMapsDirectlyToDatabaseTable objects or might be spontaneous objects or nulls! - oh yeah that's
    ///     what this constructor does
    /// </summary>
    /// <param name="message"></param>
    /// <param name="problemObjects"></param>
    public QueryBuildingException(string message, params object[] problemObjects) : base(message)
    {
        ProblemObjects = new List<IMapsDirectlyToDatabaseTable>();
        ProblemObjects.AddRange(problemObjects.OfType<IMapsDirectlyToDatabaseTable>());
    }
}
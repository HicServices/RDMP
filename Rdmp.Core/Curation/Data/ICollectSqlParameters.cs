// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Interface for all objects which can have one or more sql parameter associated with them.  For example an IFilter
///     (line of WHERE Sql) might have 2 parameters @startDate
///     and @endDate then GetAllParameters should return the two ISqlParameter objects that contain the DECLARE, Comment
///     and Value setting Sql bits for these parameters.
///     <para>Each ISqlParameter should only ever have a single owner.</para>
/// </summary>
public interface ICollectSqlParameters
{
    /// <summary>
    ///     Returns all parameters declared directly against2 the current object.  This does not normally include sub objects
    ///     existing below the current
    ///     object which might have their own <see cref="ISqlParameter" />.
    /// </summary>
    /// <returns></returns>
    ISqlParameter[] GetAllParameters();
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;

namespace Rdmp.Core.QueryBuilding.Parameters;

/// <summary>
///     Stores the fact that a ParameterManager found a particular ISqlParameter while evaluating all objects involved in a
///     query being built (See ParameterManager for more
///     information).
/// </summary>
public class ParameterFoundAtLevel
{
    /// <summary>
    ///     The <see cref="ISqlParameter" /> that was found
    /// </summary>
    public ISqlParameter Parameter { get; set; }

    /// <summary>
    ///     The <see cref="ParameterLevel" /> that the <see cref="Parameter" /> was found at during query building.  This
    ///     allows parameters declared in individual <see cref="IFilter" /> to
    ///     be overridden by parameters declared at higher scopes
    /// </summary>
    public ParameterLevel Level { get; set; }

    /// <summary>
    ///     Defines that a given parameter was found at a given level during query building.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="level"></param>
    public ParameterFoundAtLevel(ISqlParameter parameter, ParameterLevel level)
    {
        Parameter = parameter;
        Level = level;
    }

    /// <summary>
    ///     Provides human readable description of the parameter and where it was found
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Parameter.ParameterName} (At Level:{Level})";
    }
}
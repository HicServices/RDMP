// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
/// Interface for objects able to summarise themselves as strings
/// </summary>
public interface ICanBeSummarised
{
    /// <summary>
    /// Generates a summary of the objects current state.  Containing only useful
    /// information to the user to understand the objects state.
    /// </summary>
    /// <param name="includeName">true to include the name or title of the object.  False to
    /// skip that out when summarising (e.g. if you are inserting into a body of text which
    /// already has a distinguishing title)</param>
    /// <param name="includeId"></param>
    /// <returns></returns>
    public string GetSummary(bool includeName, bool includeId);
}
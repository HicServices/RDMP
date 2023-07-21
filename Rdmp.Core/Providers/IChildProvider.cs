// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Providers;

/// <summary>
///     Returns children for a given model object (any object in an RDMPCollectionUI).  This should be fast and your
///     IChildProvider should pre load all the objects
///     and then return them as needed when GetChildren is called.
/// </summary>
public interface IChildProvider
{
    /// <summary>
    ///     Returns all children that should hierarchically exist below the <paramref name="model" /> or null if none
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    object[] GetChildren(object model);
}
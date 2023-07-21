// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
///     Indicates that an object is part of a network of dependant objects (e.g. CatalogueItem depends on Catalogue).
///     Ideally you should try to
///     list all IHasDependencies in a network of objects such that when A says it DependsOn B then B should report that A
///     is DependingOnThis (B)
///     but if there are a few missing links it won't end the world.  The reason to do this is so that from any point we
///     can find all related objects
///     up and down the hierarchies.
/// </summary>
public interface IHasDependencies
{
    /// <summary>
    ///     Objects which this class instance cannot exist without (things further up the dependency hierarchy)
    /// </summary>
    /// <returns></returns>
    IHasDependencies[] GetObjectsThisDependsOn();

    /// <summary>
    ///     Objects which this class knows depend on it (things further down the dependency hierarchy).
    /// </summary>
    /// <returns></returns>
    IHasDependencies[] GetObjectsDependingOnThis();
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Referencing;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Startup;

/// <summary>
///     Plugin databases can have an IRepository for interacting with them (the easiest way to implement this is to inherit
///     from TableRepository).  However
///     in order to construct the IRepository you likely need a connection string which might be stored in the catalogue
///     database (e.g. as an
///     ExternalDatabaseServer).
///     <para>
///         Plugin authors should inherit from PluginRepositoryFinder and return a suitable TableRepository for
///         saving/loading objects into the database at runtime.
///     </para>
/// </summary>
public interface IPluginRepositoryFinder
{
    /// <summary>
    ///     Returns an instance capable of loading and saving objects into the database.
    /// </summary>
    /// <returns></returns>
    PluginRepository GetRepositoryIfAny();

    /// <summary>
    ///     Returns the Type of object returned by <see cref="GetRepositoryIfAny" />.  This is used before constructing an
    ///     actual instance to decide whether or not a given
    ///     unknown object reference should be resolved by your <see cref="IRepository" /> or somebody elses (See
    ///     <see cref="IReferenceOtherObject" />).
    /// </summary>
    /// <returns></returns>
    Type GetRepositoryType();
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;

/// <summary>
/// Object (usually a IMapsDirectlyToDatabaseTable) which can have its state saved into a database but also have its current state compared with the
/// database state and (if necessary) unsaved changes can be discarded.
/// </summary>
public interface IRevertable : IMapsDirectlyToDatabaseTable, ISaveable
{
    /// <summary>
    /// Resets all public properties on the class to match the values stored in the <see cref="IRepository"/>
    /// </summary>
    void RevertToDatabaseState();

    /// <summary>
    /// Connects to the database <see cref="IRepository"/> and checks the values of public properties against the currently held (in memory)
    /// version of the class.
    /// </summary>
    /// <returns>Report about the differences if any to the class</returns>
    RevertableObjectReport HasLocalChanges();

    /// <summary>
    /// Connects to the database <see cref="IRepository"/> and returns true if the object (in memory) still exists in the database.
    /// </summary>
    /// <returns></returns>
    bool Exists();
}
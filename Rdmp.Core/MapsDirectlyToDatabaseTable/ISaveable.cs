// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
///     Object (usually a IMapsDirectlyToDatabaseTable) which can have its state saved into a database.  The object must
///     already exist in the database (See
///     IMapsDirectlyToDatabaseTable) and hence SaveToDatabase will just update the database values to match the changes in
///     memory and will not result in the
///     creation of any new records.
/// </summary>
public interface ISaveable
{
    /// <summary>
    ///     Saves the current values of all Properties not declared as <seealso cref="NoMappingToDatabase" /> to the database.
    /// </summary>
    void SaveToDatabase();
}
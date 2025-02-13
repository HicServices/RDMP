// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.ComponentModel;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
/// Indicates that a class cannot exist in memory without simultaneously existing as a record in a database table.  This is how RDMP handles continuous access
/// by multiple users and persistence of objects as well as allowing for enforcing program logic via database constraints.
/// 
/// <para>RDMP basically treats the database as main memory and has many classes which are directly checked out, modified and saved into the database.  These
/// classes must follow strict rules e.g. all public properties must directly match columns in the database table holding them (See DatabaseEntity).  This is
/// done in order to prevent corruption / race conditions / data loass etc in a multi user environment.</para>
/// </summary>
public interface IMapsDirectlyToDatabaseTable : IDeleteable, INotifyPropertyChanged
{
    /// <summary>
    /// Every database table that stores an <see cref="IMapsDirectlyToDatabaseTable"/> must have an identity column called ID which must be the primary key.
    /// Therefore for a given <see cref="IRepository"/> this uniquely identifies a given object.
    /// </summary>
    int ID { get; set; }

    /// <summary>
    /// The persistence database that stores the object.  For example a <see cref="TableRepository"/>.
    /// </summary>
    [NoMappingToDatabase]
    IRepository Repository { get; set; }

    /// <summary>
    /// Makes any persistent Property change attempts throw an Exception. (See also <see cref="INotifyPropertyChanged.PropertyChanged"/>)
    /// </summary>
    void SetReadOnly();

    //you must have a Property for each thing in your database table (With the same name)

    //use MapsDirectlyToDatabaseTableRepository to fully utilise this interface

    //ensure you have a the same class name as the table name DIRECTLY
    //ensure you have a constructor that initializes your object when passed a DbDataReader (parameter value) and DbCommand (how to update yourself)
    //these two things are required for MapsDirectlyToDatabaseTable.GetAllObjects and MapsDirectlyToDatabaseTable.GetObjectByID
}
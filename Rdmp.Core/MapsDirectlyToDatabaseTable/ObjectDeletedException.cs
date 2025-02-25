// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.MapsDirectlyToDatabaseTable;

/// <summary>
/// Thrown when you attempt an operation on an IMapsDirectlyToDatabaseTable object that has had its database entry deleted (either by you or another system
/// user).  All IMapsDirectlyToDatabaseTable objects must exist both in memory and in the database and once DeleteInDatabase is called on an IDeletable (or
/// however else the database copy disappears) then the memory copy becomes invalid and should not be used for anything.
/// </summary>
public class ObjectDeletedException : Exception
{
    public IMapsDirectlyToDatabaseTable DeletedObject { get; set; }

    public ObjectDeletedException(IMapsDirectlyToDatabaseTable deletedObject, Exception inner = null) : base(
        $"Object {deletedObject} has been deleted from the database", inner)
    {
        DeletedObject = deletedObject;
    }
}
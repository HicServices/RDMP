// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;

public enum RelationshipType
{
    /// <summary>
    ///     The decorated property reflects a reference to another shared object which must be supplied as part of the gathered
    ///     objects in a ShareDefinition.
    /// </summary>
    SharedObject,

    /// <summary>
    ///     The decorated property reflects a system boundary between shared objects and local objects.  The decorated property
    ///     should not
    ///     be a reference to a shared object.  Instead it should be maped to a local object on import.  For example when
    ///     sharing a CatalogueItem,
    ///     the associated ColumnInfo is not something that should be transmitted but it must exist and be selected before
    ///     CatalogueItem can be imported.
    /// </summary>
    LocalReference,

    /// <summary>
    ///     The decorated property reflects a system boundary between shared objects and local objects.  The decorated property
    ///     should not
    ///     be a reference to a shared object.  Instead it should be skipped entirely.  For example when sharing a Catalogue,
    ///     the associated
    ///     LoadMetadata is irrelevant and should not be shared (it should be left as null in the imported destination).
    /// </summary>
    IgnoreableLocalReference,

    /// <summary>
    ///     The decorated property reflects a reference to another shared object which may or may not be supplied as part of
    ///     the gathered objects (Optional).
    ///     If no shared object is included in a share then the marked property is ignored (i.e. behaves like a
    ///     <see cref="IgnoreableLocalReference" />).
    /// </summary>
    OptionalSharedObject
}
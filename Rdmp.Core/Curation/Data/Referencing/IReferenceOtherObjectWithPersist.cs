// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.Referencing;

/// <summary>
///     Interface for all objects which reference a single other object and correctly persist it to the RDMP database
/// </summary>
public interface IReferenceOtherObjectWithPersist : IReferenceOtherObject
{
    /// <summary>
    ///     The Type of object that was referred to (e.g. <see cref="Catalogue" />).  Must be an
    ///     <see cref="IMapsDirectlyToDatabaseTable" /> object
    /// </summary>
    string ReferencedObjectType { get; set; }

    /// <summary>
    ///     The ID of the object being refered to by this class
    /// </summary>
    int ReferencedObjectID { get; set; }

    /// <summary>
    ///     The platform database which is storing the object being referred to (e.g. DataExport or Catalogue)
    /// </summary>
    string ReferencedObjectRepositoryType { get; }
}
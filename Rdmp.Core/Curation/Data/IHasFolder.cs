// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Interface for objects that exist within a hierarchy of virtual folders.  The
///     <see cref="Folder" /> is primarily used in <see cref="FolderHelper.BuildFolderTree{T}(T[], FolderNode{T})" />
///     to build a virtual folder structure based on the current string values.
/// </summary>
public interface IHasFolder : IMapsDirectlyToDatabaseTable, ISaveable
{
    /// <summary>
    ///     A useful virtual folder in which to depict the object.  Note that this is not usually
    ///     a Directory (i.e. not a file system folder)
    /// </summary>
    string Folder { get; set; }
}
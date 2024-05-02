// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Linkng table between catalogues and loadmetadatas
/// </summary>
public interface ILoadMetadataCatalogueLinkage : IMapsDirectlyToDatabaseTable
{
    /// <summary>
    ///     Returns where the object exists (e.g. database) as <see cref="ICatalogueRepository" /> or null if the object does
    ///     not exist in a catalogue repository.
    /// </summary>
    ICatalogueRepository CatalogueRepository { get; }

    int LoadMetadataID { get; }
    int CatalogueID { get; }
}
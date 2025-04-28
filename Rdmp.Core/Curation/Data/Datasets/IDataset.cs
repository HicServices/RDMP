// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.Datasets;

/// <summary>
/// The core of datasets within RDMP.
/// Simple objects to link up catalogue data to DOI and datasets
/// </summary>
public interface IDataset: IMapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Returns where the object exists (e.g. database) as <see cref="ICatalogueRepository"/> or null if the object does not exist in a catalogue repository.
    /// </summary>
    ICatalogueRepository CatalogueRepository { get; }

    /// <summary>
    /// The Name of the Dataset
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The (optional) DOI of the dataset
    /// </summary>
    string DigitalObjectIdentifier { get; }

    /// <summary>
    /// The Source of the Dataset e.g. Pure, HDR
    /// </summary>
    string Type { get; }

    /// <summary>
    /// The URL to access the dataset
    /// </summary>
    string Url { get; }

    public abstract string GetID();
}

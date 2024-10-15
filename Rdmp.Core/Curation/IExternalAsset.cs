// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.


using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation;

/// <summary>
/// Used to store references to external assets.
/// Requires a valid ticketing system to connect to.
/// </summary>
public  interface IExternalAsset: IMapsDirectlyToDatabaseTable
{
    /// <summary>
    /// Firendly Name for the asset to be displayed
    /// </summary>
    public string Name { get;}

    /// <summary>
    /// The type of the object the asset is being linked to
    /// </summary>
    public string ObjectType { get; }

    /// <summary>
    /// The ticketing system used to reference the external API
    /// </summary>
    public int TicketingConfiguration_ID { get; }

    /// <summary>
    /// The ID of the asset on the remote system
    /// </summary>
    public int ExternalAsset_ID { get; }

    /// <summary>
    /// The ID of the object the asset is associated with
    /// </summary>
    public int ObjectId { get; }
}

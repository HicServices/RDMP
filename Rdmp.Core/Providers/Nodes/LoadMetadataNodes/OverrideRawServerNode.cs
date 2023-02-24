// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Providers.Nodes.UsedByNodes;

namespace Rdmp.Core.Providers.Nodes.LoadMetadataNodes;

internal class OverrideRawServerNode:ObjectUsedByOtherObjectNode<LoadMetadata,ExternalDatabaseServer>,IDeletableWithCustomMessage
{
    public OverrideRawServerNode(LoadMetadata user, ExternalDatabaseServer objectBeingUsed) : base(user, objectBeingUsed)
    {

    }

    public void DeleteInDatabase()
    {
        User.OverrideRAWServer_ID = null;
        User.SaveToDatabase();
    }

    public override string ToString()
    {
        return $"Override RAW:{ObjectBeingUsed.Name}";
    }

    public string GetDeleteMessage()
    {
        return "remove explicit RAW server";
    }
    /// <inheritdoc/>
    public string GetDeleteVerb()
    {
        return "Remove";
    }
}
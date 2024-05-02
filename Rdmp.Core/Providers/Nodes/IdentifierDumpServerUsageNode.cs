// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Identifies a database which is used to 'split off' identifiable data (columns) during a data load instead of
///     loading it into the LIVE database (from which you
///     execute data extractions).
/// </summary>
public class IdentifierDumpServerUsageNode : Node, IDeleteable
{
    public TableInfo TableInfo { get; }
    public ExternalDatabaseServer IdentifierDumpServer { get; }

    public IdentifierDumpServerUsageNode(TableInfo tableInfo, ExternalDatabaseServer identifierDumpServer)
    {
        TableInfo = tableInfo;
        IdentifierDumpServer = identifierDumpServer;
    }

    public override string ToString()
    {
        return $"Usage of:{IdentifierDumpServer.Name}";
    }

    protected bool Equals(IdentifierDumpServerUsageNode other)
    {
        return Equals(TableInfo, other.TableInfo);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((IdentifierDumpServerUsageNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TableInfo);
    }

    public void DeleteInDatabase()
    {
        TableInfo.IdentifierDumpServer_ID = null;
        TableInfo.SaveToDatabase();
    }
}
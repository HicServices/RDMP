// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Tree Node for documenting the allowed usage of a specific DataAccessCredentials (username / password) under a given
///     DataAccessContext (loading, extracting etc).
/// </summary>
public class DataAccessCredentialUsageNode : Node, IDeleteable
{
    public DataAccessCredentials Credentials { get; }
    public ITableInfo TableInfo { get; }
    public DataAccessContext Context { get; }

    public DataAccessCredentialUsageNode(DataAccessCredentials credentials, ITableInfo tableInfo,
        DataAccessContext context)
    {
        Credentials = credentials;
        TableInfo = tableInfo;
        Context = context;
    }

    public override string ToString()
    {
        return $"{Credentials} (Under Context:{Context})";
    }

    protected bool Equals(DataAccessCredentialUsageNode other)
    {
        return Equals(Credentials, other.Credentials) &&
               Equals(TableInfo, other.TableInfo) &&
               Context == other.Context;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DataAccessCredentialUsageNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Credentials, TableInfo, Context);
    }

    public void DeleteInDatabase()
    {
        TableInfo.CatalogueRepository.TableInfoCredentialsManager.BreakLinkBetween(Credentials, TableInfo, Context);
    }
}
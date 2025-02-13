// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;

/// <summary>
/// Depicts a server in a given DLE <see cref="LoadBubble"/> (e.g. the RAW server or the STAGING/LIVE server).
/// </summary>
public class LoadDiagramServerNode : TableInfoServerNode, IKnowWhatIAm, IOrderable
{
    private readonly LoadBubble _bubble;
    private readonly DiscoveredDatabase _database;
    private readonly string _description;

    public string ErrorDescription { get; private set; }

    private Dictionary<DiscoveredDatabase, TableInfo[]> _liveDatabaseDictionary;

    public readonly List<LoadDiagramDatabaseNode> Children = new();

    public LoadDiagramServerNode(LoadBubble bubble, DiscoveredDatabase database, TableInfo[] loadTables,
        HICDatabaseConfiguration config)
        : base(database.Server.Name, database.Server.DatabaseType, loadTables)
    {
        _bubble = bubble;
        _database = database;
        var serverName = database.Server.Name;

        _description = bubble switch
        {
            LoadBubble.Raw => $"RAW Server:{serverName}",
            LoadBubble.Staging => $"STAGING Server:{serverName}",
            LoadBubble.Live => $"LIVE Server:{serverName}",
            _ => throw new ArgumentOutOfRangeException(nameof(bubble))
        };

        //Live can have multiple databases (for lookups)
        if (_bubble == LoadBubble.Live)
        {
            var servers = loadTables.Select(static t => t.Server).Distinct().ToArray();
            if (servers.Length > 1)
            {
                _description = $"Ambiguous LIVE Servers:{string.Join(",", servers)}";
                ErrorDescription =
                    $"The TableInfo collection that underlie the Catalogues in this data load configuration are on different servers.  The servers they believe they live on are:{string.Join(",", servers)}.  All TableInfos in a load must belong on the same server or the load will not work.";
            }

            var databases = loadTables.Select(t => t.GetDatabaseRuntimeName()).Distinct().ToArray();

            _liveDatabaseDictionary = new Dictionary<DiscoveredDatabase, TableInfo[]>();

            foreach (var dbname in databases)
                _liveDatabaseDictionary.Add(_database.Server.ExpectDatabase(dbname),
                    loadTables.Where(t =>
                            t.GetDatabaseRuntimeName().Equals(dbname, StringComparison.CurrentCultureIgnoreCase))
                        .ToArray());
        }

        //if it is live yield all the lookups
        if (_bubble == LoadBubble.Live)
            foreach (var kvp in _liveDatabaseDictionary)
                Children.Add(new LoadDiagramDatabaseNode(_bubble, kvp.Key, kvp.Value, config));
        else
            Children.Add(new LoadDiagramDatabaseNode(_bubble, _database, loadTables, config));
    }

    public IEnumerable<LoadDiagramDatabaseNode> GetChildren() => Children;

    public override string ToString() => _description;

    public void DiscoverState()
    {
        foreach (var db in Children)
            db.DiscoverState();
    }

    #region equality

    protected bool Equals(LoadDiagramServerNode other) =>
        base.Equals(other) && _bubble == other._bubble && Equals(_database, other._database);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoadDiagramServerNode)obj);
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), _bubble, _database);

    public string WhatIsThis()
    {
        return _bubble switch
        {
            LoadBubble.Raw =>
                "Depicts what server will be used for the RAW database and the tables/columns that are anticipated/found in that server currently",
            LoadBubble.Staging =>
                "Depicts what server will be used for the STAGING database and the tables/columns that are anticipated/found in that server currently",
            LoadBubble.Live =>
                "Depicts the current live server that the load will target (based on which Catalogues are associated with the load)",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion

    public int Order
    {
        get => (int)_bubble;
        set { }
    }
}
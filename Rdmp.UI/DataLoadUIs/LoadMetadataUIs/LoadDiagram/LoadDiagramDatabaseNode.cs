// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;

/// <summary>
/// Depicts a database in a given DLE <see cref="LoadBubble"/>.  Given the Create/Destroy nature of load stages this
/// database may or may not map to an existing database.
/// </summary>
public class LoadDiagramDatabaseNode : Node, IHasLoadDiagramState, IKnowWhatIAm
{
    private readonly LoadBubble _bubble;
    public readonly DiscoveredDatabase Database;
    private readonly TableInfo[] _loadTables;
    private readonly HICDatabaseConfiguration _config;

    public LoadDiagramState State { get; set; }

    public string DatabaseName { get; private set; }

    public List<LoadDiagramTableNode> _anticipatedChildren = new();
    public List<UnplannedTable> _unplannedChildren = new();


    public LoadDiagramDatabaseNode(LoadBubble bubble, DiscoveredDatabase database, TableInfo[] loadTables,
        HICDatabaseConfiguration config)
    {
        _bubble = bubble;
        Database = database;
        _loadTables = loadTables;
        _config = config;

        DatabaseName = Database.GetRuntimeName();

        _anticipatedChildren.AddRange(_loadTables.Select(t => new LoadDiagramTableNode(this, t, _bubble, _config)));
    }

    public IEnumerable<object> GetChildren() => _anticipatedChildren.Cast<object>().Union(_unplannedChildren);

    public override string ToString() => DatabaseName;

    public Bitmap GetImage(ICoreIconProvider coreIconProvider) => coreIconProvider.GetImage(_bubble).ImageToBitmap();

    public void DiscoverState()
    {
        _unplannedChildren.Clear();

        if (!Database.Exists())
        {
            State = LoadDiagramState.NotFound;
            foreach (var plannedChild in _anticipatedChildren)
                plannedChild.SetStateNotFound();

            return;
        }

        //database does exist
        State = LoadDiagramState.Found;

        //so check the children (tables) for state
        foreach (var plannedChild in _anticipatedChildren)
            plannedChild.DiscoverState();

        //also discover any unplanned tables if not live
        if (_bubble != LoadBubble.Live)
            foreach (var discoveredTable in Database.DiscoverTables(true))
            {
                //it's an anticipated one
                if (_anticipatedChildren.Any(c =>
                        c.TableName.Equals(discoveredTable.GetRuntimeName(),
                            StringComparison.CurrentCultureIgnoreCase)))
                    continue;

                //it's unplanned (maybe user created it as part of his load script or something)
                _unplannedChildren.Add(new UnplannedTable(discoveredTable));
            }
    }

    #region equality

    protected bool Equals(LoadDiagramDatabaseNode other) =>
        string.Equals(DatabaseName, other.DatabaseName) && _bubble == other._bubble;

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoadDiagramDatabaseNode) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((DatabaseName != null ? DatabaseName.GetHashCode() : 0) * 397) ^ (int)_bubble;
        }
    }

    public string WhatIsThis()
    {
        return _bubble switch
        {
            LoadBubble.Raw =>
                "Depicts what database will be used for the RAW database and the tables/columns that are anticipated/found in that server currently",
            LoadBubble.Staging =>
                "Depicts what database will be used for the STAGING database and the tables/columns that are anticipated/found in that server currently",
            LoadBubble.Live =>
                "Depicts the current live database(s) that the load will target (based on which Catalogues are associated with the load)",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion
}
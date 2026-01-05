// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;

/// <summary>
/// Depicts a table in a given DLE <see cref="LoadBubble"/>.  Given the Create/Destroy nature of load stages this
/// node may or may not map to an existing table in the database.
/// </summary>
public class LoadDiagramTableNode : Node, ICombineableSource, IHasLoadDiagramState, IMasqueradeAs, IKnowWhatIAm
{
    private readonly LoadDiagramDatabaseNode _databaseNode;
    public readonly TableInfo TableInfo;
    public readonly LoadBubble Bubble;
    private readonly HICDatabaseConfiguration _config;

    public LoadDiagramState State { get; set; }

    public readonly DiscoveredTable Table;

    private List<LoadDiagramColumnNode> _anticipatedChildren = new();
    private List<DiscoveredColumn> _unplannedChildren = new();

    public LoadDiagramTableNode(LoadDiagramDatabaseNode databaseNode, TableInfo tableInfo, LoadBubble bubble,
        HICDatabaseConfiguration config)
    {
        _databaseNode = databaseNode;
        TableInfo = tableInfo;
        Bubble = bubble;
        _config = config;

        State = LoadDiagramState.Anticipated;

        TableName = TableInfo.GetRuntimeName(Bubble);

        //only reference schema if it is LIVE
        var schema = bubble >= LoadBubble.Live ? tableInfo.Schema : null;

        Table = databaseNode.Database.ExpectTable(TableName, schema);


        var cols =
            TableInfo.GetColumnsAtStage(Bubble.ToLoadStage())
                .Select(c => new LoadDiagramColumnNode(this, c, Bubble));

        _anticipatedChildren.AddRange(cols);
    }

    public string DatabaseName => _databaseNode.DatabaseName;
    public string TableName { get; private set; }

    public IEnumerable<object> GetChildren(bool dynamicColumnsOnly)
    {
        foreach (var c in _anticipatedChildren.Where(c =>
                     !dynamicColumnsOnly || c.IsDynamicColumn || c.State == LoadDiagramState.Different ||
                     c.State == LoadDiagramState.New))
            yield return c;

        foreach (var c in _unplannedChildren)
            yield return c;
    }

    public override string ToString() => TableName;

    public ICombineToMakeCommand GetCombineable() =>
        new SqlTextOnlyCombineable(TableInfo.GetQuerySyntaxHelper()
            .EnsureFullyQualified(DatabaseName, null, TableName));

    public void DiscoverState()
    {
        _unplannedChildren.Clear();

        //assume no children exist
        foreach (var anticipatedChild in _anticipatedChildren)
            anticipatedChild.State = LoadDiagramState.NotFound;

        //we don't exist either!
        if (!Table.Exists())
        {
            State = LoadDiagramState.NotFound;
            return;
        }

        //we do exist
        State = LoadDiagramState.Found;

        //discover children and marry them up to planned/ new unplanned ones
        foreach (var discoveredColumn in Table.DiscoverColumns())
        {
            var match = _anticipatedChildren.SingleOrDefault(c =>
                c.ColumnName.Equals(discoveredColumn.GetRuntimeName(), StringComparison.CurrentCultureIgnoreCase));
            if (match != null)
                match.SetState(discoveredColumn);
            else
                _unplannedChildren.Add(discoveredColumn); //unplanned column
        }

        //any NotFound or Different etc cols or any unplanned children
        if (_anticipatedChildren.Any(c => c.State > LoadDiagramState.Found) || _unplannedChildren.Any())
            State = LoadDiagramState.Different; //elevate our state to Different
    }


    public void SetStateNotFound()
    {
        State = LoadDiagramState.NotFound;

        foreach (var c in _anticipatedChildren)
            c.State = LoadDiagramState.NotFound;

        _unplannedChildren.Clear();
    }

    #region equality

    protected bool Equals(LoadDiagramTableNode other) => Equals(_databaseNode, other._databaseNode) &&
                                                         Bubble == other.Bubble &&
                                                         string.Equals(TableName, other.TableName);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoadDiagramTableNode)obj);
    }

    public override int GetHashCode() => HashCode.Combine(_databaseNode, Bubble, TableName);

    public string WhatIsThis()
    {
        switch (State)
        {
            case LoadDiagramState.Different:
            case LoadDiagramState.Anticipated:
            case LoadDiagramState.Found:
                return Bubble switch
                {
                    LoadBubble.Raw =>
                        "A Table that will be created in the RAW bubble when the load is run, this table will not have any constraints (not nulls, referential integrity etc)",
                    LoadBubble.Staging =>
                        "A Table that will be created in the STAGING bubble when the load is run, this table will have normal constraints that match LIVE",
                    _ => "A Table that is involved in the load (based on the Catalogues associated with the load)"
                };
            case LoadDiagramState.NotFound:
                return
                    "A Table that was expected to exist in the given load stage but didn't.  This is probably because no load is currently underway/crashed.";
            case LoadDiagramState.New:
                return
                    "A Table that was NOT expected to exist in the given load stage but did.  This may be a working table created by load scripts or a table that is part of another ongoing/crashed load";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public object MasqueradingAs() => Bubble == LoadBubble.Live ? TableInfo : null;

    #endregion
}
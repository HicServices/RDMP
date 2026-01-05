// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.Combining;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.DataLoad.Extensions;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Providers.Nodes;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram.StateDiscovery;

namespace Rdmp.UI.DataLoadUIs.LoadMetadataUIs.LoadDiagram;

/// <summary>
/// Depicts a column in a given DLE <see cref="LoadBubble"/>.  Given the Create/Destroy nature of load stages this
/// node may or may not map to an existing column in the database.
/// </summary>
public class LoadDiagramColumnNode : Node, ICombineableSource, IHasLoadDiagramState, IKnowWhatIAm
{
    private readonly LoadDiagramTableNode _tableNode;
    private readonly IHasStageSpecificRuntimeName _column;
    private readonly LoadBubble _bubble;
    private string _expectedDataType;
    private string _discoveredDataType;
    public string ColumnName { get; private set; }

    public LoadDiagramState State { get; set; }

    public LoadDiagramColumnNode(LoadDiagramTableNode tableNode, IHasStageSpecificRuntimeName column, LoadBubble bubble)
    {
        _tableNode = tableNode;
        _column = column;
        _bubble = bubble;
        ColumnName = _column.GetRuntimeName(_bubble.ToLoadStage());

        _expectedDataType = _column switch
        {
            PreLoadDiscardedColumn preLoadDiscarded => preLoadDiscarded.SqlDataType,
            ColumnInfo colInfo => colInfo.GetRuntimeDataType(_bubble.ToLoadStage()),
            _ => throw new Exception(
                $"Expected _column to be ColumnInfo or PreLoadDiscardedColumn but it was:{_column.GetType().Name}")
        };
    }

    public bool IsDynamicColumn
    {
        get
        {
            if (_column is PreLoadDiscardedColumn)
                return true;

            var colInfo = (ColumnInfo)_column;

            return colInfo.IsPrimaryKey || colInfo.ANOTable_ID != null || SpecialFieldNames.IsHicPrefixed(colInfo);
        }
    }

    public override string ToString() => ColumnName;

    public string GetDataType() => State == LoadDiagramState.Different ? _discoveredDataType : _expectedDataType;

    public ICombineToMakeCommand GetCombineable()
    {
        var querySyntaxHelper = _tableNode.TableInfo.GetQuerySyntaxHelper();

        return new SqlTextOnlyCombineable(querySyntaxHelper.EnsureFullyQualified(_tableNode.DatabaseName, null,
            _tableNode.TableName, ColumnName));
    }

    public Bitmap GetImage(ICoreIconProvider coreIconProvider)
    {
        //if its a ColumnInfo and RAW then use the basic ColumnInfo icon
        if (_column is ColumnInfo && _bubble <= LoadBubble.Raw)
            return coreIconProvider.GetImage(RDMPConcept.ColumnInfo).ImageToBitmap();

        //otherwise use the default Live/PreLoadDiscardedColumn icon
        return coreIconProvider.GetImage(_column).ImageToBitmap();
    }

    public void SetState(DiscoveredColumn discoveredColumn)
    {
        _discoveredDataType = discoveredColumn.DataType.SQLType;
        State = _discoveredDataType.Equals(_expectedDataType) ? LoadDiagramState.Found : LoadDiagramState.Different;
    }

    #region equality

    protected bool Equals(LoadDiagramColumnNode other) => _bubble == other._bubble &&
                                                          Equals(_tableNode, other._tableNode) &&
                                                          string.Equals(ColumnName, other.ColumnName);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoadDiagramColumnNode)obj);
    }

    public override int GetHashCode() => HashCode.Combine(_bubble, _tableNode, ColumnName);

    public string WhatIsThis()
    {
        return State switch
        {
            LoadDiagramState.Different or LoadDiagramState.Anticipated or LoadDiagramState.Found => _bubble switch
            {
                LoadBubble.Raw =>
                    "A Column that will be created in the RAW bubble when the load is run, this will not have any constraints (not nulls, referential integrity etc)",
                LoadBubble.Staging =>
                    "A Column that will be created in the STAGING bubble when the load is run, this will have normal constraints that match LIVE",
                _ => "A Column that is involved in the load (based on the Catalogues associated with the load)"
            },
            LoadDiagramState.NotFound =>
                "A Column that was expected to exist in the given load stage but didn't.  This is probably because no load is currently underway/crashed.",
            LoadDiagramState.New =>
                "A Column that was NOT expected to exist in the given load stage but did.  This may be a working table created by load scripts or a table that is part of another ongoing/crashed load",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    #endregion
}
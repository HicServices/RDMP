// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.UI.ProjectUI.Datasets.Node;

internal class AvailableForceJoinNode : IMasqueradeAs
{
    [CanBeNull] public SelectedDataSetsForcedJoin ForcedJoin { get; set; }

    public TableInfo TableInfo { get; set; }
    public JoinInfo[] JoinInfos { get; set; }
    public bool IsMandatory { get; set; }

    /// <summary>
    /// The table will be in the query if it IsMandatory (because of the columns the user has selected) or is explicitly picked for inclusion by the user (ForcedJoin)
    /// </summary>
    public bool IsIncludedInQuery => ForcedJoin != null || IsMandatory;

    public AvailableForceJoinNode(TableInfo tableInfo, bool isMandatory)
    {
        TableInfo = tableInfo;
        IsMandatory = isMandatory;
    }

    public object MasqueradingAs() => TableInfo;

    public override string ToString() => TableInfo.ToString();

    #region Equality Members

    protected bool Equals(AvailableForceJoinNode other) => TableInfo.Equals(other.TableInfo);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((AvailableForceJoinNode)obj);
    }

    public override int GetHashCode() => TableInfo.GetHashCode();

    #endregion

    /// <summary>
    /// Populates <see cref="JoinInfos"/> by finding all potential joins to <paramref name="otherNodes"/>
    /// </summary>
    /// <param name="coreChildProvider"></param>
    /// <param name="otherNodes"></param>
    public void FindJoinsBetween(ICoreChildProvider coreChildProvider, HashSet<AvailableForceJoinNode> otherNodes)
    {
        var allJoins = coreChildProvider.AllJoinInfos;
        var mycols = coreChildProvider.TableInfosToColumnInfos[TableInfo.ID].ToArray();

        var foundJoinInfos = new List<JoinInfo>();

        foreach (var theirCols in otherNodes.Where(otherNode => !Equals(otherNode, this))
                     .Select(otherNode => coreChildProvider.TableInfosToColumnInfos[otherNode.TableInfo.ID].ToArray()))
            foundJoinInfos.AddRange(
                TableInfo.CatalogueRepository.JoinManager.GetAllJoinInfosBetweenColumnInfoSets(allJoins, mycols,
                    theirCols));

        JoinInfos = foundJoinInfos.ToArray();
    }
}
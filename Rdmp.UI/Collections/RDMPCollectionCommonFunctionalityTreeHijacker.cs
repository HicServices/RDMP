// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using BrightIdeasSoftware;
using Rdmp.Core;

namespace Rdmp.UI.Collections;

/// <summary>
/// The Tree data model that is served by TreeFactoryGetter in RDMPCollectionCommonFunctionality.  Allows overriding of default TreeListView object model
/// functionality e.g. sorting.
/// 
/// <para>This implementation involves ensuring that ordering tree nodes always respects our OrderableComparer class.  This means that even when you reorder Projects
/// for example, the order of the subfolders (Cohorts, ExtractionConfigurations) doesn't change (which it would normally if only alphabetical comparing was done).</para>
/// </summary>
internal class RDMPCollectionCommonFunctionalityTreeHijacker : TreeListView.Tree
{
    private OLVColumn _lastSortColumn;
    private SortOrder _lastSortOrder;

    public RDMPCollectionCommonFunctionalityTreeHijacker(TreeListView treeView) : base(treeView)
    {
                
    }

    public override void Sort(OLVColumn column, SortOrder order)
    {
        _lastSortColumn = column;
        _lastSortOrder = order;

        base.Sort(column, order);

    }

    protected override TreeListView.BranchComparer GetBranchComparer()
    {   
        return new TreeListView.BranchComparer(
            new OrderableComparer(
                _lastSortColumn != null ? new ModelObjectComparer(_lastSortColumn, _lastSortOrder) : null)
        );
    }
}
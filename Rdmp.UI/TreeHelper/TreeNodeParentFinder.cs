// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BrightIdeasSoftware;

namespace Rdmp.UI.TreeHelper;

/// <summary>
/// Helper class for finding parents in a <see cref="TreeListView"/> that match a given Type.
/// </summary>
public class TreeNodeParentFinder
{
    private TreeListView _tree;

    public TreeNodeParentFinder(TreeListView tree)
    {
        _tree = tree;
    }
    public T GetFirstOrNullParentRecursivelyOfType<T>(object modelObject) where T : class
    {
        //get parent of node
        var parent = _tree.GetParent(modelObject);

        //if there is no parent
        if (parent == null)
            return default;//return null
            
        //if parent is correct type return it
        var correctType = parent as T;
        if (correctType != null)
            return correctType;

        //otherwise explore upwards on parent to get parent of correct type
        return GetFirstOrNullParentRecursivelyOfType<T>(parent);
    }

    public T GetLastOrNullParentRecursivelyOfType<T>(object modelObject,T lastOneFound = null) where T:class
    {
        //get parent of node
        var parent = _tree.GetParent(modelObject);

        //if there are no parents
        if (parent == null)
            return lastOneFound;//return what we found (if any)

        //found a parent of the correct type
        var correctType = parent as T;
        if (correctType != null)
            lastOneFound = correctType;

        //but either way we need to look further up for the last one
        return GetLastOrNullParentRecursivelyOfType<T>(parent, lastOneFound);

    }
}
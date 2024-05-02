// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cache;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Providers.Nodes.LoadMetadataNodes;

public class PermissionWindowUsedByCacheProgressNode : Node, IDeletableWithCustomMessage
{
    public CacheProgress CacheProgress { get; set; }
    public PermissionWindow PermissionWindow { get; }
    public bool DirectionIsCacheToPermissionWindow { get; set; }

    public PermissionWindowUsedByCacheProgressNode(CacheProgress cacheProgress, PermissionWindow permissionWindow,
        bool directionIsCacheToPermissionWindow)
    {
        CacheProgress = cacheProgress;
        PermissionWindow = permissionWindow;
        DirectionIsCacheToPermissionWindow = directionIsCacheToPermissionWindow;
    }

    public override string ToString()
    {
        return DirectionIsCacheToPermissionWindow ? PermissionWindow.Name : CacheProgress.ToString();
    }

    public void DeleteInDatabase()
    {
        CacheProgress.PermissionWindow_ID = null;
        CacheProgress.SaveToDatabase();
    }

    /// <inheritdoc />
    public string GetDeleteMessage()
    {
        return "remove PermissionWindow from CacheProgress";
    }

    /// <inheritdoc />
    public string GetDeleteVerb()
    {
        return "Remove";
    }

    #region Equality Members

    protected bool Equals(PermissionWindowUsedByCacheProgressNode other)
    {
        return CacheProgress.Equals(other.CacheProgress) &&
               PermissionWindow.Equals(
                   other.PermissionWindow) &&
               DirectionIsCacheToPermissionWindow.Equals(
                   other
                       .DirectionIsCacheToPermissionWindow);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((PermissionWindowUsedByCacheProgressNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CacheProgress, PermissionWindow, DirectionIsCacheToPermissionWindow);
    }

    #endregion

    public object GetImageObject()
    {
        return DirectionIsCacheToPermissionWindow ? PermissionWindow : CacheProgress;
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Providers.Nodes.LoadMetadataNodes;

public class LoadDirectoryNode : Node, IDirectoryInfoNode, IOrderable
{
    public LoadMetadata LoadMetadata { get; }

    public LoadDirectoryNode(LoadMetadata loadMetadata)
    {
        LoadMetadata = loadMetadata;
    }

    public bool IsEmpty => string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles) && string.IsNullOrWhiteSpace(LoadMetadata.LocationOfForLoadingDirectory);


    public override string ToString() => string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles)
        ? string.IsNullOrWhiteSpace(LoadMetadata.LocationOfForLoadingDirectory) ? "???" : "Custom"
        : LoadMetadata.LocationOfFlatFiles;

    public DirectoryInfo GetDirectoryInfoIfAny() => string.IsNullOrWhiteSpace(LoadMetadata.LocationOfFlatFiles)
        ? null
        : new DirectoryInfo(LoadMetadata.LocationOfFlatFiles);

    protected bool Equals(LoadDirectoryNode other) => Equals(LoadMetadata, other.LoadMetadata);

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((LoadDirectoryNode)obj);
    }

    public override int GetHashCode() => System.HashCode.Combine(LoadMetadata);

    public int Order
    {
        get => 3;
        set { }
    }
}
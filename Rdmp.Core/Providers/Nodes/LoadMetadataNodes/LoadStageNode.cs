// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Providers.Nodes.LoadMetadataNodes;

public class LoadStageNode : Node,IOrderable
{
    public LoadMetadata LoadMetadata { get; private set; }
    public LoadStage LoadStage { get; private set; }

    //prevent reordering
    public int Order { get { return (int)LoadStage; } set { } }

    public LoadStageNode(LoadMetadata loadMetadata, LoadStage loadStage)
    {
        LoadMetadata = loadMetadata;
        LoadStage = loadStage;
    }

    public override string ToString()
    {
        return LoadStage.ToString();
    }

    protected bool Equals(LoadStageNode other)
    {
        return Equals(LoadMetadata, other.LoadMetadata) && LoadStage == other.LoadStage;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((LoadStageNode) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((LoadMetadata != null ? LoadMetadata.GetHashCode() : 0)*397) ^ (int) LoadStage;
        }
    }
}
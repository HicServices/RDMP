// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Collection of all project specific datasets (<see cref="Catalogue" />s which can only be used with this
///     <see cref="Project" />).
/// </summary>
public class ProjectCataloguesNode : Node, IOrderable
{
    public Project Project { get; }

    public int Order
    {
        get => 5;
        set { }
    }

    public ProjectCataloguesNode(Project project)
    {
        Project = project;
    }

    public override string ToString()
    {
        return "Project Specific Catalogues";
    }

    protected bool Equals(ProjectCataloguesNode other)
    {
        return Project.Equals(other.Project);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProjectCataloguesNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Project);
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Providers.Nodes.ProjectCohortNodes;

/// <summary>
///     Collection of all saved cohort lists (<see cref="ExtractableCohort" />) which were saved against a
///     <see cref="Project" />.  Cohorts are
///     associated with the <see cref="Project" /> via the project number.
/// </summary>
public class ProjectSavedCohortsNode : Node, IOrderable
{
    public Project Project { get; }

    public ProjectSavedCohortsNode(Project project)
    {
        Project = project;
    }

    public override string ToString()
    {
        return "Saved Cohorts";
    }

    protected bool Equals(ProjectSavedCohortsNode other)
    {
        return Equals(Project, other.Project);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ProjectSavedCohortsNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Project);
    }

    public int Order
    {
        get => 2;
        set { }
    }
}
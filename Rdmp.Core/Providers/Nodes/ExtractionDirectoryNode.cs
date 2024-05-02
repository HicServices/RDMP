// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.Providers.Nodes.LoadMetadataNodes;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
///     Location on disk in which linked project extracts are generated for a given <see cref="Project" /> (assuming you
///     are extracting to disk
///     e.g. with an <see cref="ExecuteDatasetExtractionFlatFileDestination" />).
/// </summary>
public class ExtractionDirectoryNode : Node, IDirectoryInfoNode, IOrderable
{
    public Project Project { get; }

    public ExtractionDirectoryNode(Project project)
    {
        Project = project;
    }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Project.ExtractionDirectory) ? "???" : Project.ExtractionDirectory;
    }

    protected bool Equals(ExtractionDirectoryNode other)
    {
        return Equals(Project, other.Project);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ExtractionDirectoryNode)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Project);
    }

    public DirectoryInfo GetDirectoryInfoIfAny()
    {
        return string.IsNullOrWhiteSpace(Project.ExtractionDirectory)
            ? null
            : new DirectoryInfo(Project.ExtractionDirectory);
    }

    public int Order
    {
        get => 4;
        set { }
    }
}
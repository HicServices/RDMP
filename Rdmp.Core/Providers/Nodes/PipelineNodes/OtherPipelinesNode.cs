// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Pipelines;

namespace Rdmp.Core.Providers.Nodes.PipelineNodes;

/// <summary>
///     Pipelines are sequences of tailorable components which achieve a given goal (e.g. load a cohort).  This node is a
///     collection of all
///     pipelines for which are not compatible with core RDMP use cases (these might be broken or designed for plugin code
///     / custom goals -
///     e.g. loading imaging data).
/// </summary>
public class OtherPipelinesNode : SingletonNode
{
    public List<Pipeline> Pipelines { get; } = new();

    public OtherPipelinesNode() : base("Other Pipelines")
    {
    }
}
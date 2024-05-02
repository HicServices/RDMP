// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Logging;

namespace Rdmp.Core.DataFlowPipeline.Requirements;

/// <summary>
///     Default options for creating common pipeline contexts
/// </summary>
[Flags]
public enum PipelineUsage
{
    /// <summary>
    ///     There are no special flags for this pipeline context yet
    /// </summary>
    None = 0,

    /// <summary>
    ///     The usage context of the pipeline is that program will always set its own destination therefore no Pipelines can be
    ///     configured which have their own user defined
    ///     destination. When used in DataFlowPipelineContextFactory this prevents the addition of IDataFlowDestination
    ///     components
    /// </summary>
    FixedDestination = 1,

    /// <summary>
    ///     Pipeline puts data into a single <see cref="TableInfo" />
    /// </summary>
    LoadsSingleTableInfo = 2,

    /// <summary>
    ///     Pipeline must log to an already existing <see cref="TableLoadInfo" />
    /// </summary>
    LogsToTableLoadInfo = 4,

    /// <summary>
    ///     Pipeline takes as input a file which is expected to be read by the source
    /// </summary>
    LoadsSingleFlatFile = 8,

    /// <summary>
    ///     The pipeline cannot have a source, the source instance is provided at runtime by the environment
    /// </summary>
    FixedSource = 16
}
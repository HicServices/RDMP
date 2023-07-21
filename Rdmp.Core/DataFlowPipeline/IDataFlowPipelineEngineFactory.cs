// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataFlowPipeline;

/// <summary>
///     Factory for turning an IPipeline into a runnable engine.  See IDataFlowPipelineEngineFactory Generic T for full
///     description
/// </summary>
public interface IDataFlowPipelineEngineFactory
{
    /// <summary>
    ///     Turns the blueprint <see cref="IPipeline" /> into a runnable instance of <see cref="IDataFlowPipelineEngine" />.
    ///     This engine will be uninitialized
    ///     to start with.
    /// </summary>
    /// <param name="pipeline"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    IDataFlowPipelineEngine Create(IPipeline pipeline, IDataLoadEventListener listener);
}
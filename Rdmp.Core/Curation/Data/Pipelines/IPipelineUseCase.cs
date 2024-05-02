// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataFlowPipeline.Requirements;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Describes a specific use case for executing an IPipeline under.  This includes specifying the type T of the data
///     flow, if there is an explicit
///     source/destination component instance which must be used, what objects are available for PreInitialize on
///     components (GetInitializationObjects).
///     <para>
///         An instance of IPipelineUseCase is not just the general case (which is defined by IDataFlowPipelineContext) but
///         the specific hydrated use case
///         e.g. 'I want to Release Project 205'.
///     </para>
/// </summary>
public interface IPipelineUseCase : IHasDesignTimeMode
{
    /// <summary>
    ///     All the objects available for executing the Pipeline.
    ///     <para>
    ///         OR: If <see cref="IHasDesignTimeMode.IsDesignTime" /> then an array of the Types of objects that should be
    ///         around at runtime
    ///         when performing the task described by the PipelineUseCase
    ///     </para>
    /// </summary>
    /// <returns></returns>
    HashSet<object> GetInitializationObjects();

    /// <summary>
    ///     Returns all <see cref="Pipeline" /> from the collection which are compatible with the <see cref="GetContext" /> and
    ///     <see cref="GetInitializationObjects" />
    /// </summary>
    /// <param name="pipelines"></param>
    /// <returns></returns>
    IEnumerable<Pipeline> FilterCompatiblePipelines(IEnumerable<Pipeline> pipelines);

    /// <summary>
    ///     Returns true if the <paramref name="pipeline" /> is compatible with the use case
    /// </summary>
    /// <returns></returns>
    bool IsAllowable(Pipeline pipeline);

    /// <summary>
    ///     Returns an object describing which <see cref="Pipeline" />s can be used to undertake the activity described by this
    ///     use case (e.g. loading a flat file into the
    ///     database).  This includes the flow object (T) of and whether there are fixed sources/destinations as well as any
    ///     forbidden <see cref="PipelineComponent" /> types
    /// </summary>
    /// <returns></returns>
    IDataFlowPipelineContext GetContext();

    /// <summary>
    ///     The fixed runtime instance of <see cref="IDataFlowSource{T}" /> that will be used instead of an
    ///     <see cref="IPipelineComponent" /> when
    ///     running this use case. If this is populated then <see cref="Pipeline" />s cannot have a user configured source
    ///     component.
    /// </summary>
    object ExplicitSource { get; }

    /// <summary>
    ///     The fixed runtime instance of <see cref="IDataFlowDestination{T}" /> that will be used instead of an
    ///     <see cref="IPipelineComponent" /> when
    ///     running this use case. If this is populated then <see cref="Pipeline" />s cannot have a user configured destination
    ///     component.
    /// </summary>
    object ExplicitDestination { get; }

    /// <summary>
    ///     Constructs and engine from the provided <paramref name="pipeline" /> initialized by
    ///     <see cref="GetInitializationObjects" />
    /// </summary>
    /// <param name="pipeline"></param>
    /// <param name="listener"></param>
    /// <returns></returns>
    IDataFlowPipelineEngine GetEngine(IPipeline pipeline, IDataLoadEventListener listener);
}
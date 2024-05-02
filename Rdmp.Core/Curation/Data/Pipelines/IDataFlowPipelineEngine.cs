// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Interface for the Generic IDataFlowPipelineEngine T.  An IDataFlowPipelineEngine is a collection of
///     IDataFlowComponents starting with an IDataFlowSource and
///     ending with an IDataFlowDestination with any number of IDataFlowComponents in the middle.  Each component must
///     operate on the class that flows through which is
///     of type T (see the Generic implementation).
///     <para>
///         Before running the IDataFlowPipelineEngine you should call Initialize with the objects that are available for
///         IPipelineRequirement on components.
///     </para>
///     <para>See also Pipeline</para>
/// </summary>
public interface IDataFlowPipelineEngine : ICheckable
{
    /// <summary>
    ///     Runs all components from source to destination repeatedly until the source returs null;
    /// </summary>
    /// <param name="cancellationToken"></param>
    void ExecutePipeline(GracefulCancellationToken cancellationToken);

    /// <summary>
    ///     Runs the source GetChunk once and passes it down through the other components to the destination
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="completionUIAlerts"></param>
    /// <returns></returns>
    bool ExecuteSinglePass(GracefulCancellationToken cancellationToken,
        List<Tuple<string, IBasicActivateItems>> completionUIAlerts = null);

    /// <summary>
    ///     Components can declare IPipelineRequirement, calling this method will PreInitialize all components with compatible
    ///     IPipelineRequirements with the values
    ///     provided.  This is used to for example tell an ExecuteDatasetExtractionSource what IExtractCommand it is supposed
    ///     to be running.
    /// </summary>
    /// <param name="initializationObjects"></param>
    void Initialize(params object[] initializationObjects);

    /// <summary>
    ///     All middle IDataFlowComponents in the pipeline (except the source / destination)
    /// </summary>
    List<object> ComponentObjects { get; }

    /// <summary>
    ///     The IDataFlowDestination component at the end of the pipeline
    /// </summary>
    object DestinationObject { get; }

    /// <summary>
    ///     The IDataFlowSource component at the start of the pipeline
    /// </summary>
    object SourceObject { get; }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataFlowPipeline.Requirements;

/// <summary>
///     See DataFlowPipelineContext T Generic
/// </summary>
public interface IDataFlowPipelineContext
{
    /// <summary>
    ///     Returns the Type which must flow down pipeline engines assembled under this context e.g. DataTable, ReleaseAudit
    ///     etc.  This determines the T that all
    ///     <see cref="IDataFlowComponent{T}" /> must be implement in order to be compatible.
    /// </summary>
    /// <returns></returns>
    Type GetFlowType();

    /// <summary>
    ///     Determines whether ever single component in the pipeline (blueprint) is compatible with the current context (based
    ///     on Types, no instances are constructed).
    /// </summary>
    /// <param name="pipeline"></param>
    /// <returns></returns>
    bool IsAllowable(IPipeline pipeline);

    /// <summary>
    ///     Determines whether ever single component in the pipeline (blueprint) is compatible with the current context (based
    ///     on Types, no instances are constructed).
    /// </summary>
    /// <param name="pipeline"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    bool IsAllowable(IPipeline pipeline, out string reason);


    /// <summary>
    ///     Determines whether a given IDataFlowComponent Type is compatible with the current context
    /// </summary>
    /// <param name="t">The Type of an IDataFlowComponent</param>
    /// <returns>true if the component is compatible with the context</returns>
    bool IsAllowable(Type t);

    /// <summary>
    ///     Determines whether a given IDataFlowComponent Type is compatible with the current context
    /// </summary>
    /// <param name="type">The Type of an IDataFlowComponent</param>
    /// <param name="reason">
    ///     Null or the reason why the component is not compatible with the context e.g. it implements
    ///     interface X which is forbidden by the context
    /// </param>
    /// <returns>true if the component is compatible with the context</returns>
    bool IsAllowable(Type type, out string reason);

    /// <summary>
    ///     Types which <see cref="IPipelineComponent" /> classes (<see cref="IPipelineComponent.Class" />) are not allowed to
    ///     implement.
    /// </summary>
    HashSet<Type> CannotHave { get; }

    /// <summary>
    ///     Returns all IPipelineRequirement Types (T) which the given class implements e.g. class bob : IPipelineRequirement
    ///     &lt;Project&gt;, IPipelineRequirement&lt;ExtractableCohort&gt;
    ///     would return typeof(Project) and typeof(ExtractableCohort)
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    IEnumerable<Type> GetIPipelineRequirementsForType(Type t);


    /// <summary>
    ///     Calls all <see cref="IPipelineRequirement{T}.PreInitialize" /> methods on the <paramref name="component" /> (which
    ///     must be an <see cref="IDataFlowComponent{T}" /> or <see cref="IDataFlowSource{T}" />)
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="component"></param>
    /// <param name="initializationObjects"></param>
    void PreInitializeGeneric(IDataLoadEventListener listener, object component, params object[] initializationObjects);
}
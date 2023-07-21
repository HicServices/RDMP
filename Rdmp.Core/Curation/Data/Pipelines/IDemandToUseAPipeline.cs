// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Interface for components (of PipelineComponents or ProcessTasks) which have [DemandsInitialization] property(s) of
///     type Pipeline.  This lets you have a pipeline
///     component which requires the user select another Pipeline as one of its arguments.  You might want to do this for
///     example if you have a standard pipeline for
///     reading records and you want to use it in many places (in many other pipelines).  You must define the Context and
///     any Fixed components.  Note that you can even
///     set yourself (this) to the FixedDestination to effectively join two IPipelines together.
///     <para>
///         The user will only be able to select IPipelines which are compatible with the Context you provide (so it
///         won't for example override source/destination etc).
///     </para>
/// </summary>
public interface IDemandToUseAPipeline
{
    /// <summary>
    ///     Get an <see cref="IPipelineUseCase" /> which describes the flow type, required source/destination components and
    ///     the types of required initialization objects
    ///     that must be available for a <see cref="Pipeline" /> to be usable/selectable with the implementing component.
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    IPipelineUseCase GetDesignTimePipelineUseCase(RequiredPropertyInfo property);
}
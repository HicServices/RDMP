// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Each Pipeline has 0 or more PipelineComponents.  A Pipeline Component is a persistence record for a user
///     configuration of a class implementing IDataFlowComponent with
///     zero or more [DemandsInitialization] properties.  The class the user has chosen is stored in Class property and a
///     PipelineComponentArgument will exist for each
///     [DemandsInitialization] property.
///     <para>
///         PipelineComponents are turned into IDataFlowComponents when stamping out the Pipeline for use at a given time
///         (See DataFlowPipelineEngineFactory.Create)
///     </para>
///     <para>
///         PipelineComponent is the Design time class (where it appears in Pipeline, what argument values it should be
///         hydrated with etc) while IDataFlowComponent is
///         the runtime instance of the configuration.
///     </para>
/// </summary>
public interface IPipelineComponent : IArgumentHost, ISaveable, IMapsDirectlyToDatabaseTable, IHasDependencies,
    IOrderable
{
    /// <summary>
    ///     The human readable name for the pipeline component (currently this is the same as the <see cref="Class" />).
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     The <see cref="Pipeline" /> in which the component is configured
    /// </summary>
    int Pipeline_ID { get; set; }

    /// <summary>
    ///     The full name of the C# class Type which should be isntantiated and hydrated when using the <see cref="Pipeline" />
    ///     in which this component is configured.
    /// </summary>
    string Class { get; set; }

    /// <summary>
    ///     All the arguments for hydrating <see cref="Class" />
    /// </summary>
    IEnumerable<IPipelineComponentArgument> PipelineComponentArguments { get; }

    /// <summary>
    ///     Returns <see cref="Class" /> as a resolved System.Type
    /// </summary>
    /// <returns></returns>
    Type GetClassAsSystemType();

    /// <summary>
    ///     Returns the name only (without namespace) of the <see cref="Class" />
    /// </summary>
    /// <returns></returns>
    string GetClassNameLastPart();

    /// <summary>
    ///     Creates a new copy of the current <see cref="IPipelineComponent" /> into another <see cref="Pipeline" />
    ///     <paramref name="intoTargetPipeline" />
    ///     <para>This is usually done as part of <see cref="IPipeline.Clone" /></para>
    /// </summary>
    /// <param name="intoTargetPipeline"></param>
    /// <returns></returns>
    PipelineComponent Clone(Pipeline intoTargetPipeline);
}
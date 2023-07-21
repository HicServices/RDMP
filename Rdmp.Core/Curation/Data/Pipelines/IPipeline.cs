// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Describes the flow of strongly typed objects (usually DataTables) from a source to a destination (e.g. extracting
///     linked cohort data into a flat file ).
///     This entity is the serialized version of <see cref="IDataFlowPipelineEngine" /> (built by a
///     <see cref="IDataFlowPipelineEngineFactory" /> ).
///     <para>
///         Each <see cref="Pipeline" /> is composed of a sequence of <see cref="PipelineComponent" /> which can each
///         perform specific jobs e.g. 'clean strings', 'substitute column X for column Y by mapping values off of remote
///         server B'.
///     </para>
///     <para>
///         A Pipeline can be missing either/both a source and destination.  This means that the pipeline can only be used
///         in a situation where the context forces
///         a particular source/destination (for example if the user is trying to bulk insert a CSV file then the
///         Destination might be a fixed instance of DataTableUploadDestination
///         initialized with a specific server/database that the user had picked on a user interface).
///     </para>
///     <para>
///         Remember that Pipeline is the serialization, pipelines are used all over the place in RDMP software under
///         different contexts (caching, data extraction etc)
///         and sometimes we even create DataFlowPipelineEngine on the fly without even having a Pipeline serialization to
///         create it from.
///     </para>
/// </summary>
public interface IPipeline : IInjectKnown<IPipelineComponent[]>, INamed
{
    /// <summary>
    ///     Human readable description of the intended purpose of the pipeline as configured by the user.  Should include
    ///     what the pipeline is supposed to do.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    ///     The component acting as the source of the pipeline and producing data (e.g. by reading a flat file).  This
    ///     can be null if the <see cref="IPipelineUseCase" /> has a fixed runtime source instance instead.
    /// </summary>
    int? DestinationPipelineComponent_ID { get; set; }

    /// <summary>
    ///     The component acting as the destination of the pipeline and consuming data (e.g. writing records to a database).
    ///     This
    ///     can be null if the <see cref="IPipelineUseCase" /> has a fixed runtime destination instance instead.
    /// </summary>
    int? SourcePipelineComponent_ID { get; set; }

    /// <summary>
    ///     All components in the pipeline (including the source and destination)
    /// </summary>
    IList<IPipelineComponent> PipelineComponents { get; }

    /// <inheritdoc cref="DestinationPipelineComponent_ID" />
    IPipelineComponent Destination { get; }

    /// <inheritdoc cref="SourcePipelineComponent_ID" />
    IPipelineComponent Source { get; }


    /// <summary>
    ///     Creates a complete copy (in the database) of the current <see cref="Pipeline" /> including all new copies of
    ///     components and arguments
    /// </summary>
    /// <returns></returns>
    Pipeline Clone();
}
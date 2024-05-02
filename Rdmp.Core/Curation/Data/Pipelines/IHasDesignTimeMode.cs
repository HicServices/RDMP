// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataFlowPipeline.Requirements;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
///     Interface for classes which are requirements of a Pipeline (e.g. the file you want to load) but which might not be
///     available at design time
///     (e.g. when the user wants to edit the 'BULK UPLOAD Files' pipeline).  Rather than making the user pick a file
///     implement this interface and
///     provide a suitable static method for constructing the object  and mark it as IsDesignTime too.
///     <para>
///         PipelineComponents should check objects they are initialized with (See <see cref=" IPipelineRequirement{T}" />)
///         to see if they are <see cref="IHasDesignTimeMode" /> and have<see cref="IsDesignTime" /> before checking on
///         them (e.g. checking a file exists on disk).
///     </para>
/// </summary>
public interface IHasDesignTimeMode
{
    /// <summary>
    ///     True if the user is trying to edit a <see cref="Pipeline" /> independent of carrying out the task (i.e. no input
    ///     objects have been selected).
    /// </summary>
    bool IsDesignTime { get; }
}
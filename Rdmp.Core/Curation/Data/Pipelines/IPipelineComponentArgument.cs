// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.Curation.Data.Pipelines;

/// <summary>
/// See PipelineComponentArgument
/// </summary>
public interface IPipelineComponentArgument : IArgument, IHasDependencies
{
    /// <summary>
    /// Component for whom this <see cref="IPipelineComponentArgument"/> provides a value for.  There will be one <see cref="IPipelineComponentArgument"/>
    /// per public property with <see cref="DemandsInitializationAttribute"/> on the <see cref="PipelineComponent.Class"/>.
    /// </summary>
    int PipelineComponent_ID { get; set; }

    /// <summary>
    /// Creates a new copy of the current argument and associates it with <paramref name="intoTargetComponent"/>
    /// </summary>
    /// <param name="intoTargetComponent"></param>
    void Clone(PipelineComponent intoTargetComponent);
}
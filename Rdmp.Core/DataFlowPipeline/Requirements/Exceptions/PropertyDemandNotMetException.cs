// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using Rdmp.Core.Curation.Data.Pipelines;

namespace Rdmp.Core.DataFlowPipeline.Requirements.Exceptions;

/// <summary>
///     Thrown when a component blueprint (<see cref="PipelineComponent" />) could not be resolved into an instance because
///     a given property on the
///     class was not set up correctly (<see cref="PropertyInfo" />)
/// </summary>
internal class PropertyDemandNotMetException : Exception
{
    public IPipelineComponent PipelineComponent { get; private set; }
    public PropertyInfo PropertyInfo { get; private set; }

    public PropertyDemandNotMetException(string message, IPipelineComponent pipelineComponent,
        PropertyInfo propertyInfo)
        : base(message)
    {
        PipelineComponent = pipelineComponent;
        PropertyInfo = propertyInfo;
    }
}
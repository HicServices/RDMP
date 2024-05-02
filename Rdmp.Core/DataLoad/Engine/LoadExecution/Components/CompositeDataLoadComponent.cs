// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

/// <summary>
///     DataLoadComponent (DLE) that consists of running multiple subcomponents (also DataLoadComponents).  This is used
///     for composite stages e.g.
///     adjustStagingAndMigrateToLive where you want to run all or none (skip) of the components and pass the collection
///     around as a single object.
/// </summary>
public class CompositeDataLoadComponent : DataLoadComponent
{
    public IList<IDataLoadComponent> Components { get; }

    public CompositeDataLoadComponent(IList<IDataLoadComponent> components)
    {
        Components = components ?? new List<IDataLoadComponent>();
    }

    public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        if (Skip(job))
            return ExitCodeType.Error;

        foreach (var component in Components)
        {
            var result = component.Run(job, cancellationToken);

            job.PushForDisposal(component);

            if (result != ExitCodeType.Success)
                return result;
        }

        return ExitCodeType.Success;
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data;
using Rdmp.Core.Logging;

namespace Rdmp.Core.DataFlowPipeline.Requirements;

/// <summary>
/// Factory for constructing DataFlowPipelineContexts based on some handy presets.  Particularly helpful because of the weird way we enforce FixedDestination
/// (basically we forbid the IPipeline from having any IDataFlowDestination).  Feel free to adjust your context after the factory creates it.  This is very low
/// level functionality you should only need it if you are trying to define a new IPipelineUseCase for an entirely novel kind of pipeline usage.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DataFlowPipelineContextFactory<T>
{
    /// <summary>
    /// Creates a new <see cref="DataFlowPipelineContext{T}"/> set up with appropriate <see cref="DataFlowPipelineContext{T}.CannotHave"/> /
    /// <see cref="DataFlowPipelineContext{T}.MustHaveSource"/> etc for the given <paramref name="flags"/>.
    /// </summary>
    /// <param name="flags"></param>
    /// <returns></returns>
    public DataFlowPipelineContext<T> Create(PipelineUsage flags)
    {
        var toReturn = new DataFlowPipelineContext<T>();

        //context has a fixed destination so we cannot allow any alternate destinations to sneak in
        if (flags.HasFlag(PipelineUsage.FixedDestination))
        {
            toReturn.MustHaveDestination = null;
            toReturn.CannotHave.Add(typeof(IDataFlowDestination<T>));
        }
        else
        {
            toReturn.MustHaveDestination =
                typeof(IDataFlowDestination<T>); //context does not have a fixed destination so the pipeline configuration must specify the destination itself
        }

        if (flags.HasFlag(PipelineUsage.FixedSource))
        {
            toReturn.MustHaveSource = null;
            toReturn.CannotHave.Add(typeof(IDataFlowSource<T>));
        }
        else
        {
            toReturn.MustHaveSource =
                typeof(IDataFlowSource<T>); //context does not have a fixed source so the pipeline configuration must specify the source itself
        }

        if (!flags.HasFlag(PipelineUsage.LoadsSingleTableInfo))
            toReturn.CannotHave.Add(typeof(IPipelineRequirement<TableInfo>));

        if (!flags.HasFlag(PipelineUsage.LogsToTableLoadInfo))
            toReturn.CannotHave.Add(typeof(IPipelineRequirement<TableLoadInfo>));

        if (flags.HasFlag(PipelineUsage.LoadsSingleFlatFile))
            toReturn.MustHaveSource = typeof(IPipelineRequirement<FlatFileToLoad>);

        return toReturn;
    }
}
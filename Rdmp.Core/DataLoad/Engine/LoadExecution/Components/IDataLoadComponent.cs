// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

/// <summary>
///     A discrete step in DLE execution either configured by the user (ProcessTask=>RuntimeTask) or a fixed step e.g.
///     MigrateRAWTableToStaging.  See
///     DataLoadEngine.cd for how all the various components interact in the larger scheme of the DLE IDataLoadExecution.
/// </summary>
public interface IDataLoadComponent : IDisposeAfterDataLoad
{
    bool SkipComponent { get; }
    string Description { get; }
    ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken);
}
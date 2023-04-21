// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

public abstract class DataLoadComponent : IDataLoadComponent
{
    public bool SkipComponent { get; set; }
    public string Description { get; set; }
        
    public abstract ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken);

    protected DataLoadComponent()
    {
        SkipComponent = false;
    }

    protected bool Skip(IDataLoadJob job)
    {
        if (!SkipComponent) return false;

        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "Skipped load component: " + Description));
        return true;
    }


    public virtual void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
    }
}
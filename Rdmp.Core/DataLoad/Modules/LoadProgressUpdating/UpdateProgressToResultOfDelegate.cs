// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.DataLoad.Engine.Job.Scheduling;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.LoadProgressUpdating;

/// <summary>
///     UpdateProgressIfLoadsuccessful which executes an arbitrary Func in order to determine what date to update the
///     LoadProgress.DataLoadProgress to
///     (See UpdateProgressIfLoadsuccessful).
/// </summary>
public class UpdateProgressToResultOfDelegate : UpdateProgressIfLoadsuccessful
{
    private readonly Func<DateTime> _delegateToRun;

    public UpdateProgressToResultOfDelegate(ScheduledDataLoadJob job, Func<DateTime> delegateToRun) : base(job)
    {
        _delegateToRun = delegateToRun;
        DateToSetProgressTo = DateTime.MinValue;
    }

    public override void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        if (exitCode == ExitCodeType.Success)
            DateToSetProgressTo = _delegateToRun();

        base.LoadCompletedSoDispose(exitCode, postLoadEventListener);
    }
}
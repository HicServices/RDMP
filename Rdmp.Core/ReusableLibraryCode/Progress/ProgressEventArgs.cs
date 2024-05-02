// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     Event args for IDataLoadEventListener.OnProgress events.  Includes the task description of what is trying to be
///     achieved e.g. 'Load RAW database table
///     bob' along with the time spent performing the activity and how far through it we are (ProgressMeasurement).
///     <para>You can use a Stopwatch with Start and Stop to compute TimeSpentProcessingSoFar</para>
///     <para>
///         TaskDescription should be the same for all progress messages towards a goal.  For example do not send a message
///         OnProgress 'loaded 30 records to bob' and
///         another 'loaded 50 records to bob' instead just send OnProgress 'loading records to bob' and store the count in
///         the ProgressMeasurement field.
///     </para>
/// </summary>
public class ProgressEventArgs
{
    public string TaskDescription { get; set; }
    public ProgressMeasurement Progress { get; set; }
    public TimeSpan TimeSpentProcessingSoFar { get; set; }

    public bool Handled { get; set; }

    public ProgressEventArgs(string taskDescription, ProgressMeasurement progress, TimeSpan timeSpentProcessingSoFar)
    {
        TaskDescription = taskDescription;
        Progress = progress;
        TimeSpentProcessingSoFar = timeSpentProcessingSoFar;

        Handled = false;
    }
}

public delegate void ProgressEventHandler(object sender, ProgressEventArgs args);
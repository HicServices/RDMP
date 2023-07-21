// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     Event handler for progress updates and one off notifications.  This can include errors (ProgressEventType.Error),
///     warnings and information.  Progress
///     events are incremental messages in which a numerical count increases (possibly to a known maximum) e.g. 'loaded 300
///     records out of 2000'.
///     <para>It is valid to respond to OnNotify with ProgressEventType.Error (or even Warning) by throwing an Exception.</para>
/// </summary>
public interface IDataLoadEventListener
{
    void OnNotify(object sender, NotifyEventArgs e);
    void OnProgress(object sender, ProgressEventArgs e);
}
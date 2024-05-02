// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.ReusableLibraryCode.Checks;

/// <summary>
///     ICheckable where the Check method simply replays an existing list of CheckEventArgs stored in a
///     ToMemoryCheckNotifier.  The use case for this is when
///     you want to store the results of checking an ICheckable then replay it later (possibly multiple times) e.g. into a
///     UI component.
/// </summary>
public class ReplayCheckable : ICheckable
{
    private readonly ToMemoryCheckNotifier _toReplay;

    public ReplayCheckable(ToMemoryCheckNotifier toReplay)
    {
        _toReplay = toReplay;
    }

    public void Check(ICheckNotifier notifier)
    {
        foreach (var msg in _toReplay.Messages)
        {
            //don't propose fixes since this is a replay the time for applying the fix has long expired.
            msg.ProposedFix = null;

            notifier.OnCheckPerformed(msg);
        }
    }
}
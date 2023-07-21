// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.ReusableLibraryCode.Progress;

/// <summary>
///     Allows an IDataLoadEventListener to respond to check events like an ICheckNotifier.  All OnCheckPerformed events
///     raised are converted to OnNotify events
///     and passed to the IDataLoadEventListener.  Since IDataLoadEventListeners cannot respond to ProposedFixes this class
///     will always return false for
///     OnCheckPerformed (no fixes will ever be accepted).
/// </summary>
public class FromDataLoadEventListenerToCheckNotifier : ICheckNotifier
{
    private readonly IDataLoadEventListener _listener;

    public FromDataLoadEventListenerToCheckNotifier(IDataLoadEventListener listener)
    {
        _listener = listener;
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        _listener.OnNotify(this, args.ToNotifyEventArgs());

        //reject all proposed fixes
        return false;
    }
}
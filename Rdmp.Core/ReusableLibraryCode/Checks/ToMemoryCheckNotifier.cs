// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Threading;

namespace Rdmp.Core.ReusableLibraryCode.Checks;

/// <summary>
/// ICheckNotifier which records all CheckEventArgs received into a public List for later evaluation.  Primarily for use in testing to check for specific
/// messages.  Can also be used with a ReplayCheckable in order to check a component (or multiple components) and then at a later time replay the events into
/// a UI.
/// </summary>
public class ToMemoryCheckNotifier : ICheckNotifier
{
    private readonly ICheckNotifier _childToPassEventsTo;
    public List<CheckEventArgs> Messages { get; } = new();

    private readonly Lock _lockList = new();

    private CheckResult _worst = CheckResult.Success;

    /// <summary>
    /// Sometimes you want to know what the messages and the worst event encountered were but you still want to pass the messages to a third party e.g.
    /// checksUI, use this constructor to record all messages in memory as they go through but also let the supplied child check notifier actually handle
    /// those events and pass back the bool that child supplies for proposed fixes
    /// </summary>
    /// <param name="childToPassEventsTo"></param>
    public ToMemoryCheckNotifier(ICheckNotifier childToPassEventsTo)
    {
        _childToPassEventsTo = childToPassEventsTo;
    }

    public ToMemoryCheckNotifier()
    {
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        var fix = _childToPassEventsTo?.OnCheckPerformed(args) == true;

        //if child accepted the fix that reduces a Fail to a Warning
        if (fix && !string.IsNullOrWhiteSpace(args.ProposedFix) && args.Result == CheckResult.Fail)
            args.Result = CheckResult.Warning;

        if (args.Result > _worst)
            _worst = args.Result;

        lock (_lockList) Messages.Add(args);

        return fix;
    }

    public CheckResult GetWorst() => _worst;
}
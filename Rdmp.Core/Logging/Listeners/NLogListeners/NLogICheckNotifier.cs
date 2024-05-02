// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NLog;
using Rdmp.Core.Logging.Listeners.Extensions;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Logging.Listeners.NLogListeners;

/// <summary>
///     <see cref="ICheckNotifier" /> that passes all events to an <see cref="NLog.LogManager" />.  Optionally throws on
///     Errors (after logging).
/// </summary>
public class NLogICheckNotifier : NLogListener, ICheckNotifier
{
    public bool AcceptFixes { get; set; }

    public NLogICheckNotifier(bool acceptFixes, bool throwOnError) : base(throwOnError)
    {
        AcceptFixes = acceptFixes;
    }

    public bool OnCheckPerformed(CheckEventArgs args)
    {
        var level = args.ToLogLevel();

        if (args.ProposedFix != null && AcceptFixes)
            //downgrade it to warning if we are accepting the fix
            if (level > LogLevel.Warn)
                level = LogLevel.Warn;

        Log("Checks", level, args.Ex, args.Message);

        return AcceptFixes;
    }
}
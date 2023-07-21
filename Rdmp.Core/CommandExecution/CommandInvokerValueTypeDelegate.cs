// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     <see cref="CommandInvokerDelegate" /> that handles any value type except <see cref="Enum" /> (in
///     <see cref="CommandInvokerDelegate.Run" />).
/// </summary>
internal class CommandInvokerValueTypeDelegate : CommandInvokerDelegate
{
    /// <inheritdoc />
    public CommandInvokerValueTypeDelegate(Func<RequiredArgument, object> run) : base(typeof(object), false, run)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(Type t)
    {
        return t.IsValueType && !typeof(Enum).IsAssignableFrom(t);
    }
}
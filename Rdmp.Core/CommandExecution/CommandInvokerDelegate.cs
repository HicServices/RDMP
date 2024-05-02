// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     A function which can be run to fetch instances of a given <see cref="Type" /> when required at runtime by a
///     <see cref="CommandInvoker" />
/// </summary>
public class CommandInvokerDelegate
{
    /// <summary>
    ///     True if the delegate automatically supplies the value without any user input e.g.
    ///     <see cref="IBasicActivateItems" />
    /// </summary>
    public bool IsAuto { get; }

    /// <summary>
    ///     The base class for which the delegate handles locating instances of e.g. <see cref="IDeleteable" />
    /// </summary>
    protected readonly Type HandledType;

    /// <summary>
    ///     The method to run when it is time to pick an object for the give <see cref="RequiredArgument" />
    /// </summary>
    public Func<RequiredArgument, object> Run { get; }

    /// <summary>
    ///     Set to true to require <see cref="HandledType" /> to exactly match candidates.  False to identify
    ///     compatible objects using <see cref="Type.IsAssignableFrom(Type?)" />.  Defaults to false.
    /// </summary>
    public bool RequireExactMatch { get; internal init; }

    /// <summary>
    ///     Defines a new <see cref="Type" /> which we know how to get instances at runtime to fulfil
    /// </summary>
    /// <param name="handledType"></param>
    /// <param name="isAuto"></param>
    /// <param name="run">The function to run when values are required of the <paramref name="handledType" /> during runtime</param>
    public CommandInvokerDelegate(Type handledType, bool isAuto, Func<RequiredArgument, object> run)
    {
        IsAuto = isAuto;
        Run = run;
        HandledType = handledType;
    }

    /// <summary>
    ///     Returns true if the delegate <see cref="Run" /> function can return valid objects of the passed <see cref="Type" />
    /// </summary>
    /// <param name="t">The type of object you need</param>
    /// <returns></returns>
    public virtual bool CanHandle(Type t)
    {
        return HandledType == t || (!RequireExactMatch && HandledType.IsAssignableTo(t));
    }
}
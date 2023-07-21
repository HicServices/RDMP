// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution;

/// <summary>
///     <see cref="CommandInvokerDelegate" /> which handles arrays.  Delegate function <see cref="CanHandle" /> any array
///     that is assignable
///     to the element type the delegate is set up for (e.g. we could service a request for <see cref="Catalogue" />[] when
///     set up for
///     <see cref="IMapsDirectlyToDatabaseTable" /> elements.
/// </summary>
internal class CommandInvokerArrayDelegate : CommandInvokerDelegate
{
    /// <summary>
    ///     Declares a new delegate method which returns specific implementations of a base array type
    /// </summary>
    /// <param name="element">
    ///     The base element type of your array e.g. <see cref="IMapsDirectlyToDatabaseTable" />(should not
    ///     be an array itself)
    /// </param>
    /// <param name="isAuto">True if the <paramref name="run" /> never requires user input</param>
    /// <param name="run">Method to invoke to fetch an instance when needed</param>
    public CommandInvokerArrayDelegate(Type element, bool isAuto, Func<RequiredArgument, object> run) : base(element,
        isAuto, run)
    {
    }

    /// <inheritdoc />
    public override bool CanHandle(Type t)
    {
        return t.IsArray && HandledType.IsAssignableFrom(t.GetElementType());
    }
}
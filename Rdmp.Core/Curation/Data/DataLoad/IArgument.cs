// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Records the user configured value of a property marked with [DemandsInitialization] declared on a data flow/dle
///     component (including plugin components).
///     See Argument for full description.
/// </summary>
public interface IArgument : IMapsDirectlyToDatabaseTable, ISaveable
{
    /// <summary>
    ///     The name of the Property which this object stores the value of.  The Property should be decorated with
    ///     [DemandsInitialization]
    /// </summary>
    string Name { get; set; }

    /// <summary>
    ///     Record of <see cref="DemandsInitializationAttribute.Description" /> as it was specified when the
    ///     <see cref="IArgument" /> was created
    /// </summary>
    string Description { get; set; }

    /// <summary>
    ///     The string value for populating the class Property at runtime.  This is usually set to null or a default then
    ///     adjusted by the user as needed to tailor
    ///     the <see cref="IArgumentHost" /> class.
    /// </summary>
    string Value { get; }

    /// <summary>
    ///     The full Type name of the class Property this <see cref="IArgument" /> holds the runtime value for (See
    ///     <see cref="IArgumentHost" />)
    /// </summary>
    string Type { get; }

    /// <summary>
    ///     Change the current <see cref="Value" /> held by the <see cref="IArgument" /> to a new value (this must be a
    ///     supported Type - See <see cref="Argument.PermissableTypes" />)
    /// </summary>
    /// <param name="o"></param>
    void SetValue(object o);

    /// <summary>
    ///     Parses the current <see cref="Value" /> into the <see cref="IArgument.Type" /> and returns it as a strongly typed
    ///     object
    /// </summary>
    /// <returns></returns>
    object GetValueAsSystemType();

    /// <summary>
    ///     Parses the current <see cref="Type" /> string into a <see cref="System.Type" />
    /// </summary>
    /// <returns></returns>
    Type GetSystemType();

    /// <summary>
    ///     Similar to <see cref="GetSystemType" /> except it will look for a non interface/abstract derrived class e.g. if
    ///     <see cref="Type" /> is <see cref="ICatalogue" />
    ///     it will return <see cref="Catalogue" />
    /// </summary>
    /// <returns></returns>
    Type GetConcreteSystemType();

    /// <summary>
    ///     Change the <see cref="Type" /> of the <see cref="IArgument" /> to the supplied <see cref="System.Type" />.  This
    ///     may make <see cref="Value" /> invalid.
    /// </summary>
    /// <param name="t"></param>
    void SetType(Type t);
}
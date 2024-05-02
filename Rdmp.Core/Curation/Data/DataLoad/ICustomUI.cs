// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
///     Powers the creation of instances of T in an ICustomUI
/// </summary>
public interface ICustomUI<in T> : ICustomUI where T : ICustomUIDrivenClass
{
    /// <summary>
    ///     Loads the current value into the user interface
    /// </summary>
    /// <param name="value"></param>
    void SetUnderlyingObjectTo(T value);
}

/// <summary>
///     Interface that lets you create UIs for populating <see cref="IArgument" /> values for Properties which are too
///     complicated to do with basic Types.  See <see cref="ICustomUIDrivenClass" />.  If
///     If at all possible you should avoid the overhead of this system and instead use [DemandsNestedInitialization] and
///     subclasses if you have a particularly complex concept
///     defined in your plugin component.
/// </summary>
public interface ICustomUI
{
    /// <summary>
    ///     Use this to fetch objects from the RDMP platform databases e.g. <see cref="Catalogue" />, <see cref="TableInfo" />
    ///     etc
    /// </summary>
    ICatalogueRepository CatalogueRepository { get; set; }

    /// <summary>
    ///     When implementing this just cast value to T and call the overload in ICustomUI&lt;T&gt;
    /// </summary>
    /// <param name="value"></param>
    void SetGenericUnderlyingObjectTo(ICustomUIDrivenClass value);

    /// <summary>
    ///     Fetches the final state of the object being show in the user interface (e.g. after closing the form)
    /// </summary>
    /// <returns></returns>
    ICustomUIDrivenClass GetFinalStateOfUnderlyingObject();
}
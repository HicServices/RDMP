// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Providers.Nodes.UsedByNodes;

/// <summary>
///     <see cref="Node" /> for relationship/link type objects (e.g. a <see cref="Catalogue" /> linked under a
///     <see cref="LoadMetadata" />).  The
///     node should behave like the <see cref="ObjectBeingUsed" /> but should not break equality / considered the go to
///     location for that object.
/// </summary>
/// <typeparam name="T">The Type of the parent <see cref="User" /></typeparam>
/// <typeparam name="T2">The type of <see cref="ObjectBeingUsed" /> by the parent</typeparam>
public class ObjectUsedByOtherObjectNode<T, T2> : Node, IObjectUsedByOtherObjectNode<T, T2>
    where T : class
    where T2 : class
{
    /// <summary>
    ///     The string representation of the <see cref="ObjectUsedByOtherObjectNode{T,T2}" /> when it
    ///     <see cref="IsEmptyNode" />
    /// </summary>
    public const string EmptyRepresentation = "???";

    /// <summary>
    ///     The parent object which uses another object
    /// </summary>
    public T User { get; }

    /// <summary>
    ///     The object being used by the <see cref="User" />
    /// </summary>
    public T2 ObjectBeingUsed { get; }

    /// <summary>
    ///     True if <see cref="ObjectBeingUsed" /> is null
    /// </summary>
    public bool IsEmptyNode => ObjectBeingUsed == null;

    /// <summary>
    ///     Creates a new instance describing the object and user
    /// </summary>
    /// <param name="user"></param>
    /// <param name="objectBeingUsed"></param>
    public ObjectUsedByOtherObjectNode(T user, T2 objectBeingUsed)
    {
        User = user;
        ObjectBeingUsed = objectBeingUsed;
    }

    /// <summary>
    ///     Returns the <see cref="ObjectBeingUsed" />
    /// </summary>
    /// <returns></returns>
    public object MasqueradingAs()
    {
        return ObjectBeingUsed;
    }

    /// <summary>
    ///     Returns the string representation of <see cref="ObjectBeingUsed" /> or <see cref="EmptyRepresentation" /> if
    ///     <see cref="IsEmptyNode" />
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return IsEmptyNode ? EmptyRepresentation : ObjectBeingUsed.ToString();
    }

    #region Equality

    /// <summary>
    ///     Equality based on <see cref="User" /> and <see cref="ObjectBeingUsed" />
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    protected bool Equals(ObjectUsedByOtherObjectNode<T, T2> other)
    {
        return EqualityComparer<T>.Default.Equals(User, other.User) &&
               EqualityComparer<T2>.Default.Equals(ObjectBeingUsed, other.ObjectBeingUsed);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ObjectUsedByOtherObjectNode<T, T2>)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(User, ObjectBeingUsed);
    }

    #endregion
}
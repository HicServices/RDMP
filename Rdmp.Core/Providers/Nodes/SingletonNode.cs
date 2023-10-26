// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Equ;

namespace Rdmp.Core.Providers.Nodes;

/// <summary>
/// <see cref="Node"/> of which there can only ever be one in the RDMP object hierarchy e.g. <see cref="AllCohortsNode"/>.  By convention
/// these classes should normally start with the prefix "All"
/// </summary>
public abstract class SingletonNode : Node, IEquatable<SingletonNode>
{
    private readonly string _caption;

    protected SingletonNode(string caption)
    {
        _caption = caption;
    }

    public override string ToString() => _caption;

    public bool Equals(SingletonNode other) => string.Equals(_caption, other._caption);

    public override bool Equals(object obj) => obj is SingletonNode s &&
                                               MemberwiseEqualityComparer<SingletonNode>.ByProperties.Equals(this, s);

    public override int GetHashCode() => _caption.GetHashCode();
}
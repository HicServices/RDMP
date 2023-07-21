// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     Interface for additional join column pairs required by an IJoin.  This is only applicable if you need to join two
///     tables using multiple columns at once.  E.g. A left join B
///     on A.x = B.x and A.y=B.y.  ISupplementalJoin is assumed to follow the same direction as the principal IJoin.
/// </summary>
public interface ISupplementalJoin
{
    /// <inheritdoc cref="IJoin.ForeignKey" />
    ColumnInfo ForeignKey { get; }

    /// <inheritdoc cref="IJoin.PrimaryKey" />
    ColumnInfo PrimaryKey { get; }

    /// <inheritdoc cref="IJoin.Collation" />
    string Collation { get; }
}
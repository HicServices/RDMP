// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Curation.Data;

namespace Rdmp.Core;

/// <summary>
///     Thrown when an object which should be associated with a single <see cref="ColumnInfo" /> cannot be resolved to one
///     (e.g. an <see cref="ExtractionInformation" /> which
///     has become unlinked with an underlying column).
/// </summary>
public class MissingColumnInfoException : Exception
{
    public MissingColumnInfoException(string message) : base(message)
    {
    }

    public MissingColumnInfoException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
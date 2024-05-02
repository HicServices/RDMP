// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Rdmp.Core.DataLoad.Modules.Exceptions;

/// <summary>
///     Thrown by flat file reading components (e.g. DelimitedFlatFileDataFlowSource) when there is a structural problem
///     with the file (e.g. 3 headers and 3 cells
///     per line but suddenly 4 cells appear on line 30).
/// </summary>
public class FlatFileLoadException : Exception
{
    public FlatFileLoadException(string message)
        : base(message)
    {
    }

    public FlatFileLoadException(string message, Exception e)
        : base(message, e)
    {
    }
}
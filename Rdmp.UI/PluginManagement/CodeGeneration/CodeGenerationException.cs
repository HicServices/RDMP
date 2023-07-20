// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.PluginManagement.CodeGeneration;

/// <summary>
/// Thrown when there is a problem a database table that means <see cref="MapsDirectlyToDatabaseTableClassCodeGenerator"/> can not generate
/// code for a <see cref="IMapsDirectlyToDatabaseTable"/> compatible with it (e.g. it doesn't have an ID column).
/// </summary>
public class CodeGenerationException : Exception
{
    public CodeGenerationException(string message) : base(message)
    {

    }
}
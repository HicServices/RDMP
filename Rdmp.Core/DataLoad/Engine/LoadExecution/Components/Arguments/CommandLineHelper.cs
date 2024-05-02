// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;

/// <summary>
///     Helper class for assembling a command line parameters strings for an .exe.  This will wrap with quotes when there
///     is whitespace etc.  Class can
///     be used when you have generic key value pairs you want to send to an exe as startup parameters.
/// </summary>
public class CommandLineHelper
{
    public static string CreateArgString(string name, object value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The argument 'name' parameter is empty");

        if (!char.IsUpper(name, 0))
            throw new ArgumentException(
                $"The name argument should be in Pascal case, the first character in {name} should be uppercase");

        if (value is LoadDirectory loadDirectory)
            value = loadDirectory.RootPath.FullName;

        return value switch
        {
            null => throw new ArgumentException("The argument value is null"),
            bool => Convert.ToBoolean(value) ? $"-{ConvertArgNameToString(name)}" : "",
            LoadStage => CreateArgString(name, value.ToString()),
            DiscoveredDatabase dbInfo =>
                $"{CreateArgString("DatabaseName", dbInfo.GetRuntimeName())} {CreateArgString("DatabaseServer", dbInfo.Server.Name)}",
            _ => $"-{ConvertArgNameToString(name)}={GetValueString(value)}"
        };
    }

    public static string ConvertArgNameToString(string name)
    {
        // Will split on capitals without breaking up capital sequences
        // e.g. 'TestArg' => 'test-arg' and 'TestTLAArg' => 'test-tla-arg'
        return Regex.Replace(name, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @"-$1").ToLower();
    }

    public static string GetValueString(object value)
    {
        return value switch
        {
            string s => s.Contains(' ')
                ? $@"""{value}"""
                : //<- looks like a snake (or a golf club? GM)
                s,
            DateTime dt =>
                $"\"{(dt.TimeOfDay.TotalSeconds.Equals(0) ? dt.ToString("yyyy-MM-dd") : dt.ToString("yyyy-MM-dd HH:mm:ss"))}\"",
            FileInfo fi => $"\"{fi.FullName}\"",
            _ => throw new ArgumentException(
                $"Cannot create a value string from an object of type {value.GetType().FullName}")
        };
    }
}
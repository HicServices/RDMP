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
/// Helper class for assembling a command line parameters strings for an .exe.  This will wrap with quotes when there is whitespace etc.  Class can
/// be used when you have generic key value pairs you want to send to an exe as startup parameters.
/// </summary>
public class CommandLineHelper
{
    public static string CreateArgString(string name, object value)
    {
        if (value is LoadDirectory directory)
            value = directory.RootPath.FullName;

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("The argument 'name' parameter is empty");

        if (!char.IsUpper(name, 0))
            throw new ArgumentException(
                $"The name argument should be in Pascal case, the first character in {name} should be uppercase");

        if (value == null)
            throw new ArgumentException("The argument value is null");

        if (value is bool)
            if (Convert.ToBoolean(value))
                return $"-{ConvertArgNameToString(name)}";
            else
                return "";

        if (value is LoadStage)
            return CreateArgString(name, value.ToString());

        if (value is DiscoveredDatabase dbInfo)
            return
                $"{CreateArgString("DatabaseName", dbInfo.GetRuntimeName())} {CreateArgString("DatabaseServer", dbInfo.Server.Name)}";

        return $"-{ConvertArgNameToString(name)}={GetValueString(value)}";
    }

    public static string ConvertArgNameToString(string name) =>
        // Will split on capitals without breaking up capital sequences
        // e.g. 'TestArg' => 'test-arg' and 'TestTLAArg' => 'test-tla-arg'
        Regex.Replace(name, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @"-$1").ToLower();

    public static string GetValueString(object value)
    {
        if (value is string)
            if (value.ToString().Contains(' '))
                return $@"""{value}""";//<- looks like a snake (or a golf club? GM)
            else
                return s;

        if (value is DateTime dt)
            return
                $"\"{(dt.TimeOfDay.TotalSeconds.Equals(0) ? dt.ToString("yyyy-MM-dd") : dt.ToString("yyyy-MM-dd HH:mm:ss"))}\"";

        if (value is FileInfo fi) return $"\"{fi.FullName}\"";

        throw new ArgumentException($"Cannot create a value string from an object of type {value.GetType().FullName}");
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using System.Text;
using FAnsi.Discovery;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.UI.PluginManagement.CodeGeneration;

/// <summary>
/// Generates template code for a <see cref="IMapsDirectlyToDatabaseTable"/> implementation class which
/// models the data in the table (like a budget version of entity framework).
/// </summary>
public class MapsDirectlyToDatabaseTableClassCodeGenerator
{
    private readonly DiscoveredTable _table;

    public MapsDirectlyToDatabaseTableClassCodeGenerator(DiscoveredTable table)
    {
        _table = table;
    }

    public string GetCode()
    {
        var columns = _table.DiscoverColumns();

        if (!columns.Any(c => c.GetRuntimeName().Equals("ID")))
            throw new CodeGenerationException(
                "Table must have an ID autonum column to become an IMapsDirectlyToDatabaseTable class");

        var classStart = new StringBuilder();

        classStart.Append($"public class {_table.GetRuntimeName()}: DatabaseEntity");

        var isINamed = columns.Any(c => c.GetRuntimeName() == "Name");
        if (isINamed)
            classStart.Append(",INamed");

        classStart.AppendLine();
        classStart.AppendLine("{");

        var databaseFields = new StringBuilder();
        databaseFields.AppendLine("\t#region Database Properties");
        databaseFields.AppendLine();

        var databaseProperties = new StringBuilder();

        var constructors = new StringBuilder();

        constructors.AppendLine(
            $"\tpublic {_table.GetRuntimeName()}(IRepository repository/*, TODO Required Construction Properties For NEW*/)");
        constructors.AppendLine(@"  {
        repository.InsertAndHydrate(this,new Dictionary<string, object>()
        {
            //TODO Any parameters here as key value pairs
        });

        if (ID == 0 ||  !repository.Equals(Repository))
            throw new ArgumentException(""Repository failed to properly hydrate this class"");
    }");


        constructors.AppendLine(
            $"\tpublic {_table.GetRuntimeName()}(IRepository repository, DbDataReader r): base(repository, r)");
        constructors.AppendLine("\t{");

        foreach (var col in columns.Where(c => c.GetRuntimeName() != "ID"))
        {
            var type = GetCSharpTypeFor(col, out var setCode);
            var propertyName = col.GetRuntimeName();
            var fieldString = col.GetRuntimeName();

            //camel case it
            fieldString = $"_{fieldString[..1].ToLower()}{fieldString[1..]}";

            databaseFields.AppendLine($"\tprivate {type} {fieldString};");

            databaseProperties.AppendLine($"\tpublic {type} {propertyName}");
            databaseProperties.AppendLine("\t{");
            databaseProperties.AppendLine($"\t\tget {{ return {fieldString};}}");
            databaseProperties.AppendLine($"\t\tset {{ SetField(ref {fieldString}, value);}}");
            databaseProperties.AppendLine("\t}");

            constructors.AppendLine($"\t\t{propertyName} = {setCode}");
        }

        databaseFields.AppendLine("\t#endregion");
        databaseFields.AppendLine();

        constructors.AppendLine("\t}");

        if (isINamed)
            constructors.AppendLine("\t" + @"public override string ToString()
    {
        return Name;
    }");

        return $"{classStart}{databaseFields}{databaseProperties}{constructors}}}";
    }

    private static string GetCSharpTypeFor(DiscoveredColumn col, out string setCode)
    {
        var r = $"r[\"{col.GetRuntimeName()}\"]";

        if (col.DataType.GetLengthIfString() != -1)
        {
            setCode = col.AllowNulls ? $"{r} as string;" : $"{r}.ToString();";
            return "string";
        }

        if (col.DataType.SQLType.Contains("date"))
            if (col.AllowNulls)
            {
                setCode = $"ObjectToNullableDateTime({r});";
                return "DateTime?";
            }
            else
            {
                setCode = $"Convert.ToDateTime({r});";
                return "DateTime";
            }

        if (col.DataType.SQLType.Contains("int"))
            if (col.AllowNulls)
            {
                setCode = $"ObjectToNullableInt({r});";
                return "int?";
            }
            else
            {
                setCode = $"Convert.ToInt32({r});";
                return "int";
            }

        if (col.DataType.SQLType.Contains("bit"))
            if (col.AllowNulls)
            {
                setCode = $"ObjectToNullableBool({r});//TODO: Confirm you actually mean true/false/null?";
                return "bool?";
            }
            else
            {
                setCode = $"Convert.ToBoolean({r});";
                return "bool";
            }


        setCode = "TODO Unrecognised Type";
        return "TODO  Unrecognised Type";
    }
}
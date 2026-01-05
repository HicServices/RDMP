// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///  Use this class to create standard parameters which you will always manually add in code to a QueryBuilder.  These are not editable
///  by users and are not stored in a database.  They should be used for things such as cohortDefinitionID, projectID etc.
/// </summary>
public class ConstantParameter : ISqlParameter
{
    private readonly IQuerySyntaxHelper _syntaxHelper;

    /// <summary>
    /// Creates a new unchangeable always available parameter in a query being built.
    /// </summary>
    /// <param name="parameterSQL">The declaration sql e.g. DECLARE @bob as int</param>
    /// <param name="value">The value to set the parameter e.g. 1</param>
    /// <param name="comment">Some text to appear above the parameter, explaining its purpose</param>
    /// <param name="syntaxHelper"></param>
    public ConstantParameter(string parameterSQL, string value, string comment, IQuerySyntaxHelper syntaxHelper)
    {
        _syntaxHelper = syntaxHelper;
        Value = value;
        Comment = comment;
        ParameterSQL = parameterSQL;
    }

    /// <summary>
    /// Not supported for constant parameters
    /// </summary>
    public void SaveToDatabase()
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override string ToString() => $"{ParameterName} = {Value}";

    /// <summary>
    /// Checks the syntax of the parameter (See <see cref="ParameterSyntaxChecker"/>)
    /// </summary>
    /// <param name="notifier"></param>
    public void Check(ICheckNotifier notifier)
    {
        new ParameterSyntaxChecker(this).Check(notifier);
    }

    /// <inheritdoc/>
    public IQuerySyntaxHelper GetQuerySyntaxHelper() => _syntaxHelper;

    /// <inheritdoc/>
    public string ParameterName => QuerySyntaxHelper.GetParameterNameFromDeclarationSQL(ParameterSQL);

    /// <inheritdoc/>
    [Sql]
    public string ParameterSQL { get; set; }

    /// <inheritdoc/>
    [Sql]
    public string Value { get; set; }

    /// <inheritdoc/>
    public string Comment { get; set; }

    /// <summary>
    /// Returns null, <see cref="ConstantParameter"/> are never owned by any objects
    /// </summary>
    /// <returns></returns>
    public IMapsDirectlyToDatabaseTable GetOwnerIfAny() => null;

    /// <summary>
    /// Attempts to parse the provided <paramref name="sql"/> text into a <see cref="ConstantParameter"/>
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="syntaxHelper"></param>
    /// <returns></returns>
    public static ConstantParameter Parse(string sql, IQuerySyntaxHelper syntaxHelper)
    {
        var lines = sql.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        string comment = null;

        var commentRegex = new Regex(@"/\*(.*)\*/");
        var matchComment = commentRegex.Match(lines[0]);
        if (lines.Length >= 3 && matchComment.Success)
            comment = matchComment.Groups[1].Value;

        var declaration = comment == null ? lines[0] : lines[1];
        declaration = declaration.TrimEnd(new[] { '\r' });

        var valueLine = comment == null ? lines[1] : lines[2];

        if (!valueLine.StartsWith("SET"))
            throw new Exception($"Value line did not start with SET:{sql}");

        var valueLineSplit = valueLine.Split(new[] { '=' });
        var value = valueLineSplit[1].TrimEnd(new[] { ';', '\r' });

        return new ConstantParameter(declaration.Trim(), value.Trim(), comment, syntaxHelper);
    }
}
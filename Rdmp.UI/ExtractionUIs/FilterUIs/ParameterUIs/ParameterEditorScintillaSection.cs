// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.FilterImporting;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.UI.ExtractionUIs.FilterUIs.ParameterUIs;

/// <summary>
///     Models a <see cref="ISqlParameter" /> being edited in a <see cref="ParameterEditorScintillaControlUI" />.  Includes
///     the location whether it
///     should be editable etc.  Also handles reconciling user edits to the SQL into the <see cref="ISqlParameter" /> (if
///     edit is legal).
/// </summary>
public class ParameterEditorScintillaSection
{
    private readonly ParameterRefactorer _refactorer;
    private readonly IQuerySyntaxHelper _querySyntaxHelper;

    public ParameterEditorScintillaSection(ParameterRefactorer refactorer, int lineStart, int lineEnd,
        ISqlParameter parameter, bool editable, string originalText)
    {
        _refactorer = refactorer;
        LineStart = lineStart;
        LineEnd = lineEnd;
        Parameter = parameter;
        Editable = editable;
        _querySyntaxHelper = parameter.GetQuerySyntaxHelper();

        var prototype = ConstantParameter.Parse(originalText, _querySyntaxHelper);
        if (prototype.Value != parameter.Value)
            throw new ArgumentException(
                $"Parameter {parameter} was inconsistent with the SQL passed to us based on QueryBuilder.DeconstructStringIntoParameter, they had different Values");

        if (prototype.ParameterSQL != parameter.ParameterSQL)
            throw new ArgumentException(
                $"Parameter {parameter} was inconsistent with the SQL passed to us based on QueryBuilder.DeconstructStringIntoParameter, they had different ParameterSQL");

        if (prototype.Comment != parameter.Comment)
            throw new ArgumentException(
                $"Parameter {parameter} was inconsistent with the SQL passed to us based on QueryBuilder.DeconstructStringIntoParameter, they had different Comment");
    }

    public int LineStart { get; }
    public int LineEnd { get; }

    public ISqlParameter Parameter { get; }
    public bool Editable { get; private set; }

    public bool IncludesLine(int lineNumber)
    {
        return lineNumber >= LineStart && lineNumber <= LineEnd;
    }

    public FreeTextParameterChangeResult CheckForChanges(string sql)
    {
        try
        {
            var oldName = Parameter.ParameterName;

            var newPrototype = ConstantParameter.Parse(sql, _querySyntaxHelper);

            if (string.Equals(newPrototype.Comment, Parameter.Comment) //can be null you see
                &&
                string.Equals(newPrototype.Value, Parameter.Value)
                &&
                newPrototype.ParameterSQL.Equals(Parameter.ParameterSQL))
                return FreeTextParameterChangeResult.NoChangeMade;

            Parameter.Comment = newPrototype.Comment;
            Parameter.Value = newPrototype.Value;
            Parameter.ParameterSQL = newPrototype.ParameterSQL;
            Parameter.SaveToDatabase();

            _refactorer.HandleRename(Parameter, oldName, Parameter.ParameterName);


            return FreeTextParameterChangeResult.ChangeAccepted;
        }
        catch (Exception)
        {
            return FreeTextParameterChangeResult.ChangeRejected;
        }
    }
}

public enum FreeTextParameterChangeResult
{
    NoChangeMade,
    ChangeAccepted,
    ChangeRejected
}
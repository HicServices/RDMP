// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.QueryBuilding.SyntaxChecking;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     The count(*) column in an AggregateConfiguration, this is used by AggregateBuilder.  This can be any aggregate
///     function such as 'sum', 'avg' etc.
/// </summary>
public class AggregateCountColumn : SpontaneousObject, IColumn
{
    private IQuerySyntaxHelper _syntaxHelper;
    private readonly string _sql;

    /// <summary>
    ///     The default alias for unamed count columns
    /// </summary>
    public const string DefaultAliasName = "MyCount";

    /// <summary>
    ///     Creates a new Aggregate Function (count / max etc) with the given line of SELECT SQL
    ///     <para>Can include aliases e.g. count(*) as MyCount</para>
    /// </summary>
    /// <param name="sql"></param>
    public AggregateCountColumn(string sql) : base(new MemoryRepository())
    {
        _sql = sql;
    }

    /// <summary>
    ///     Initializes the <see cref="IQuerySyntaxHelper" /> for the column and optionally ensures that it has an alias.  If
    ///     no <see cref="Alias" /> has
    ///     been specified or was found in the current sql then <see cref="DefaultAliasName" /> is set.
    /// </summary>
    /// <param name="syntaxHelper"></param>
    /// <param name="ensureAliasExists"></param>
    public void SetQuerySyntaxHelper(IQuerySyntaxHelper syntaxHelper, bool ensureAliasExists)
    {
        _syntaxHelper = syntaxHelper;

        //if alias exists
        if (_syntaxHelper.SplitLineIntoSelectSQLAndAlias(_sql, out var select, out var alias))
            Alias = alias; //use the users explicit alias
        else
            Alias = ensureAliasExists ? DefaultAliasName : null; //set an alias of MyCount

        SelectSQL = select;
    }

    /// <inheritdoc />
    public string GetRuntimeName()
    {
        return _syntaxHelper == null
            ? throw new Exception("SyntaxHelper is null, call SetQuerySyntaxHelper first")
            : string.IsNullOrWhiteSpace(Alias)
                ? _syntaxHelper.GetRuntimeName(SelectSQL)
                : Alias;
    }

    /// <summary>
    ///     Combines the <see cref="SelectSQL" /> with the <see cref="Alias" /> for use in SELECT Sql
    /// </summary>
    /// <returns></returns>
    public string GetFullSelectLineStringForSavingIntoAnAggregate()
    {
        return string.IsNullOrWhiteSpace(Alias)
            ? SelectSQL
            : SelectSQL + _syntaxHelper.AliasPrefix + Alias;
    }

    /// <inheritdoc />
    public ColumnInfo ColumnInfo => null;

    /// <inheritdoc />
    public int Order { get; set; }

    /// <inheritdoc />
    [Sql]
    public string SelectSQL { get; set; }

    /// <inheritdoc />
    public string Alias { get; private set; }

    /// <inheritdoc />
    public bool HashOnDataRelease => false;

    /// <inheritdoc />
    public bool IsExtractionIdentifier => false;

    /// <inheritdoc />
    public bool IsPrimaryKey => false;

    /// <inheritdoc />
    public void Check(ICheckNotifier notifier)
    {
        new ColumnSyntaxChecker(this).Check(notifier);
    }
}
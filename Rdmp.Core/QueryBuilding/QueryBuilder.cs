// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.QueryBuilding.Parameters;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.QueryBuilding;

/// <summary>
///     This class maintains a list of user defined ExtractionInformation objects.  It can produce SQL which will try to
///     extract this set of ExtractionInformation objects only from the database.  This includes determining which
///     ExtractionInformation
///     are Lookups, which tables the various objects come from, figuring out whether they can be joined by using JoinInfo
///     in the catalogue
///     <para>
///         It will throw when query SQL if it is not possible to join all the underlying tables or there are any other
///         problems.
///     </para>
///     <para>You can ask it what is on line X or ask what line number has ExtractionInformation Y on it</para>
///     <para>
///         ExtractionInformation is sorted by column order prior to generating the SQL (i.e. not the order you add them
///         to the query builder)
///     </para>
/// </summary>
public class QueryBuilder : ISqlQueryBuilder
{
    private readonly ITableInfo[] _forceJoinsToTheseTables;
    private readonly object oSQLLock = new();

    /// <inheritdoc />
    public string SQL
    {
        get
        {
            lock (oSQLLock)
            {
                if (SQLOutOfDate)
                    RegenerateSQL();
                return _sql;
            }
        }
    }

    /// <inheritdoc />
    public string LimitationSQL { get; private set; }

    /// <inheritdoc />
    public List<QueryTimeColumn> SelectColumns { get; }

    /// <inheritdoc />
    public List<ITableInfo> TablesUsedInQuery { get; private set; }

    /// <inheritdoc />
    public List<JoinInfo> JoinsUsedInQuery { get; private set; }

    /// <inheritdoc />
    public List<CustomLine> CustomLines { get; }

    /// <inheritdoc />
    public CustomLine TopXCustomLine { get; set; }

    /// <inheritdoc />
    public ParameterManager ParameterManager { get; }

    /// <summary>
    ///     Optional field, this specifies where to start gargantuan joins such as when there are 3+ joins and multiple primary
    ///     key tables e.g. in a star schema.
    ///     If this is not set and there are too many JoinInfos defined in the Catalogue then the class will bomb out with the
    ///     Exception
    /// </summary>
    public ITableInfo PrimaryExtractionTable { get; set; }

    /// <summary>
    ///     A container that contains all the subcontainers and filters to be assembled during the query (use a
    ///     SpontaneouslyInventedFilterContainer if you want to inject your
    ///     own container tree at runtime rather than referencing a database entity)
    /// </summary>
    public IContainer RootFilterContainer
    {
        get => _rootFilterContainer;
        set
        {
            _rootFilterContainer = value;
            SQLOutOfDate = true;
        }
    }

    /// <inheritdoc />
    public bool CheckSyntax { get; set; }


    private string _salt;

    /// <summary>
    ///     Only use this if you want IColumns which are marked as requiring Hashing to be hashed.  Once you set this on a
    ///     QueryEditor all fields so marked will be hashed using the
    ///     specified salt
    /// </summary>
    /// <param name="salt">A 3 letter string indicating the desired SALT</param>
    public void SetSalt(string salt)
    {
        if (string.IsNullOrWhiteSpace(salt))
            throw new NullReferenceException("Salt cannot be blank");

        _salt = salt;
    }

    public void SetLimitationSQL(string limitationSQL)
    {
        if (limitationSQL != null && limitationSQL.Contains("top"))
            throw new Exception("Use TopX property instead of limitation SQL to achieve this");

        LimitationSQL = limitationSQL;
        SQLOutOfDate = true;
    }

    /// <inheritdoc />
    public List<IFilter> Filters { get; private set; }

    /// <summary>
    ///     Limits the number of returned rows to the supplied maximum or -1 if there is no maximum
    /// </summary>
    public int TopX
    {
        get => _topX;
        set
        {
            //it already has that value
            if (_topX == value)
                return;

            _topX = value;
            SQLOutOfDate = true;
        }
    }

    private string _sql;

    /// <inheritdoc />
    public bool SQLOutOfDate { get; set; }

    private IContainer _rootFilterContainer;
    private readonly string _hashingAlgorithm;
    private int _topX;

    public IQuerySyntaxHelper QuerySyntaxHelper { get; set; }

    /// <summary>
    ///     Used to build extraction queries based on ExtractionInformation sets
    /// </summary>
    /// <param name="limitationSQL">Any text you want after SELECT to limit the results e.g. "DISTINCT" or "TOP 10"</param>
    /// <param name="hashingAlgorithm"></param>
    /// <param name="forceJoinsToTheseTables"></param>
    public QueryBuilder(string limitationSQL, string hashingAlgorithm, ITableInfo[] forceJoinsToTheseTables = null)
    {
        _forceJoinsToTheseTables = forceJoinsToTheseTables;
        SetLimitationSQL(limitationSQL);
        ParameterManager = new ParameterManager();
        CustomLines = new List<CustomLine>();

        CheckSyntax = true;
        SelectColumns = new List<QueryTimeColumn>();

        _hashingAlgorithm = hashingAlgorithm;

        TopX = -1;
    }

    /// <inheritdoc />
    public void AddColumnRange(IColumn[] columnsToAdd)
    {
        //add the new ones to the list
        foreach (var col in columnsToAdd)
            AddColumn(col);

        SQLOutOfDate = true;
    }

    /// <inheritdoc />
    public void AddColumn(IColumn col)
    {
        var toAdd = new QueryTimeColumn(col);

        //if it is new, add it to the list
        if (!SelectColumns.Contains(toAdd))
        {
            SelectColumns.Add(toAdd);
            SQLOutOfDate = true;
        }
    }

    /// <inheritdoc />
    public CustomLine AddCustomLine(string text, QueryComponent positionToInsert)
    {
        SQLOutOfDate = true;
        return SqlQueryBuilderHelper.AddCustomLine(this, text, positionToInsert);
    }

    /// <summary>
    ///     Updates .SQL Property, note that this is automatically called when you query .SQL anyway so you do not need to
    ///     manually call it.
    /// </summary>
    public void RegenerateSQL()
    {
        var checkNotifier = ThrowImmediatelyCheckNotifier.Quiet;

        _sql = "";

        //reset the Parameter knowledge
        ParameterManager.ClearNonGlobals();

        #region Setup to output the query, where we figure out all the joins etc

        //reset everything

        SelectColumns.Sort();

        //work out all the filters
        Filters = SqlQueryBuilderHelper.GetAllFiltersUsedInContainerTreeRecursively(RootFilterContainer);

        TablesUsedInQuery = SqlQueryBuilderHelper.GetTablesUsedInQuery(this, out var primary, _forceJoinsToTheseTables);

        //force join to any TableInfos that would not be normally joined to but the user wants to anyway e.g. if there's WHERE sql that references them but no columns
        if (_forceJoinsToTheseTables != null)
            foreach (var force in _forceJoinsToTheseTables)
                if (!TablesUsedInQuery.Contains(force))
                    TablesUsedInQuery.Add(force);

        PrimaryExtractionTable = primary;

        SqlQueryBuilderHelper.FindLookups(this);

        JoinsUsedInQuery = SqlQueryBuilderHelper.FindRequiredJoins(this);

        //deal with case when there are no tables in the query or there are only lookup descriptions in the query
        if (TablesUsedInQuery.Count == 0)
            throw new Exception("There are no TablesUsedInQuery in this dataset");


        QuerySyntaxHelper = SqlQueryBuilderHelper.GetSyntaxHelper(TablesUsedInQuery);

        if (TopX != -1)
            SqlQueryBuilderHelper.HandleTopX(this, QuerySyntaxHelper, TopX);
        else
            SqlQueryBuilderHelper.ClearTopX(this);

        //declare parameters
        ParameterManager.AddParametersFor(Filters);

        #endregion

        /////////////////////////////////////////////Assemble Query///////////////////////////////

        #region Preamble (including variable declarations/initializations)

        //assemble the query - never use Environment.Newline, use TakeNewLine() so that QueryBuilder knows what line its got up to
        var toReturn = "";

        foreach (var parameter in ParameterManager.GetFinalResolvedParametersList())
        {
            //if the parameter is one that needs to be told what the query syntax helper is e.g. if it's a global parameter designed to work on multiple datasets
            if (parameter is IInjectKnown<IQuerySyntaxHelper> needsToldTheSyntaxHelper)
                needsToldTheSyntaxHelper.InjectKnown(QuerySyntaxHelper);

            if (CheckSyntax)
                parameter.Check(checkNotifier);

            toReturn += GetParameterDeclarationSQL(parameter);
        }

        //add user custom Parameter lines
        toReturn = AppendCustomLines(toReturn, QueryComponent.VariableDeclaration);

        #endregion

        #region Select (including all IColumns)

        toReturn += Environment.NewLine;
        toReturn += $"SELECT {LimitationSQL}{Environment.NewLine}";

        toReturn = AppendCustomLines(toReturn, QueryComponent.SELECT);
        toReturn += Environment.NewLine;

        toReturn = AppendCustomLines(toReturn, QueryComponent.QueryTimeColumn);

        for (var i = 0; i < SelectColumns.Count; i++)
        {
            //output each of the ExtractionInformations that the user requested and record the line number for posterity
            var columnAsSql = SelectColumns[i].GetSelectSQL(_hashingAlgorithm, _salt, QuerySyntaxHelper);

            //there is another one coming
            if (i + 1 < SelectColumns.Count)
                columnAsSql += ",";

            toReturn += columnAsSql + Environment.NewLine;
        }

        #endregion

        //work out basic JOINS Sql
        toReturn += SqlQueryBuilderHelper.GetFROMSQL(this);

        //add user custom JOIN lines
        toReturn = AppendCustomLines(toReturn, QueryComponent.JoinInfoJoin);

        #region Filters (WHERE)

        toReturn += SqlQueryBuilderHelper.GetWHERESQL(this);

        toReturn = AppendCustomLines(toReturn, QueryComponent.WHERE);
        toReturn = AppendCustomLines(toReturn, QueryComponent.Postfix);

        _sql = toReturn;
        SQLOutOfDate = false;

        #endregion
    }

    private string AppendCustomLines(string toReturn, QueryComponent stage)
    {
        var lines = SqlQueryBuilderHelper.GetCustomLinesSQLForStage(this, stage).ToArray();
        if (lines.Any())
        {
            toReturn += Environment.NewLine;
            toReturn += string.Join(Environment.NewLine, lines.Select(l => l.Text));
        }

        return toReturn;
    }

    /// <inheritdoc />
    public IEnumerable<Lookup> GetDistinctRequiredLookups()
    {
        return SqlQueryBuilderHelper.GetDistinctRequiredLookups(this);
    }

    /// <summary>
    ///     Generates Sql to comment, declare and set the initial value for the supplied <see cref="ISqlParameter" />.
    /// </summary>
    /// <param name="sqlParameter"></param>
    /// <returns></returns>
    public static string GetParameterDeclarationSQL(ISqlParameter sqlParameter)
    {
        var toReturn = "";

        if (!string.IsNullOrWhiteSpace(sqlParameter.Comment))
            toReturn += $"/*{sqlParameter.Comment}*/{Environment.NewLine}";

        toReturn += sqlParameter.ParameterSQL + Environment.NewLine;

        //it's a table valued parameter! advanced
        if (!string.IsNullOrEmpty(sqlParameter.Value) &&
            Regex.IsMatch(sqlParameter.Value, @"\binsert\s+into\b", RegexOptions.IgnoreCase))
            toReturn += $"{sqlParameter.Value};{Environment.NewLine}";
        else
            toReturn +=
                $"SET {sqlParameter.ParameterName}={sqlParameter.Value};{Environment.NewLine}"; //its a regular value

        return toReturn;
    }

    public static string GetParameterDeclarationSQL(IEnumerable<ISqlParameter> sqlParameters)
    {
        return string.Join("", sqlParameters.Select(GetParameterDeclarationSQL));
    }
}
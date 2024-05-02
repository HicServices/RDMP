// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing;

/// <summary>
///     <see cref="IViewSQLAndResultsCollection" /> for querying a sample of records in an <see cref="ExtractableCohort" />
///     (list of patient identifiers)
/// </summary>
public class ViewCohortExtractionUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection
{
    public int Top
    {
        get => _arguments.TryGetValue(TopKey, out var value) ? int.Parse(value) : 100;
        set => _arguments[TopKey] = value.ToString();
    }

    private Dictionary<string, string> _arguments = new();
    private const string TopKey = "Top";

    private const string IncludeCohortIDKey = "IncludeCohortID";

    /// <summary>
    ///     True to fetch the cohort ID (OriginID) from the cohort table as a SELECT column when retrieving records
    /// </summary>
    public bool IncludeCohortID
    {
        get => !_arguments.TryGetValue(IncludeCohortIDKey, out var value) || bool.Parse(value);
        set => _arguments[IncludeCohortIDKey] = value.ToString();
    }

    public ViewCohortExtractionUICollection()
    {
    }

    public ViewCohortExtractionUICollection(ExtractableCohort cohort) : this()
    {
        DatabaseObjects.Add(cohort);
    }

    public override string SaveExtraText()
    {
        return PersistStringHelper.SaveDictionaryToString(_arguments);
    }

    public override void LoadExtraText(string s)
    {
        _arguments = PersistStringHelper.LoadDictionaryFromString(s);
    }

    public ExtractableCohort Cohort => DatabaseObjects.OfType<ExtractableCohort>().SingleOrDefault();


    public IEnumerable<DatabaseEntity> GetToolStripObjects()
    {
        yield return Cohort;
    }

    public IDataAccessPoint GetDataAccessPoint()
    {
        return Cohort.ExternalCohortTable;
    }

    public string GetSql()
    {
        if (Cohort == null)
            return "";

        var ect = Cohort.ExternalCohortTable;
        var tableName = ect.TableName;

        var response = GetQuerySyntaxHelper().HowDoWeAchieveTopX(Top);

        var selectSql = GetSelectList(ect);

        // Don't bother with top/limit SQL if theres none set
        var responseSQL = response.SQL;
        if (Top <= 0) responseSQL = "";

        return response.Location switch
        {
            QueryComponent.SELECT => $"Select {responseSQL} {selectSql} from {tableName} WHERE {Cohort.WhereSQL()}",
            QueryComponent.WHERE => $"Select {selectSql} from {tableName} WHERE {responseSQL} AND {Cohort.WhereSQL()}",
            QueryComponent.Postfix => $"Select {selectSql} from {tableName} WHERE {Cohort.WhereSQL()} {responseSQL}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    ///     Returns a block that should be inserted after the SELECT keyword in the query
    ///     that contains all relevant columns to be extracted for cohorts in the given cohort
    ///     database
    /// </summary>
    /// <param name="ect"></param>
    /// <returns></returns>
    private string GetSelectList(IExternalCohortTable ect)
    {
        var selectList = new List<string> { ect.PrivateIdentifierField };

        // if it is not an identifiable extraction
        if (!string.Equals(ect.PrivateIdentifierField, ect.ReleaseIdentifierField))
            // add the release identifier too
            selectList.Add(ect.ReleaseIdentifierField);

        if (IncludeCohortID) selectList.Add(ect.DefinitionTableForeignKeyField);

        return Environment.NewLine + string.Join($",{Environment.NewLine}", selectList) + Environment.NewLine;
    }

    public string GetTabName()
    {
        return $"View {Cohort}(V{Cohort.ExternalVersion})";
    }

    public void AdjustAutocomplete(IAutoCompleteProvider autoComplete)
    {
        if (Cohort == null)
            return;

        var ect = Cohort.ExternalCohortTable;
        var table = ect.DiscoverCohortTable();
        autoComplete.Add(table);
    }

    public IQuerySyntaxHelper GetQuerySyntaxHelper()
    {
        return Cohort?.GetQuerySyntaxHelper();
    }
}
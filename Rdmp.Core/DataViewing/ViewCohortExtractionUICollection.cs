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
using ReusableLibraryCode.DataAccess;

namespace Rdmp.Core.DataViewing
{
    /// <summary>
    /// <see cref="IViewSQLAndResultsCollection"/> for querying a sample of records in an <see cref="ExtractableCohort"/> (list of patient identifiers)
    /// </summary>
    public class ViewCohortExtractionUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection
    {
        public int Top
        {
            get => _arguments.ContainsKey(TopKey) ? int.Parse(_arguments[TopKey]) : 100;
            set => _arguments[TopKey] = value.ToString();
        }

        Dictionary<string, string> _arguments = new Dictionary<string, string>();
        private const string TopKey = "Top";

        public string Username { get; set; }

        public ViewCohortExtractionUICollection()
        {
        }

        public ViewCohortExtractionUICollection(ExtractableCohort cohort) : this()
        {
            DatabaseObjects.Add(cohort);
        }

        public override string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(_arguments);
        }
        public override void LoadExtraText(string s)
        {
            _arguments = Helper.LoadDictionaryFromString(s);
        }

        public ExtractableCohort Cohort { get { return DatabaseObjects.OfType<ExtractableCohort>().SingleOrDefault(); } }

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

            var tableName = Cohort.ExternalCohortTable.TableName;

            var response = GetQuerySyntaxHelper().HowDoWeAchieveTopX(Top);

            return response.Location switch
            {
                QueryComponent.SELECT  => $"Select {response.SQL} * from {tableName} WHERE {Cohort.WhereSQL()}",
                QueryComponent.WHERE   => $"Select * from {tableName} WHERE {response.SQL} AND {Cohort.WhereSQL()}",
                QueryComponent.Postfix => $"Select * from {tableName} WHERE {Cohort.WhereSQL()} {response.SQL}",
                _ => throw new ArgumentOutOfRangeException()
            };
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
}
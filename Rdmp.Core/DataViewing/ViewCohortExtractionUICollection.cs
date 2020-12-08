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
    public class ViewCohortExtractionUICollection : PersistableObjectCollection, IViewSQLAndResultsCollection
    {
        public ViewCohortExtractionUICollection()
        {
        }

        public ViewCohortExtractionUICollection(ExtractableCohort cohort) : this()
        {
            DatabaseObjects.Add(cohort);
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

            var response = GetQuerySyntaxHelper().HowDoWeAchieveTopX(100);

            switch (response.Location)
            {
                case QueryComponent.SELECT:
                    return "Select " + response.SQL + " * from " + tableName + " WHERE " + Cohort.WhereSQL();
                case QueryComponent.WHERE:
                    return "Select * from " + tableName + " WHERE " + response.SQL + " AND " + Cohort.WhereSQL();
                case QueryComponent.Postfix:
                    return "Select * from " + tableName + " WHERE " + Cohort.WhereSQL() + " " + response.SQL;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public string GetTabName()
        {
            return "Top 100 " + Cohort + "(V" + Cohort.ExternalVersion + ")";
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
            var c = Cohort;
            return c != null ? c.GetQuerySyntaxHelper() : null;
        }
    }
}
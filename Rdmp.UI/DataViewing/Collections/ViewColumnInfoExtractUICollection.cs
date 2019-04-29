// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.Dashboarding;
using Rdmp.Core.CatalogueLibrary.Data.Spontaneous;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.UI.AutoComplete;
using ReusableLibraryCode.DataAccess;

namespace Rdmp.UI.DataViewing.Collections
{
    public class ViewColumnInfoExtractUICollection : PersistableObjectCollection,IViewSQLAndResultsCollection
    {
        public ViewType ViewType { get; private set; }

        /// <summary>
        /// for persistence, do not use
        /// </summary>
        public ViewColumnInfoExtractUICollection()
        {
        }

        public ViewColumnInfoExtractUICollection(ColumnInfo c, ViewType viewType, IFilter filter = null): this()
        {
            DatabaseObjects.Add(c);
            if (filter != null)
                DatabaseObjects.Add(filter);
            ViewType = viewType;
        }

        public override string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(new Dictionary<string, string>() {{"ViewType", ViewType.ToString()}});
        }

        public override void LoadExtraText(string s)
        {
            string value = Helper.GetValueIfExistsFromPersistString("ViewType", s);
            ViewType = (ViewType) Enum.Parse(typeof (ViewType), value);
        }
        
        public IEnumerable<DatabaseEntity> GetToolStripObjects()
        {
            var filter = GetFilterIfAny() as ConcreteFilter;

            if (filter != null)
                yield return filter;

            yield return ColumnInfo.TableInfo;
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            if (ColumnInfo == null)
                return null;

            return ColumnInfo.TableInfo;
        }

        public string GetSql()
        {
            var qb = new QueryBuilder(null, null,new []{ColumnInfo.TableInfo});

            if (ViewType == ViewType.TOP_100)
                qb.TopX = 100;

            if (ViewType == ViewType.Distribution)
                AddDistributionColumns(qb);
            else
                qb.AddColumn(new ColumnInfoToIColumn(new MemoryRepository(),ColumnInfo));

            var filter = GetFilterIfAny();
            if (filter != null && !string.IsNullOrWhiteSpace(filter.WhereSQL))
                qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(new MemoryCatalogueRepository(), null, new[] { filter }, FilterContainerOperation.AND);

            if (ViewType == ViewType.Aggregate)
                qb.AddCustomLine("count(*),", QueryComponent.QueryTimeColumn);

            var sql = qb.SQL;

            if(ViewType == ViewType.Aggregate)
                sql += " GROUP BY " + ColumnInfo;

            return sql;
        }

        private void AddDistributionColumns(QueryBuilder qb)
        {
            var repo = new MemoryRepository();
            qb.AddColumn(new SpontaneouslyInventedColumn(repo,"CountTotal", "count(1)"));
            qb.AddColumn(new SpontaneouslyInventedColumn(repo,"CountNull", "SUM(CASE WHEN " + ColumnInfo.GetFullyQualifiedName() + " IS NULL THEN 1 ELSE 0  END)"));
            qb.AddColumn(new SpontaneouslyInventedColumn(repo,"CountZero", "SUM(CASE WHEN " + ColumnInfo.GetFullyQualifiedName() + " = 0 THEN 1  ELSE 0 END)"));

            qb.AddColumn(new SpontaneouslyInventedColumn(repo,"Max", "max(" + ColumnInfo.GetFullyQualifiedName() + ")"));
            qb.AddColumn(new SpontaneouslyInventedColumn(repo,"Min", "min(" + ColumnInfo.GetFullyQualifiedName() + ")"));

            switch (ColumnInfo.GetQuerySyntaxHelper().DatabaseType)
            {
                case DatabaseType.MicrosoftSQLServer:
                    qb.AddColumn(new SpontaneouslyInventedColumn(repo,"stdev ", "stdev(" + ColumnInfo.GetFullyQualifiedName() + ")"));
                    break;
                case DatabaseType.MySql:
                case DatabaseType.Oracle:
                    qb.AddColumn(new SpontaneouslyInventedColumn(repo, "stddev ", "stddev(" + ColumnInfo.GetFullyQualifiedName() + ")"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            qb.AddColumn(new SpontaneouslyInventedColumn(repo, "avg", "avg(" + ColumnInfo.GetFullyQualifiedName() + ")"));

        }

        public string GetTabName()
        {
            return ColumnInfo + "(" + ViewType + ")";
        }

        public void AdjustAutocomplete(AutoCompleteProvider autoComplete)
        {
            autoComplete.Add(ColumnInfo);
        }

        public ColumnInfo ColumnInfo
        {
            get { return DatabaseObjects.OfType<ColumnInfo>().SingleOrDefault(); }
        }

        private IFilter GetFilterIfAny()
        {
            return (IFilter) DatabaseObjects.SingleOrDefault(o => o is IFilter);
        }

        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var c = ColumnInfo;
            return c != null ? c.GetQuerySyntaxHelper() : null;
        }
    }
}
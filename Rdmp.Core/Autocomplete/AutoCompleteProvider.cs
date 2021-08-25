// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataViewing;
using Rdmp.Core.QueryBuilding;

namespace Rdmp.Core.Autocomplete
{
    /// <summary>
    /// Creates autocomplete strings based on RDMP objects (e.g. <see cref="TableInfo"/>)
    /// </summary>
    public class AutoCompleteProvider : IAutoCompleteProvider
    {
        protected HashSet<string> Items = new HashSet<string>();

        public AutoCompleteProvider()
        {

        }
        public AutoCompleteProvider(IQuerySyntaxHelper helper)
        {
            if(helper != null)
            {
                AddSQLKeywords(helper);
            }
        }

        protected IEnumerable<string> GetBits(string arg)
        {
            //     yield return arg;

            foreach (Match m in Regex.Matches(arg, @"\b\w*\b"))
                yield return m.Value;
        }

        public void Add(ITableInfo tableInfo)
        {
            Add(tableInfo, LoadStage.PostLoad);
        }


        public void Add(ColumnInfo columnInfo, ITableInfo tableInfo, string databaseName, LoadStage stage, IQuerySyntaxHelper syntaxHelper)
        {
            var col = columnInfo.GetRuntimeName(stage);
            var table = tableInfo.GetRuntimeName(stage);
            var dbName = tableInfo.GetDatabaseRuntimeName(stage);

            var fullySpecified = syntaxHelper.EnsureFullyQualified(dbName, tableInfo.Schema, table, col);

            AddUnlessDuplicate(fullySpecified);
        }

        public void Add(ColumnInfo columnInfo)
        {
            AddUnlessDuplicate(columnInfo.GetFullyQualifiedName());
        }

        private void Add(PreLoadDiscardedColumn discardedColumn, ITableInfo tableInfo, string rawDbName)
        {
            var colName = discardedColumn.GetRuntimeName();

            AddUnlessDuplicate(tableInfo.GetQuerySyntaxHelper().EnsureFullyQualified(rawDbName, null, tableInfo.GetRuntimeName(), colName));
        }

        public void Add(IColumn column)
        {
            string runtimeName;
            try
            {
                runtimeName = column.GetRuntimeName();
            }
            catch (Exception)
            {
                return;
            }

            AddUnlessDuplicate(column.SelectSQL);
        }

        private void AddUnlessDuplicate(string text)
        {
            Items.Add(text);
        }

        public void AddSQLKeywords(IQuerySyntaxHelper syntaxHelper)
        {
            if (syntaxHelper == null)
                return;

            foreach (KeyValuePair<string, string> kvp in syntaxHelper.GetSQLFunctionsDictionary())
            {
                AddUnlessDuplicate(kvp.Value);
            }
        }

        public void Add(ISqlParameter parameter)
        {
            AddUnlessDuplicate(parameter.ParameterName);
        }

        public void Add(ITableInfo tableInfo, LoadStage loadStage)
        {
            //we already have it or it is not setup properly
            if (string.IsNullOrWhiteSpace(tableInfo.Database) || string.IsNullOrWhiteSpace(tableInfo.Server))
                return;

            var runtimeName = tableInfo.GetRuntimeName(loadStage);
            var dbName = tableInfo.GetDatabaseRuntimeName(loadStage);

            var syntaxHelper = tableInfo.GetQuerySyntaxHelper();
            var fullSql = syntaxHelper.EnsureFullyQualified(dbName, null, runtimeName);


            foreach (IHasStageSpecificRuntimeName o in tableInfo.GetColumnsAtStage(loadStage))
            {
                var preDiscarded = o as PreLoadDiscardedColumn;
                var columnInfo = o as ColumnInfo;

                if (preDiscarded != null)
                    Add(preDiscarded, tableInfo, dbName);
                else
                if (columnInfo != null)
                    Add(columnInfo, tableInfo, dbName, loadStage, syntaxHelper);
                else throw new Exception("Expected IHasStageSpecificRuntimeName returned by TableInfo.GetColumnsAtStage to return only ColumnInfos and PreLoadDiscardedColumns.  It returned a '" + o.GetType().Name + "'");
            }

            AddUnlessDuplicate(fullSql);
        }

        public void Add(DiscoveredTable discoveredTable)
        {
            AddUnlessDuplicate(discoveredTable.GetFullyQualifiedName());

            DiscoveredColumn[] columns = null;
            try
            {
                if (discoveredTable.Exists())
                    columns = discoveredTable.DiscoverColumns();
            }
            catch (Exception)
            {
                //couldn't load nevermind
            }

            if (columns != null)
                foreach (var col in columns)
                    Add(col);
        }

        private void Add(DiscoveredColumn discoveredColumn)
        {
            AddUnlessDuplicate(discoveredColumn.GetFullyQualifiedName());
        }

        public void Clear()
        {
            Items.Clear();
        }

        public void Add(Type type)
        {
            Items.Add(type.Name);
        }

        public void Add(AggregateConfiguration aggregateConfiguration)
        {
            Add(aggregateConfiguration.Catalogue);
        }

        public void Add(ICatalogue catalogue)
        {
            foreach (var ei in catalogue.GetAllExtractionInformation(ExtractionCategory.Any))
                Add(ei);
        }
    }
}

// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using AutocompleteMenuNS;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using FAnsi.Discovery;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using ScintillaNET;

namespace CatalogueManager.AutoComplete
{
    public class AutoCompleteProvider
    {
        private IActivateItems _activator;
        private AutocompleteMenu _autocomplete = new AutocompleteMenu();

        List<AutocompleteItem> items = new List<AutocompleteItem>();
        private ImageList _imageList;

        public AutoCompleteProvider(IActivateItems activator)
        {
            _activator = activator;
            _imageList = _activator.CoreIconProvider.GetImageList(true);
            _autocomplete.ImageList = _imageList;
            _autocomplete.SearchPattern = @"[\w@\.]";
        }


        public void RegisterForEvents(Scintilla queryEditor)
        {
            _autocomplete.TargetControlWrapper = new ScintillaWrapper(queryEditor);
        }
        
        public void Add(ITableInfo tableInfo)
        {
            Add(tableInfo,LoadStage.PostLoad);
        }


        public void Add(ColumnInfo columnInfo, ITableInfo tableInfo, string databaseName, LoadStage stage, IQuerySyntaxHelper syntaxHelper)
        {
            var col = columnInfo.GetRuntimeName(stage);
            var table = tableInfo.GetRuntimeName(stage);
            var dbName = tableInfo.GetDatabaseRuntimeName(stage);

            var snip = new SubstringAutocompleteItem(col);
            snip.MenuText = col;

            var fullySpecified = syntaxHelper.EnsureFullyQualified(dbName, tableInfo.Schema, table, col);

            snip.Text = fullySpecified;
            snip.Tag = columnInfo;
            snip.ImageIndex = GetIndexFor(columnInfo, RDMPConcept.ColumnInfo.ToString());
            
            AddUnlessDuplicate(snip);
        }

        public void Add(ColumnInfo columnInfo)
        {
            var snip = new SubstringAutocompleteItem(columnInfo.GetRuntimeName());
            snip.MenuText = columnInfo.GetRuntimeName();
            snip.Text = columnInfo.GetFullyQualifiedName();
            snip.Tag = columnInfo;
            snip.ImageIndex = GetIndexFor(columnInfo, RDMPConcept.ColumnInfo.ToString());

            AddUnlessDuplicate(snip);
        }

        private void Add(PreLoadDiscardedColumn discardedColumn, ITableInfo tableInfo, string rawDbName)
        {
            var snip = new SubstringAutocompleteItem(discardedColumn.GetRuntimeName());
            var colName = discardedColumn.GetRuntimeName();
            snip.MenuText = colName;

            snip.Text = tableInfo.GetQuerySyntaxHelper().EnsureFullyQualified(rawDbName,null, tableInfo.GetRuntimeName(), colName);
            snip.Tag = discardedColumn;
            snip.ImageIndex = GetIndexFor(discardedColumn, RDMPConcept.ColumnInfo.ToString());

            AddUnlessDuplicate(snip);
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
            var snip = new SubstringAutocompleteItem(runtimeName);
            snip.MenuText = column.GetRuntimeName();
            snip.Text = column.SelectSQL;
            snip.Tag = column;
            snip.ImageIndex = GetImageIndexForType(column);

            AddUnlessDuplicate(snip);
        }

        private void AddUnlessDuplicate(AutocompleteItem snip)
        {
            //already got this snip
            if(items.Any(i=>i.Text.Equals(snip.Text) && i.MenuText.Equals(snip.MenuText)))
                return;

            if(_activator == null)
                throw new Exception("You cannot add items to AutoCompleteProvider until it has an ItemActivator");

            snip.ToolTipTitle = "Code Snip";
            snip.ToolTipText = snip.Text;
            
            items.Add(snip);

            _autocomplete.SetAutocompleteItems(items);
        }
        
        private int GetIndexFor(object o, string key)
        {
            return _imageList.Images.IndexOfKey(IsFavourite(o) ? key + "Favourite" : key);
        }
        
        private bool IsFavourite(object potentialFavourite)
        {
            var dbObj = potentialFavourite as IMapsDirectlyToDatabaseTable;
            return  dbObj != null && _activator.FavouritesProvider.IsFavourite(dbObj);
        }

        private int GetImageIndexForType(IColumn column)
        {
            if (column is ExtractionInformation || column is ExtractableColumn || column is ReleaseIdentifierSubstitution)
                return GetIndexFor(column, RDMPConcept.ExtractionInformation.ToString());

            return GetIndexFor(column, RDMPConcept.ColumnInfo.ToString());
        }

        public void AddSQLKeywords(IQuerySyntaxHelper syntaxHelper)
        {
            if (syntaxHelper == null)
                return;

            foreach (KeyValuePair<string, string> kvp in syntaxHelper.GetSQLFunctionsDictionary())
            {
                var snip = new SubstringAutocompleteItem(kvp.Key);
                snip.MenuText = kvp.Key;
                snip.Text = kvp.Value;
                snip.Tag = kvp;
                snip.ImageIndex = GetIndexFor(null, RDMPConcept.SQL.ToString());//sql icon

                AddUnlessDuplicate(snip);
            }
        }

        public void Add(ISqlParameter parameter)
        {
            string name = parameter.ParameterName;

            var snip = new SubstringAutocompleteItem(name);
            snip.Tag = name;
            snip.Text = parameter.ParameterName;
            snip.ImageIndex = GetIndexFor(parameter, RDMPConcept.ParametersNode.ToString());//parameter icon

            snip.ToolTipText = snip.ToString();

            AddUnlessDuplicate(snip);
        }
        
        public void Add(ITableInfo tableInfo, LoadStage loadStage)
        {
            //we already have it
            if(items.Any(i=>i.Tag.Equals(tableInfo)))
                return;

            var runtimeName = tableInfo.GetRuntimeName(loadStage);
            var dbName = tableInfo.GetDatabaseRuntimeName(loadStage);

            var syntaxHelper = tableInfo.GetQuerySyntaxHelper();
            var fullSql = syntaxHelper.EnsureFullyQualified(dbName,null, runtimeName);

            var snip = new SubstringAutocompleteItem(tableInfo.GetRuntimeName());
            snip.MenuText = runtimeName; //name of table
            snip.Text = fullSql;//full SQL
            snip.Tag = tableInfo; //record object for future reference
            snip.ImageIndex = GetIndexFor(tableInfo,RDMPConcept.TableInfo.ToString());
            

            foreach (IHasStageSpecificRuntimeName o in tableInfo.GetColumnsAtStage(loadStage))
            {
                var preDiscarded = o as PreLoadDiscardedColumn;
                var columnInfo = o as ColumnInfo;

                if(preDiscarded != null)
                    Add(preDiscarded, tableInfo, dbName);
                else
                if(columnInfo != null)
                    Add(columnInfo, tableInfo, dbName, loadStage, syntaxHelper);
                else throw new Exception("Expected IHasStageSpecificRuntimeName returned by TableInfo.GetColumnsAtStage to return only ColumnInfos and PreLoadDiscardedColumns.  It returned a '" + o.GetType().Name +"'");
            }

            AddUnlessDuplicate(snip);
        }

        public void Add(DiscoveredTable discoveredTable)
        {
            if (items.Any(i => i.Tag.Equals(discoveredTable)))
                return;
            
            var snip = new SubstringAutocompleteItem(discoveredTable.GetRuntimeName());
            snip.MenuText = discoveredTable.GetRuntimeName(); //name of table
            snip.Text = discoveredTable.GetFullyQualifiedName();//full SQL
            snip.Tag = discoveredTable; //record object for future reference
            snip.ImageIndex = GetIndexFor(discoveredTable, RDMPConcept.TableInfo.ToString());


            AddUnlessDuplicate(snip);

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

            if(columns != null)
                foreach (var col in columns)
                    Add(col);
        }

        private void Add(DiscoveredColumn discoveredColumn)
        {
            if (items.Any(i => i.Tag.Equals(discoveredColumn)))
                return;

            var snip = new SubstringAutocompleteItem(discoveredColumn.GetRuntimeName());
            snip.MenuText = discoveredColumn.GetRuntimeName(); //name of table
            snip.Text = discoveredColumn.GetFullyQualifiedName();//full SQL
            snip.Tag = discoveredColumn; //record object for future reference
            snip.ImageIndex = GetIndexFor(discoveredColumn, RDMPConcept.ColumnInfo.ToString());

            AddUnlessDuplicate(snip);
        }

        public void Clear()
        {
            items.Clear();
            _autocomplete.SetAutocompleteItems(items);
        }

        public void Add(Type type)
        {
            //we already have it
            if (items.Any(i => i.Tag.Equals(type)))
                return;

            var snip = new SubstringAutocompleteItem(type.Name);
            snip.MenuText = type.Name; //name of table
            snip.Text = type.Name;//full text
            snip.Tag = type; //record object for future reference

            if (!_imageList.Images.ContainsKey(type.Name))
            {
                var img = _activator.CoreIconProvider.GetImage(type);
                if (img != null)
                    _imageList.Images.Add(type.Name, img);
            }

            snip.ImageIndex = GetIndexFor(type, type.Name);
            items.Add(snip);

            _autocomplete.SetAutocompleteItems(items);
        }

        public bool IsShowing()
        {
            if (_autocomplete == null)
                return false;

            return _autocomplete.Visible;
        }

        public void UnRegister()
        {
            _autocomplete.TargetControlWrapper = null;
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

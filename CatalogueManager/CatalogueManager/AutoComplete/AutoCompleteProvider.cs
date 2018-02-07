using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutocompleteMenuNS;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.DataLoad;
using CatalogueManager.Collections.Providers;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.ExtractionTime;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using DataLoadEngine.Migration;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableUIComponents.ScintillaHelper;
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
        }


        public void RegisterForEvents(Scintilla queryEditor)
        {
            _autocomplete.TargetControlWrapper = new ScintillaWrapper(queryEditor);
        }
        
        public void Add(TableInfo tableInfo)
        {
            Add(tableInfo,LoadStage.PostLoad);
        }


        private void Add(ColumnInfo columnInfo, TableInfo tableInfo, string databaseName,LoadStage stage)
        {
            var runtimeName = columnInfo.GetRuntimeName(stage);

            var snip = new SubstringAutocompleteItem(runtimeName);
            snip.MenuText = runtimeName;
            snip.Text = tableInfo.GetQuerySyntaxHelper().EnsureFullyQualified(databaseName, null, tableInfo.GetRuntimeName(), runtimeName);
            snip.Tag = columnInfo;
            snip.ImageIndex = GetIndexFor(columnInfo, RDMPConcept.ColumnInfo.ToString());
            
            AddUnlessDuplicate(snip);
        }
        private void Add(PreLoadDiscardedColumn discardedColumn, TableInfo tableInfo, string rawDbName)
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
            catch (Exception e)
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

            if (column is AggregateDimension || column is CohortCustomColumn)
                return GetIndexFor(column, RDMPConcept.ColumnInfo.ToString());
            
            throw new ArgumentOutOfRangeException("Did not know what type of icon to use for IColumn Type:" + column.GetType());
        }

        public void AddSQLKeywords()
        {

            foreach (KeyValuePair<string, string> kvp in ScintillaTextEditorFactory.SQLFunctionsDictionary)
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
            snip.ImageIndex = GetIndexFor(parameter, RDMPConcept.ParametersNode.ToString());//parameter icon

            snip.ToolTipText = snip.ToString();

            AddUnlessDuplicate(snip);
        }
        
        public void Add(TableInfo tableInfo, LoadStage loadStage)
        {
            //we already have it
            if(items.Any(i=>i.Tag.Equals(tableInfo)))
                return;

            var runtimeName = tableInfo.GetRuntimeName(loadStage);
            var dbName = tableInfo.GetDatabaseRuntimeName(loadStage);

            var fullSql = tableInfo.GetQuerySyntaxHelper().EnsureFullyQualified(dbName,null, runtimeName);

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
                    Add(columnInfo, tableInfo, dbName, loadStage);
                else throw new Exception("Expected IHasStageSpecificRuntimeName returned by TableInfo.GetColumnsAtStage to return only ColumnInfos and PreLoadDiscardedColumns.  It returned a '" + o.GetType().Name +"'");
            }

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
    }
}

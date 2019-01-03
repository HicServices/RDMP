using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.AutoComplete;
using CatalogueManager.ObjectVisualisation;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueManager.DataViewing.Collections
{
    public class ViewTableInfoExtractUICollection :PersistableObjectCollection, IViewSQLAndResultsCollection
    {
        public ViewType ViewType { get; private set; }

        /// <summary>
        /// for persistence, do not use
        /// </summary>
        public ViewTableInfoExtractUICollection()
        {
        }

        public ViewTableInfoExtractUICollection(ITableInfo t, ViewType viewType, IFilter filter = null)
            : this()
        {
            DatabaseObjects.Add(t);

            if(filter != null)
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

        public object GetDataObject()
        {
            return DatabaseObjects.Single(o=>o is ColumnInfo || o is TableInfo);
        }

        public IFilter GetFilterIfAny()
        {
            return (IFilter) DatabaseObjects.SingleOrDefault(o => o is IFilter);
        }
        public IEnumerable<DatabaseEntity> GetToolStripObjects()
        {
            yield return TableInfo;
            
            var filter = GetFilterIfAny() as ConcreteFilter;

            if (filter != null)
                yield return filter;
        }

        public IEnumerable<string> GetToolStripStrings()
        {
            yield return ViewType.ToString();
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            return TableInfo;
        }

        public string GetSql()
        {
            var qb = new QueryBuilder(null, null);
            
            if(ViewType ==  ViewType.TOP_100)
            qb.TopX = 100;

            qb.AddColumnRange(TableInfo.ColumnInfos.Select(c => new ColumnInfoToIColumn(c)).ToArray());

            var filter = GetFilterIfAny();
            if(filter != null)
                qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(null,new []{filter},FilterContainerOperation.AND);
            
            if(ViewType == ViewType.Aggregate)
                qb.AddCustomLine("count(*),",QueryComponent.QueryTimeColumn);

            var sql = qb.SQL;

            if (ViewType == ViewType.Aggregate)
                throw new NotSupportedException("ViewType.Aggregate can only be applied to ColumnInfos not TableInfos");

            return sql;
        
        }

        public string GetTabName()
        {
            return TableInfo + "(" + ViewType + ")";
        }

        public void AdjustAutocomplete(AutoCompleteProvider autoComplete)
        {
            autoComplete.Add(TableInfo);
        }

        public TableInfo TableInfo { get { return DatabaseObjects.OfType<TableInfo>().SingleOrDefault(); } }
        public IQuerySyntaxHelper GetQuerySyntaxHelper()
        {
            var t = TableInfo;
            return t != null ? t.GetQuerySyntaxHelper() : null;
        }
    }
}
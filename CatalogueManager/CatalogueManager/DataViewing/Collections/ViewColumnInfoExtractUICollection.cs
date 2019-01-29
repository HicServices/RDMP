using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.AutoComplete;
using CatalogueManager.ObjectVisualisation;
using FAnsi.Discovery.QuerySyntax;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.DataViewing.Collections
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
            var qb = new QueryBuilder(null, null);

            if (ViewType == ViewType.TOP_100)
                qb.TopX = 100;

            qb.AddColumn(new ColumnInfoToIColumn(ColumnInfo));
            
            var filter = GetFilterIfAny();
            if (filter != null && !string.IsNullOrWhiteSpace(filter.WhereSQL))
                qb.RootFilterContainer = new SpontaneouslyInventedFilterContainer(null, new[] { filter }, FilterContainerOperation.AND);

            if (ViewType == ViewType.Aggregate)
                qb.AddCustomLine("count(*),", QueryComponent.QueryTimeColumn);

            var sql = qb.SQL;

            if(ViewType == ViewType.Aggregate)
                sql += " GROUP BY " + ColumnInfo;

            return sql;
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
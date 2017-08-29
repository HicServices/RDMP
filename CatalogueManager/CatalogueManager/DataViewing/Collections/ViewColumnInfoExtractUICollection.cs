using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.QueryBuilding;
using CatalogueLibrary.Spontaneous;
using CatalogueManager.ObjectVisualisation;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery.QuerySyntax;

namespace CatalogueManager.DataViewing.Collections
{
    public class ViewColumnInfoExtractUICollection : IViewSQLAndResultsCollection
    {
        public PersistStringHelper Helper { get; private set;}
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        public ViewType ViewType { get; private set; }

        /// <summary>
        /// for persistence, do not use
        /// </summary>
        public ViewColumnInfoExtractUICollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        public ViewColumnInfoExtractUICollection(ColumnInfo c, ViewType viewType, IFilter filter = null): this()
        {
            DatabaseObjects.Add(c);
            if (filter != null)
                DatabaseObjects.Add(filter);
            ViewType = viewType;
        }

        public string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(new Dictionary<string, string>() {{"ViewType", ViewType.ToString()}});
        }

        public void LoadExtraText(string s)
        {
            string value = Helper.GetValueIfExistsFromPersistString("ViewType", s);
            ViewType = (ViewType) Enum.Parse(typeof (ViewType), value);
        }

        public IHasDependencies GetAutocompleteObject()
        {
            return ColumnInfo;
        }

        public void SetupRibbon(RDMPObjectsRibbonUI ribbon)
        {
            ribbon.Add(ColumnInfo);
            ribbon.Add(ViewType.ToString());

            var filter = GetFilterIfAny();

            if(filter != null)
                ribbon.Add(filter as ConcreteFilter);
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
            if (filter != null)
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

        public ColumnInfo ColumnInfo
        {
            get { return DatabaseObjects.OfType<ColumnInfo>().SingleOrDefault(); }
        }

        private IFilter GetFilterIfAny()
        {
            return (IFilter) DatabaseObjects.SingleOrDefault(o => o is IFilter);
        }
    }
}
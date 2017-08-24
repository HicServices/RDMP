using System.Collections.Generic;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.ObjectVisualisation;
using DataExportLibrary.Data.DataTables;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;

namespace CatalogueManager.DataViewing.Collections
{
    public class ViewCustomCohortTableExtractionUICollection : IViewSQLAndResultsCollection
    {
        private Dictionary<string, string> _dictionary;
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }
        public ExtractableCohort ExtractableCohort { get { return (ExtractableCohort)DatabaseObjects[0]; } }
        private const string CustomTableNameKey = "CustomTableName";

        public ViewCustomCohortTableExtractionUICollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
            _dictionary = new Dictionary<string, string>();
        }

        public ViewCustomCohortTableExtractionUICollection(ExtractableCohort cohort, string customTableName) : this()
        {
            DatabaseObjects.Add(cohort);

            _dictionary.Add(CustomTableNameKey, customTableName);
        }

        public string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(_dictionary);
        }

        public void LoadExtraText(string s)
        {
            _dictionary = Helper.LoadDictionaryFromString(s);
        }

        public IHasDependencies GetAutocompleteObject()
        {
            return null;
        }

        public void SetupRibbon(RDMPObjectsRibbonUI ribbon)
        {
            ribbon.Add(ExtractableCohort);
            ribbon.Add(_dictionary[CustomTableNameKey]);
        }

        public IDataAccessPoint GetDataAccessPoint()
        {
            return ExtractableCohort.ExternalCohortTable;
        }

        public string GetSql()
        {
            return ExtractableCohort.GetCustomTableExtractionSQL(_dictionary[CustomTableNameKey], true);
        }

        public string GetTabName()
        {

            return "Top 100 of " + _dictionary[CustomTableNameKey];

        }
    }
}
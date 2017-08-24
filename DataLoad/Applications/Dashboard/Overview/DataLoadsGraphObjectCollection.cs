using System.Collections.Generic;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace Dashboard.Overview
{
    public class DataLoadsGraphObjectCollection : IPersistableObjectCollection
    {
        public PersistStringHelper Helper { get; private set; }
        public List<IMapsDirectlyToDatabaseTable> DatabaseObjects { get; set; }

        public DataLoadsGraphObjectCollection()
        {
            DatabaseObjects = new List<IMapsDirectlyToDatabaseTable>();
            Helper = new PersistStringHelper();
        }

        public string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(new Dictionary<string, string>());
        }

        public void LoadExtraText(string s)
        {
            
        }
    }
}
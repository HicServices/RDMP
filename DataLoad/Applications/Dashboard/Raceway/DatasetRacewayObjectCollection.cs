using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace Dashboard.Raceway
{
    public class DatasetRacewayObjectCollection : PersistableObjectCollection
    {
        public DatasetRaceway.RacewayShowPeriod ShowPeriod { get; set; }
        public bool IgnoreRows { get; set; }

        public DatasetRacewayObjectCollection()
        {
            //default
            ShowPeriod = DatasetRaceway.RacewayShowPeriod.AllTime;
            IgnoreRows = false;
        }

        public Catalogue[] GetCatalogues()
        {
            return DatabaseObjects.Cast<Catalogue>().ToArray();
        }

        public override string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(new Dictionary<string, string>
            {
                {"ShowPeriod", ShowPeriod.ToString()},
                {"IgnoreRows", IgnoreRows.ToString()}
            });
        }

        public override void LoadExtraText(string s)
        {
            var dict = Helper.LoadDictionaryFromString(s);

            //if it's empty we just use the default values we are set up for
            if (dict == null || !dict.Any())
                return;

            ShowPeriod = (DatasetRaceway.RacewayShowPeriod)Enum.Parse(typeof(DatasetRaceway.RacewayShowPeriod), dict["ShowPeriod"], true);
            IgnoreRows = Convert.ToBoolean(dict["IgnoreRows"]);
        }

        public void AddCatalogue(Catalogue catalogue)
        {
            if(catalogue == null)
                throw new ArgumentException("Catalogue must not be null", "catalogue");

            DatabaseObjects.Add(catalogue);
        }

        public void RemoveCatalogue(Catalogue catalogue)
        {
            if(catalogue == null)
                throw new ArgumentException("Catalogue must not be null", "catalogue");

            DatabaseObjects.Remove(catalogue);
        }

        public void ClearDatabaseObjects()
        {
            DatabaseObjects.Clear();
        }
    }
}

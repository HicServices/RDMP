using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using MapsDirectlyToDatabaseTable;

namespace Dashboard.PieCharts
{
    public class GoodBadCataloguePieChartObjectCollection : PersistableObjectCollection
    {
        public CataloguePieChartType PieChartType { get; set; }
        public bool ShowLabels { get; set; }


        public bool IsSingleCatalogueMode{get { return DatabaseObjects.Any(); }}

        public Catalogue GetSingleCatalogueModeCatalogue()
        {
            return (Catalogue) DatabaseObjects.SingleOrDefault();
        }

        public GoodBadCataloguePieChartObjectCollection()
        {
            //default
            PieChartType = CataloguePieChartType.EmptyDescriptions;
        }

        public override string SaveExtraText()
        {
            return Helper.SaveDictionaryToString(new Dictionary<string, string>()
            {
                {"PieChartType", PieChartType.ToString()},
                {"ShowLabels", ShowLabels.ToString()}
            });
        }

        public override void LoadExtraText(string s)
        {
            var dict = Helper.LoadDictionaryFromString(s);

            //if it's empty we just use the default values we are set up for
            if(dict == null || !dict.Any())
                return;

            PieChartType = (CataloguePieChartType)Enum.Parse(typeof(CataloguePieChartType), dict["PieChartType"], true);
            ShowLabels = bool.Parse(dict["ShowLabels"]);
        }

        public void SetAllCataloguesMode()
        {
            DatabaseObjects.Clear();
        }

        public void SetSingleCatalogueMode(Catalogue catalogue)
        {
            if(catalogue == null)
                throw new ArgumentException("Catalogue must not be null to turn on SingleCatalogue mode","catalogue");

            DatabaseObjects.Clear();
            DatabaseObjects.Add(catalogue);
        }
    }
}

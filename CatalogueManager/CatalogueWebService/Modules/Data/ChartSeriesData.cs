using System.Collections.Generic;

namespace CatalogueWebService.Modules.Data
{
    public class ChartSeriesData
    {
        public string Label { get; set; }
        public List<string> Data { get; set; }

        public ChartSeriesData()
        {
            Data = new List<string>();
        }

        public void Add(string val)
        {
            Data.Add(val);
        }
    }
}
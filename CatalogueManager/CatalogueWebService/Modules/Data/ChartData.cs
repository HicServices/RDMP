using System.Collections.Generic;

namespace CatalogueWebService.Modules.Data
{
    public class ChartData
    {
        public List<string> XAxis { get; set; }
        public List<ChartSeriesData> Series { get; set; }

        public ChartData(int numSeries)
        {
            XAxis = new List<string>();

            Series = new List<ChartSeriesData>();
            for (var i = 0; i < numSeries; ++i)
                Series.Add(new ChartSeriesData());
        }

        public void AddX(string val)
        {
            XAxis.Add(val);
        }

        public void AddY(int seriesNum, string val)
        {
            Series[seriesNum].Add(val);
        }
    }
}
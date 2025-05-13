using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.Curation.Data.Datasets.Pure.PureDatasetItem
{
#nullable enable
    /// <summary>
    /// Internal PURE system class
    /// </summary>
    public class PureDate
    {
        public PureDate(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = dateTime.Month;
            Day = dateTime.Day;
        }

        public PureDate() { }


        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month ?? 1, Day ?? 1, 0, 0, 0);
        }

        public bool IsBefore(PureDate date)
        {
            if (Year < date.Year) return true;
            if (Year == date.Year)
            {
                if (Month < date.Month) return true;
                if (Month == date.Month)
                {
                    return Day < date.Day;
                }
            }

            return false;
        }

        public PureDate(int year, int? month = null, int? day = null)
        {
            Year = year;
            if (month != null) Month = month;
            if (day != null) Day = day;
        }
        public int Year { get; set; }
        public int? Month { get; set; }
        public int? Day { get; set; }
    }
}

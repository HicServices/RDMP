using CatalogueLibrary.Data;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// A class that can determine the actual dataset span of a given catalogue e.g. one strategy might be to have the user enter manually the start and end date.  A better
    /// strategy would be to consult the latest Data Quality Engine results to see what the realistic start/end dates are (e.g. discarding outliers / future dates etc)
    /// </summary>
    public interface IDetermineDatasetTimespan
    {
        /// <summary>
        /// Summarises the range of data in the tables that underly the <paramref name="catalogue"/> if known (e.g. based on the last recorded DQE results).
        /// </summary>
        /// <param name="catalogue"></param>
        /// <param name="discardOutliers">True to attempt to throw out outlier rows when determining the dataset timespan</param>
        /// <returns></returns>
        string GetHumanReadableTimepsanIfKnownOf(Catalogue catalogue, bool discardOutliers);
    }
}
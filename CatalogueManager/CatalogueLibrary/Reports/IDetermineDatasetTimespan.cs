using CatalogueLibrary.Data;

namespace CatalogueLibrary.Reports
{
    /// <summary>
    /// A class that can determine the actual dataset span of a given catalogue e.g. one strategy might be to have the user enter manually the start and end date.  A better
    /// strategy would be to consult the latest Data Quality Engine results to see what the realistic start/end dates are (e.g. discarding outliers / future dates etc)
    /// </summary>
    public interface IDetermineDatasetTimespan
    {
        string GetHumanReadableTimepsanIfKnownOf(Catalogue catalogue, bool discardOutliers);
    }
}
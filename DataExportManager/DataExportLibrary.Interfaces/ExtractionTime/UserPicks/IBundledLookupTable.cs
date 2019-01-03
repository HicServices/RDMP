using CatalogueLibrary.Data;

namespace DataExportLibrary.Interfaces.ExtractionTime.UserPicks
{
    /// <summary>
    /// See BundledLookupTable
    /// </summary>
    public interface IBundledLookupTable
    {
        ITableInfo TableInfo { get; set; }
    }
}
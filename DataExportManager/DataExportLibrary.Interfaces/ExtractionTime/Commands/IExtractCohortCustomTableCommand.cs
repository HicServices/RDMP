using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
    /// <summary>
    /// See ExtractCohortCustomTableCommand
    /// </summary>
    public interface IExtractCohortCustomTableCommand : IExtractCommand
    {
        IExtractableCohort ExtractableCohort { get; set; }
        string TableName { get; set; }

    }
}
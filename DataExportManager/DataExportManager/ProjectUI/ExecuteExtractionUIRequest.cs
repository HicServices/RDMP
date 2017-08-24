using CatalogueLibrary.Data.Pipelines;
using DataExportLibrary.Data.DataTables;

namespace DataExportManager.ProjectUI
{
    public class ExecuteExtractionUIRequest
    {
        public ExtractionConfiguration ExtractionConfiguration { get; set; }

        public ExecuteExtractionUIRequest(ExtractionConfiguration extractionConfiguration)
        {
            ExtractionConfiguration = extractionConfiguration;
        }

        public bool AutoStart { get; set; }
    }
}
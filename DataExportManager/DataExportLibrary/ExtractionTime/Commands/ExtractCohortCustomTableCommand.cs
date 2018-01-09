using System.IO;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.ExtractionTime.Commands
{
    /// <summary>
    /// Command representing a desire to extract a given Custom Data table along with the main datasets that make up a researchers ExtractionConfiguration.
    /// This is an input object for the extraction pipeline (See IExtractCommand) and different sources/destinations could/will handle this request differently
    /// but typically the table is linked with the cohort and extracted (with release identifier substitution) to the ExtractionDirectory.
    /// </summary>
    public class ExtractCohortCustomTableCommand : IExtractCohortCustomTableCommand
    {
        private ExtractionDirectory _extractionDirectory;
        public IExtractableCohort ExtractableCohort { get; set; }
        public string TableName { get; set; }
        public ExtractCommandState State { get; set; }
        public string Name { get { return TableName; } }

        public ExtractCohortCustomTableCommand(ExtractionConfiguration configuration,IExtractableCohort extractableCohort, string tableName)
        {
            Configuration = configuration;
            ExtractableCohort = extractableCohort;
            TableName = tableName;

            _extractionDirectory = new ExtractionDirectory(Configuration.Project.ExtractionDirectory, Configuration);
        }

        public override string ToString()
        {
            return TableName;
        }

        public DirectoryInfo GetExtractionDirectory()
        {
            return _extractionDirectory.GetDirectoryForCohortCustomData();
        }

        public IExtractionConfiguration Configuration { get; set; }
        public string DescribeExtractionImplementation()
        {
            return ExtractableCohort.GetCustomTableExtractionSQL(TableName);
        }
    }
}
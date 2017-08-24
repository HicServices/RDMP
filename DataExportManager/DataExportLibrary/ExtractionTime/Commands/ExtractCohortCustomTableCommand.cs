using System.IO;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime.Commands;
using DataExportLibrary.Data.DataTables;

namespace DataExportLibrary.ExtractionTime.Commands
{
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
using System.IO;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.Interfaces.ExtractionTime.Commands
{
    public abstract class ExtractCommand:IExtractCommand
    {
        public IProject Project { get; private set; }
        public IExtractionConfiguration Configuration { get; private set; }

        protected ExtractCommand(IExtractionConfiguration configuration)
        {
            Configuration = configuration;

            //needed for ExtractDatasetCommand.EmptyCommand
            if(configuration != null)
                Project = configuration.Project;
        }

        public abstract DirectoryInfo GetExtractionDirectory();
        
        public abstract string DescribeExtractionImplementation();
        public ExtractCommandState State { get; private set; }
        
        public void ElevateState(ExtractCommandState newState)
        {
            if (State < newState)
                State = newState;
        }
    }
}
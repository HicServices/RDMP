using System.IO;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    public class FlatFileToLoad
    {
        public FlatFileToLoad(FileInfo file)
        {
            File = file;
        }

        public FileInfo File { get; set; }
    }
}
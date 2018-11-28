using System.IO;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    /// <summary>
    /// Wrapper for FileInfo that can be used in the IPipelineRequirement interface to indicate that component expects a FileInfo that is specifically going to have data loaded
    /// out of it.  Having an IPipelineRequirement for a FileInfo on a component could be confusing, we might also want to allow multiple different types of FileInfo.  Having
    /// this wrapper ensures that there is no confusion about what a FlatFileToLoad Initialization Object is for. 
    /// </summary>
    public class FlatFileToLoad
    {
        /// <summary>
        /// Creates a new instance pointed at the given <paramref name="file"/>
        /// </summary>
        /// <param name="file"></param>
        public FlatFileToLoad(FileInfo file)
        {
            File = file;
        }

        /// <summary>
        /// The file you are trying to load
        /// </summary>
        public FileInfo File { get; set; }
        
        /// <summary>
        /// Returns the filename of the file you are trying to load
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (File == null)
                return base.ToString();

            return File.Name;
        }
    }
}
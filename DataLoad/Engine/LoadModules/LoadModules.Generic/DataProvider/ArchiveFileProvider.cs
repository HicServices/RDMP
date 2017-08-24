using System;
using System.IO;
using System.IO.Compression;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;

namespace LoadModules.Generic.DataProvider
{
    public class ArchiveFileProvider : FileProvider
    {
        public ArchiveFileProvider(string sourceFilepath, string destDir) : base(sourceFilepath, destDir)
        {
        }

        public override ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            var files = SourceFilepath.Split(new[] {';'});
            try
            {
                foreach (var filepath in files)
                {
                    OnFileBeingCreated(job,filepath, false, 0, TimeSpan.Zero); 
                    ZipFile.ExtractToDirectory(filepath, DestDir);
                }
            }
            catch (Exception e)
            {
                throw new FileLoadException("Could not unarchive " + SourceFilepath + " to " + DestDir + ": " + e);
            }

            return ExitCodeType.Success;
        }
    }
}
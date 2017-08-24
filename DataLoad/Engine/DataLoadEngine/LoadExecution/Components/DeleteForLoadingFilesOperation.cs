using System.IO;
using System.Linq;
using CatalogueLibrary;
using DataLoadEngine.Job;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components
{
    public class DeleteForLoadingFilesOperation : IDisposeAfterDataLoad
    {
        private readonly IDataLoadJob _job;

        public DeleteForLoadingFilesOperation(IDataLoadJob job)
        {
            _job = job;
        }

        
        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            // We only delete ForLoading files after a successful load
            if (exitCode == ExitCodeType.Success)
            {
                var hicProjectDirectory = _job.HICProjectDirectory;

                //if there are no files and there are no directories
                if (!hicProjectDirectory.ForLoading.GetFiles().Any() && !hicProjectDirectory.ForLoading.GetDirectories().Any())
                {
                    //just skip it but tell user you are skipping it
                    postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "No files found in ForLoading so not bothering to try and delete."));
                    return;  
                }

                // Check if the attacher has communicated its intent to handle archiving
                var archivingHandledByAttacher = File.Exists(Path.Combine(hicProjectDirectory.ForLoading.FullName, "attacher_is_handling_archiving"));
                    
                if (!archivingHandledByAttacher && !ArchiveHasBeenCreated())
                {
                    postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error, "Refusing to delete files in ForLoading: the load has reported success but there is no archive of this dataset (was expecting the archive to be called '" + _job.ArchiveFilepath + "', check LoadMetadata.CacheArchiveType if the file extension is not what you expect)"));
                    return;   
                }

                _job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Deleting files in ForLoading (" + hicProjectDirectory.ForLoading.FullName + ")"));
                
                if (archivingHandledByAttacher)
                {
                    hicProjectDirectory.ForLoading.EnumerateFiles().Where(info => info.Name != "attacher_is_handling_archiving").ToList().ForEach(info => info.Delete());
                    hicProjectDirectory.ForLoading.EnumerateDirectories().Where(info => info.Name != "__hidden_from_archiver__").ToList().ForEach(info => info.Delete(true));
                }
                else
                {
                    hicProjectDirectory.ForLoading.EnumerateFiles().ToList().ForEach(info => info.Delete());
                    hicProjectDirectory.ForLoading.EnumerateDirectories().ToList().ForEach(info => info.Delete(true));
                }
            }
        }

        private bool ArchiveHasBeenCreated()
        {
            return new FileInfo(_job.ArchiveFilepath).Exists;
        }
    }
}
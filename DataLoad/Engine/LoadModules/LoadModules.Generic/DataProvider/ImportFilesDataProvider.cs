using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataProvider
{
    [Description("Copies files from the DirectoryPath into the forLoading directory.  If the data load is successful")]
    public class ImportFilesDataProvider : IPluginDataProvider
    {
        private FileInfo[] _files;

        [DemandsInitialization("The path you want to copy files from", Mandatory = true)]
        public string DirectoryPath { get; set; }

        [DemandsInitialization("The file pattern to match on the DirectoryPath", Mandatory = true)]
        public string FilePattern { get; set; }

        [DemandsInitialization("If true then at the end of a successful data load the files that were originally matched and copied to forLoading will be deleted from the remote DirectoryPath.  Note that only the files copied will be deleted, any new files that appear during the load will not be deleted")]
        public bool DeleteFilesOnsuccessfulLoad { get; set; }

        public void Check(ICheckNotifier notifier)
        {

            if (string.IsNullOrWhiteSpace(DirectoryPath))
                notifier.OnCheckPerformed(new CheckEventArgs("No DirectoryPath has been specified, this should be set to the remote folder you want to copy files out of",CheckResult.Fail));

            if (string.IsNullOrWhiteSpace(FilePattern))
                notifier.OnCheckPerformed(new CheckEventArgs("No FilePattern has been specified, this should be a pattern that matches files in the remote folder you want to copy files out of e.g. *.*", CheckResult.Fail));

            if (new DirectoryInfo(DirectoryPath).Exists)
                notifier.OnCheckPerformed(new CheckEventArgs("Path " + DirectoryPath + " was found", CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Path " + DirectoryPath + " was not found",CheckResult.Fail));
        }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            _files = new DirectoryInfo(DirectoryPath).GetFiles(FilePattern);

            foreach (FileInfo f in _files)
            {
                var to = Path.Combine(job.HICProjectDirectory.ForLoading.FullName, f.Name);
                job.OnNotify(this,new NotifyEventArgs(ProgressEventType.Information, "Copying file " + f.FullName + " to directory " + to));
                f.CopyTo(to,true);
            }

            return ExitCodeType.Success;
        }

        public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
        {
            if (exitCode == ExitCodeType.Success)
                if (DeleteFilesOnsuccessfulLoad)
                    foreach (FileInfo f in _files)
                    {
                        postLoadEventsListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "About to delete " + f.FullName));
                        f.Delete();
                    }
        }

        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        public IDataProvider Clone()
        {
            throw new NotImplementedException();
        }

        public bool Validate(IHICProjectDirectory destination)
        {
            return true;
        }
    }
}

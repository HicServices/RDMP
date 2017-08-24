using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace LoadModules.Generic.FileOperations
{
    [Description(
        @"Searches the forLoading directory for *.zip and unzips all entries in all zip archives found.  If the forLoading directory already contains a file with the same name then it is overwritten (unless the file size is also the same in which case the entry is skipped) ")]
    public class FileUnzipper : IPluginDataProvider
    {
        [DemandsInitialization("Leave blank to extract all zip archives or populate with a REGULAR EXPRESSION to extract only specific zip filenames e.g. \"nhs_readv2*\\.zip\" - notice the escaped dot to match absoltely the dot bit")]
        public Regex ZipArchivePattern { get; set; }
        
        [DemandsInitialization("Leave blank to extract all files or populate with a REGULAR EXPRESSION to extract only specific files e.g. \".*\\.txt\" to extract all .txt files - notice how the pattern is a regular expression, so the dot must be escaped to prevent matching anything")]
        public Regex ZipEntryPattern { get; set; }

        readonly List<FileInfo> _entriesUnzipped = new List<FileInfo>();

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            foreach (FileInfo fileInfo in job.HICProjectDirectory.ForLoading.GetFiles("*.zip"))
            {
                //do it as regex rather than in GetFiles above because that method probably doesn't do regex
                if (ZipArchivePattern == null || string.IsNullOrWhiteSpace(ZipArchivePattern.ToString()) || ZipArchivePattern.IsMatch(fileInfo.Name))
                    using (ZipArchive zipFile = ZipFile.Open(fileInfo.FullName,ZipArchiveMode.Read))
                    {
                        //fire event telling user we found some files in the zip file 
                        job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, zipFile.Entries.Aggregate("Identified the following zip entries:",(s,n)=>n.Name +",").TrimEnd(',')));


                        foreach (var entry in zipFile.Entries)
                        {
                            if(entry.Length == 0)
                                continue;
                            
                            //if we are matching everything or we are matching on a regex that matches the entry name
                            if (ZipEntryPattern == null || string.IsNullOrWhiteSpace(ZipEntryPattern.ToString()) || ZipEntryPattern.IsMatch(entry.Name))
                            {
                                //extract it
                                FileInfo existingFile = job.HICProjectDirectory.ForLoading.GetFiles(entry.Name).FirstOrDefault();
                        
                                if(existingFile != null && existingFile.Length == entry.Length)
                                    continue;

                                UnzipWithEvents(entry, job.HICProjectDirectory,job);
                            }
                        }
                    }
            }

            return ExitCodeType.Success;
        }

        private void UnzipWithEvents(ZipArchiveEntry entry, IHICProjectDirectory destination, IDataLoadJob job)
        {
            //create a task 
            string entryDestination = Path.Combine(destination.ForLoading.FullName, entry.Name);
            Task unzipJob = Task.Factory.StartNew(() => entry.ExtractToFile(entryDestination, true));
            
            //create a stopwatch to time how long bits take
            Stopwatch s = new Stopwatch();
            s.Start();


            FileInfo f = new FileInfo(entryDestination);
            _entriesUnzipped.Add(f);

            //monitor it
            while (!unzipJob.IsCompleted)
            {
                Thread.Sleep(200);
                if(f.Exists)
                    job.OnProgress(this,new ProgressEventArgs(entryDestination,new ProgressMeasurement((int)(f.Length / 1000),ProgressType.Kilobytes), s.Elapsed));
            }
            s.Stop();


        }

        public string GetDescription()
        {
            throw new NotImplementedException();
        }

        public IDataProvider Clone()
        {
            return new FileUnzipper();
        }
        

        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
            if (exitCode == ExitCodeType.Success || exitCode == ExitCodeType.OperationNotRequired)
            {
                int countOfEntriesThatDisapeared = _entriesUnzipped.Count(e=>!e.Exists);

                if (countOfEntriesThatDisapeared != 0)
                    postLoadEventListener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning,
                        countOfEntriesThatDisapeared + " of " + _entriesUnzipped.Count + " entries were created by " +
                        GetType().Name +
                        " during unzip phase but had disapeared at cleanup time - following successful data load"));

                //cleanup required
                foreach (FileInfo f in _entriesUnzipped.Where(e => e.Exists))
                    try
                    {
                        f.Delete();
                    }
                    catch (Exception e)
                    {
                        postLoadEventListener.OnNotify(this,new NotifyEventArgs(ProgressEventType.Warning, "Could not delete file " + f.FullName , e));
                    }
            }
        }

        
        public void Check(ICheckNotifier notifier)
        {
            if (ZipArchivePattern != null)
                notifier.OnCheckPerformed(new CheckEventArgs("Found ZipArchivePattern " + ZipArchivePattern,CheckResult.Success));

            if (ZipEntryPattern != null)
                notifier.OnCheckPerformed(new CheckEventArgs("Found ZipEntryPattern " + ZipEntryPattern, CheckResult.Success));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Tar;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataProvider.FromCache
{
    internal interface IArchivedFileExtractor
    {
        void Extract(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory, IDataLoadEventListener dataLoadJob);
    }

    internal abstract class ArchiveExtractor : IArchivedFileExtractor
    {
        private readonly string _extension;
        protected abstract void DoExtraction(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory, IDataLoadEventListener dataLoadJob);

        protected ArchiveExtractor(string extension)
        {
            _extension = extension;
        }

        public void Extract(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory, IDataLoadEventListener dataLoadJob)
        {
            //ensure it is a legit zip file
            if (!job.Value.Extension.Equals(_extension))
                throw new NotSupportedException("Unexpected job file extension -" + job.Value.Extension + " (expected " + _extension + ")");

            //tell the UI/listener that we have identified the archive
            dataLoadJob.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Archive identified:" + job.Value.FullName));

            try
            {
                DoExtraction(job, destinationDirectory, dataLoadJob);
            }
            catch (Exception ex)
            {
                throw new Exception("Error occurred extracting zip archive " + job.Value.FullName, ex);
            }
        }
    }

    internal class ZipExtractor : ArchiveExtractor
    {
        public ZipExtractor() : base(".zip")
        {
        }


        protected override void DoExtraction(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory, IDataLoadEventListener dataLoadEventListener)
        {
            var archive = new ZipArchive(new FileStream(job.Value.FullName, FileMode.Open));
            archive.ExtractToDirectory(destinationDirectory.FullName);
            archive.Dispose();
        }
    }

    internal class TarExtractor : ArchiveExtractor
    {
        public TarExtractor() : base(".tar")
        {
        }

        protected override void DoExtraction(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory, IDataLoadEventListener dataLoadJob)
        {
            using (var archive = TarArchive.CreateInputTarArchive(new FileStream(job.Value.FullName, FileMode.Open)))
                archive.ExtractContents(destinationDirectory.FullName);
        }
    }
}

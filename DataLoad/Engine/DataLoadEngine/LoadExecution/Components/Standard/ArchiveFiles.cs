using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.Job;
using DataLoadEngine.LoadProcess;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.LoadExecution.Components.Standard
{
    /// <summary>
    /// Copies all files in ForLoading directory of a DLE load into the ForArchiving folder zipped up in a file named x.zip where x is the ID of the data load run
    /// (unique logging number for the data load execution).
    /// </summary>
    public class ArchiveFiles : DataLoadComponent
    {
        public static string TempArchiveDirName = "__temp_for_archiving__";
        public static string HiddenFromArchiver = "__hidden_from_archiver__";

        public ArchiveFiles(HICLoadConfigurationFlags loadConfigurationFlags)
        {
            Description = "Archive";
            SkipComponent = !loadConfigurationFlags.ArchiveData;
        }

        public override ExitCodeType Run(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (Skip(job)) 
                return ExitCodeType.Success;

            var datasetID = job.DataLoadInfo.ID;
            var destFile = Path.Combine(job.HICProjectDirectory.ForArchiving.FullName, datasetID + ".zip");
            
            // If there is nothing in the forLoadingDirectory then 
            // There may be a HiddenFromArchiver directory with data that may be processed by another component, but this component should *always* archive *something* even if it is just some metadata about the load (if, for example, imaging data is being loaded which is too large to archive)
            if (!FoundFilesOrDirsToArchive(job))
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning, "There is nothing to archive: " + job.HICProjectDirectory.ForLoading.FullName + " is empty after completion of the load process and there is no hidden archive directory (" + HiddenFromArchiver + ")."));
                return ExitCodeType.Success;
            }

            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, "Archiving to " + destFile));

            if (File.Exists(destFile))
                throw new Exception("Cannot archive files, " + destFile + " already exists");

            // create directory for zipping, leaving out __hidden_from_archiver__
            var zipDir = job.HICProjectDirectory.ForLoading.CreateSubdirectory(TempArchiveDirName);

            MoveDirectories(job, zipDir);
            MoveFiles(job, zipDir);

            ZipFile.CreateFromDirectory(zipDir.FullName, destFile);

            return ExitCodeType.Success;
        }

        static readonly string[] DirsToIgnore = { TempArchiveDirName, HiddenFromArchiver };

        private static bool FoundFilesOrDirsToArchive(IDataLoadJob job)
        {
            //if there are any files
            if (job.HICProjectDirectory.ForLoading.EnumerateFiles().Any())
                return true;

            //or any directories that are not directories we should be ignoring
            return job.HICProjectDirectory.ForLoading.EnumerateDirectories().Any(d => !DirsToIgnore.Contains(d.Name));
        }

        private static void MoveDirectories(IDataLoadJob job, DirectoryInfo zipDir)
        {
            var dirsToMove = job.HICProjectDirectory.ForLoading.EnumerateDirectories().Where(info => !DirsToIgnore.Contains(info.Name)).ToList();
            foreach (var toMove in dirsToMove)
                toMove.MoveTo(Path.Combine(zipDir.FullName, toMove.Name));
        }

        private static void MoveFiles(IDataLoadJob job, DirectoryInfo zipDir)
        {
            var filesToMove = job.HICProjectDirectory.ForLoading.EnumerateFiles().ToList();
            foreach (var toMove in filesToMove)
                toMove.MoveTo(Path.Combine(zipDir.FullName, toMove.Name));
        }

        public override void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
        }
    }
}
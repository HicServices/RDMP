using System;
using System.IO;

namespace RDMPStartup.Events
{
    /// <summary>
    /// EventArgs for MEF downloading during Startup.cs
    /// 
    /// <para>Records whether the file was succesfully downloaded and the number of dlls saved so far.</para>
    /// </summary>
    public class MEFFileDownloadProgressEventArgs
    {
        public MEFFileDownloadProgressEventArgs(DirectoryInfo downloadDirectory, int dllsSeenInCatalogue, int currentDllNumber, string fileBeingProcessed, bool includesPdbFile,MEFFileDownloadEventStatus status, Exception exception=null)
        {
            DownloadDirectory = downloadDirectory;
            DllsSeenInCatalogue = dllsSeenInCatalogue;
            CurrentDllNumber = currentDllNumber;
            FileBeingProcessed = fileBeingProcessed;
            IncludesPdbFile = includesPdbFile;
            Status = status;
            Exception = exception;
        }

        public DirectoryInfo DownloadDirectory { get; set; }
        
        public int DllsSeenInCatalogue { get; set; }
        public int CurrentDllNumber { get; set; }

        public MEFFileDownloadEventStatus Status { get; set; }

        public string FileBeingProcessed { get; set; } 
        public bool IncludesPdbFile{get; set;} 
        public Exception Exception { get; set; }

    }
}

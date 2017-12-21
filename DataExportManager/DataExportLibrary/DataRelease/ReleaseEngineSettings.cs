using System;
using System.ComponentModel;
using System.IO;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.DataRelease
{
    /// <summary>
    /// Options for configuring ReleaseEngine behaviour (To change where files are released to etc)
    /// </summary>
    public class ReleaseEngineSettings 
    {
        [DemandsInitialization("Specify a custom Release folder, will use the Project Extraction Folder if left empty")]
        public DirectoryInfo CustomReleaseFolder { get; set; }

        [DemandsInitialization("If unchecked, it will report an error if the destination folder does not exists", DefaultValue = true)]
        public bool CreateReleaseDirectoryIfNotFound { get; set; }
        
        [DemandsInitialization("Delete the released files from the origin location if release is succesful", DefaultValue = true)]
        public bool DeleteFilesOnSuccess { get; set; }

        public ReleaseEngineSettings()
        {
            CreateReleaseDirectoryIfNotFound = true;
            DeleteFilesOnSuccess = true;
        }
    }
}
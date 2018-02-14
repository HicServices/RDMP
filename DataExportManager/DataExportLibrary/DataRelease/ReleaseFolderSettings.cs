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
    public class ReleaseFolderSettings:ICheckable
    {
        [DemandsInitialization("Specify a custom Release folder, will use the Project Extraction Folder if left empty")]
        public DirectoryInfo CustomReleaseFolder { get; set; }

        [DemandsInitialization("If unchecked, it will report an error if the destination folder does not exists", DefaultValue = true)]
        public bool CreateReleaseDirectoryIfNotFound { get; set; }
        
        public ReleaseFolderSettings()
        {
            CreateReleaseDirectoryIfNotFound = true;
        }

        public void Check(ICheckNotifier notifier)
        {
            if (CustomReleaseFolder != null)
                notifier.OnCheckPerformed(new CheckEventArgs("Custom Release folder is:" + CustomReleaseFolder, CheckResult.Success));
            else
                notifier.OnCheckPerformed(new CheckEventArgs("Release folder will be the project extraction folder", CheckResult.Success));
        }
    }
}
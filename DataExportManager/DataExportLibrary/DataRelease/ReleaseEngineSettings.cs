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
    public class ReleaseEngineSettings:ICheckable
    {
        [DemandsInitialization("Delete the released files from the origin location if release is succesful", DefaultValue = true)]
        public bool DeleteFilesOnSuccess { get; set; }

        public ReleaseEngineSettings()
        {
            DeleteFilesOnSuccess = true;
        }

        public void Check(ICheckNotifier notifier)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("All green!",CheckResult.Success));
        }
    }
}
using System;
using System.IO;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline;
using DataLoadEngine.DataProvider;
using DataLoadEngine.Job;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.DatabaseHelpers.Discovery;
using ReusableLibraryCode.Progress;

namespace LoadModules.Generic.DataProvider
{
    public class FileProvider : IDataProvider
    {
        public string SourceFilepath { get; set; }
        public string DestDir { get; set; }

        public FileProvider(string sourceFilepath, string destDir)
        {
            SourceFilepath = sourceFilepath;
            DestDir = destDir;
        }

        public IDataProvider Clone()
        {
            return new FileProvider(SourceFilepath, DestDir);
        }

        public bool Validate(IHICProjectDirectory destination)
        {
            if (string.IsNullOrWhiteSpace(SourceFilepath))
                throw new Exception("No SourceFilepath has been configured");

            if (SourceFilepath.Contains(";"))
            {
                foreach (var filepath in SourceFilepath.Split(new[] {';'}))
                {
                    if (!ValidateSingleFilepath(filepath))
                        return false;
                }
                return true;
            }
            
            return ValidateSingleFilepath(SourceFilepath);
        }

        private bool ValidateSingleFilepath(string filepath)
        {
            if (!File.Exists(filepath))
                throw new Exception("The file at " + filepath + " does not exist");

            return true;
        }

        protected void PreFetchChecks(string destFilepath)
        {
            if (!File.Exists(SourceFilepath))
                throw new FileLoadException("The source file does not exist", SourceFilepath);

            if (!Directory.Exists(DestDir))
                throw new DirectoryNotFoundException("The destination dir '" + DestDir + "' does not exist");
        }

        public void Initialize(IHICProjectDirectory hicProjectDirectory, DiscoveredDatabase dbInfo)
        {
            
        }

        virtual public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
        {
            if (SourceFilepath == null)
                throw new FileLoadException("Could not get filename from " + SourceFilepath);

            var destFilepath = Path.Combine(DestDir, Path.GetFileName(SourceFilepath));
            if (File.Exists(destFilepath))
                throw new FileLoadException("The destination file '" + destFilepath + "'exists, overwriting is not permitted", SourceFilepath);

            try
            {
                PreFetchChecks(destFilepath);
                File.Copy(SourceFilepath, destFilepath);
            }
            catch (Exception)
            {
                throw new FileLoadException("Could not copy " + SourceFilepath + " to " + destFilepath);
            }

            return ExitCodeType.Success;
        }

        public string GetDescription()
        {
            return SourceFilepath;
        }
        
        protected virtual void OnFileBeingCreated(IDataLoadEventListener listener, string filepath, bool iscompleted, int currentfilesizeinkb, TimeSpan elapsedtime)
        {
            listener.OnProgress(this, new ProgressEventArgs(filepath, new ProgressMeasurement(currentfilesizeinkb, ProgressType.Kilobytes), elapsedtime));
        }

        

        public void LoadCompletedSoDispose(ExitCodeType exitCode,IDataLoadEventListener postLoadEventListener)
        {
        }

        
        public void Check(ICheckNotifier notifier)
        {
            
        }
    }
}

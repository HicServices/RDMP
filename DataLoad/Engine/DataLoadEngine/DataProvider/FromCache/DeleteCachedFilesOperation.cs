using System;
using System.Collections.Generic;
using System.IO;
using CatalogueLibrary;
using DataLoadEngine.Job.Scheduling;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.DataProvider.FromCache
{
    public class DeleteCachedFilesOperation : UpdateProgressIfLoadsuccessful
    {
        private readonly Dictionary<DateTime, FileInfo> _cacheFileMappings;

        public DeleteCachedFilesOperation(ScheduledDataLoadJob job, Dictionary<DateTime, FileInfo> cacheFileMappings)
            : base(job)
        {
            _cacheFileMappings = cacheFileMappings;
        }

        override public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
        {
            if (exitCode != ExitCodeType.Success)
                return;

            base.LoadCompletedSoDispose(exitCode, postLoadEventListener);

            foreach (KeyValuePair<DateTime, FileInfo> keyValuePair in _cacheFileMappings)
            {
                if (keyValuePair.Value == null)
                    continue;

                try
                {
                    keyValuePair.Value.Delete();
                }
                catch (IOException e)
                {
                    Job.LogWarning(GetType().FullName, "Could not delete cached file " + keyValuePair.Value + " (" + e.Message + ")make sure to delete it manually otherwise Schedule and file system will be desynched");
                }
            }
        }
    }
}
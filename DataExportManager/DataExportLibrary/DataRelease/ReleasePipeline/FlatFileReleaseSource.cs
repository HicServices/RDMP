using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.ExtractionTime;
using DataExportLibrary.Interfaces.Data.DataTables;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using Ticketing;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class FlatFileReleaseSource<T> : FixedReleaseSource<ReleaseAudit>
    {
        private bool firstTime = true;

        public override ReleaseAudit GetChunk(IDataLoadEventListener listener, GracefulCancellationToken cancellationToken)
        {
            if (firstTime)
            {
                firstTime = false;
                return flowData ?? new ReleaseAudit()
                {
                    SourceGlobalFolder = PrepareSourceGlobalFolder()
                };
            }

            return null;
        }

        private DirectoryInfo PrepareSourceGlobalFolder()
        {
            var globalDirectoriesFound = new List<DirectoryInfo>();

            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> releasePotentials in _releaseData.ConfigurationsForRelease)
                globalDirectoriesFound.AddRange(GetAllGlobalFolders(releasePotentials));

            if (globalDirectoriesFound.Any())
            {
                var firstGlobal = globalDirectoriesFound.First();

                foreach (var directoryInfo in globalDirectoriesFound.Distinct(new DirectoryInfoComparer()))
                {
                    UsefulStuff.GetInstance().ConfirmContentsOfDirectoryAreTheSame(firstGlobal, directoryInfo);
                }

                return firstGlobal;
            }

            return null;
        }

        public override void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
        {
            firstTime = true;
        }

        public override void Abort(IDataLoadEventListener listener)
        {
            firstTime = true;
        }

        protected override void RunSpecificChecks(ICheckNotifier notifier)
        {
            
        }

        private IEnumerable<DirectoryInfo> GetAllGlobalFolders(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            const string folderName = ExtractionDirectory.GlobalsDataFolderName;

            foreach (ReleasePotential releasePotential in toRelease.Value)
            {
                Debug.Assert(releasePotential.ExtractDirectory.Parent != null, "releasePotential.ExtractDirectory.Parent != null");
                DirectoryInfo globalFolderForThisExtract = releasePotential.ExtractDirectory.Parent.EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly).SingleOrDefault();

                if (globalFolderForThisExtract == null) //this particualar release didn't include globals/custom data at all
                    continue;

                yield return (globalFolderForThisExtract);
            }
        }
    }
}
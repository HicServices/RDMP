using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using CachingEngine.BasicCache;
using CachingEngine.Factories;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Progress;

namespace CachingEngineTests.Unit
{
    public interface IFileDataFlowDestination : TestIFileDataFlowComponent
    {
    }

    public interface TestIFileDataFlowComponent
    {
        IList<FileInfo> ProcessPipelineData(IList<FileInfo> toProcess, IDataLoadEventListener listener);
        void Dispose(IDataLoadEventListener listener);
    }

    // this is really a destination?
    public class MoveToDirectory : IFileDataFlowDestination
    {
        public DirectoryInfo DestinationDirectory { get; set; }

        public IList<FileInfo> ProcessPipelineData(IList<FileInfo> toProcess, IDataLoadEventListener listener)
        {
            if (DestinationDirectory.Parent == null)
                throw new Exception("The destination directory has no parent so a new set of filepaths cannot be created.");

            var movedFiles = new List<FileInfo>();
            foreach (var fileInfo in toProcess.ToList())
            {
                var filePath = Path.Combine(DestinationDirectory.Parent.FullName, "...?");
                fileInfo.MoveTo(filePath);
                movedFiles.Add(new FileInfo(filePath));
            }

            return movedFiles;
        }

        public void Dispose(IDataLoadEventListener listener)
        {
            throw new System.NotImplementedException();
        }
    }

    public class FilesystemCacheDestination : IFileDataFlowDestination, IPipelineRequirement<CacheProgress>, IPipelineRequirement<DirectoryInfo>
    {
        public CacheProgress CacheProgress { get; set; }
        public DirectoryInfo CacheDirectory { get; set; }

        public void UseContainer()
        {
            var catalog = new DirectoryCatalog(".");
            var container = new CompositionContainer(catalog);
            
            var cacheProgress = new CacheProgress(null, (ILoadProgress) null);
            container.ComposeExportedValue(cacheProgress);

            var directoryInfo = new DirectoryInfo(".");
            container.ComposeExportedValue(directoryInfo);

            var component = container.GetExportedValue<FilesystemCacheDestination>();
        }

        public IList<FileInfo> ProcessPipelineData(IList<FileInfo> toProcess, IDataLoadEventListener listener)
        {
            var layout = new ZipCacheLayoutOnePerDay(CacheDirectory, new NoSubdirectoriesCachePathResolver());

            var moveComponent = new MoveToDirectory
            {
                DestinationDirectory = layout.GetLoadCacheDirectory(listener)
            };

            moveComponent.ProcessPipelineData(toProcess, listener);

            // would be in CacheLayout, with it being a component
            // ? where does the date come from?
            // either going to be CacheFillProgress or CacheFillProgress + period, depending on fetch logic
            if (CacheProgress.CacheFillProgress == null)
                throw new Exception("Should throw, but currently on first cache it is valid for the CacheFIllProgress to be null");
            

            return toProcess;
        }

        public void Dispose(IDataLoadEventListener listener)
        {
            throw new NotImplementedException();
        }

        public void PreInitialize(CacheProgress cacheProgress, IDataLoadEventListener listener)
        {
            CacheProgress = cacheProgress;
        }

        public void PreInitialize(DirectoryInfo cacheDirectory, IDataLoadEventListener listener)
        {
            CacheDirectory = cacheDirectory;
        }

    }
}
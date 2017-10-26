using System;
using System.Collections.Generic;
using System.IO;
using CachingEngine.PipelineExecution.Destinations;
using CatalogueLibrary.Data.DataLoad;
using ReusableLibraryCode.Progress;

namespace CachingEngine.Layouts
{
    // 'static' information about the cache layout, as opposed to the resolver which will give information for specific cache configurations
    // Cache layout is effectively based on date with load schedule-specific sub directories with dataset-specific layout information provided through the Resolver
    public interface ICacheLayout
    {
        //Readonly fields you should set in your constructor
        string DateFormat { get; }
        CacheArchiveType ArchiveType { get; }
        CacheFileGranularity CacheFileGranularity { get; }
        
        //Consider taking these as parameters to your constructor - see CacheLayout abstract class for how you should probably implement this interface
        ILoadCachePathResolver Resolver {  get; }
        DirectoryInfo RootDirectory { get; }
        
        // some interface for looking up filename
        DateTime? GetEarliestDateToLoadAccordingToFilesystem(IDataLoadEventListener listener); 
        DateTime? GetMostRecentDateToLoadAccordingToFilesystem(IDataLoadEventListener listener);
        void CreateIfNotExists(IDataLoadEventListener listener);
        bool CheckExists(DateTime archiveDate, IDataLoadEventListener listener);
        FileInfo GetArchiveFileInfoForDate(DateTime archiveDate, IDataLoadEventListener listener);
        DirectoryInfo GetLoadCacheDirectory(IDataLoadEventListener listener);

        // requested by DLE
        Queue<DateTime> GetSortedDateQueue(IDataLoadEventListener listener);
        bool CheckCacheFilesAvailability(IDataLoadEventListener listener);
        
    }
}
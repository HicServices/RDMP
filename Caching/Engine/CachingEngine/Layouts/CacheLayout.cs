using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.ServiceModel.PeerResolvers;
using CachingEngine.PipelineExecution.Destinations;
using CachingEngine.Requests;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Data.DataLoad;
using ICSharpCode.SharpZipLib.Tar;

namespace CachingEngine.Layouts
{
    public abstract class CacheLayout : ICacheLayout
    {
        public string DateFormat { get; private set; }
        public CacheArchiveType ArchiveType { get;private set; }
        public CacheFileGranularity CacheFileGranularity { get;private set; }
        public ILoadCachePathResolver Resolver {  get; private set;}
        public DirectoryInfo RootDirectory { get; private set; }

        public CacheLayout(DirectoryInfo rootDirectory, string dateFormat, CacheArchiveType cacheArchiveType, CacheFileGranularity granularity, ILoadCachePathResolver resolver)
        {
            DateFormat = dateFormat;
            ArchiveType = cacheArchiveType;
            CacheFileGranularity = granularity;
            Resolver = resolver;
            RootDirectory = rootDirectory;
        }

        public virtual void CreateIfNotExists()
        {
            var downloadDirectory = Resolver.GetLoadCacheDirectory(RootDirectory);
            if (!downloadDirectory.Exists)
                downloadDirectory.Create();
        }

        public virtual bool CheckExists(DateTime archiveDate)
        {
            var fi = GetArchiveFileInfoForDate(archiveDate);
            return fi != null && fi.Exists;
        }

        public virtual FileInfo GetArchiveFileInfoForDate(DateTime archiveDate)
        {
            var loadCacheDirectory = GetLoadCacheDirectory();
            
            if(ArchiveType == CacheArchiveType.None)
            {
                var matching = loadCacheDirectory.GetFiles(archiveDate.ToString(DateFormat) + ".*").ToArray();

                if(matching.Length > 1)
                    throw new Exception("Mulitple files found in Cache that share the date " + archiveDate + ", matching files were:" + string.Join(",",matching.Select(m=>m.Name)) + ".  Cache directory is:" + loadCacheDirectory);

                if (matching.Length == 1)
                    return matching[0];

                //no matching archive is problem?
                return null;
            }

            var filename = archiveDate.ToString(DateFormat) + "." + ArchiveType.ToString().ToLower();

            return new FileInfo(Path.Combine(loadCacheDirectory.FullName, filename));
        }

        /// <summary>
        /// The cache sub-directory for a particular load schedule within a load metadata. Uses a resolver for dataset-specific cache layout knowledge
        /// </summary>
        /// <returns></returns>
        public virtual DirectoryInfo GetLoadCacheDirectory()
        {
            if(Resolver == null)
                throw new Exception("No ILoadCachePathResolver has been set on CacheLayout " + this + ", this tells the system whether there are subdirectories and which one to use for a given ICacheLayout, if you don't have one use a new NoSubdirectoriesCachePathResolver() in your ICacheLayout constructor");

            if(RootDirectory == null)
                throw new NullReferenceException("RootDirectory has not been set yet");

            var downloadDirectory = Resolver.GetLoadCacheDirectory(RootDirectory);

            if (downloadDirectory == null)
                throw new Exception("Resolver " + Resolver + " of type " + Resolver.GetType().FullName + " returned null from GetLoadCacheDirectory");
            return downloadDirectory.Exists ? downloadDirectory : Directory.CreateDirectory(downloadDirectory.FullName);
        }
        
        private IEnumerable<FileInfo> GetArchiveFilesInLoadCacheDirectory()
        {
            var disciplineRoot = GetLoadCacheDirectory();
            return disciplineRoot.EnumerateFiles("*." + (ArchiveType != CacheArchiveType.None ?ArchiveType.ToString():"*"));
        }

        private IEnumerable<DateTime> GetDateListFromArchiveFilesInLoadCacheDirectory()
        {
            // remove the extension
            return GetArchiveFilesInLoadCacheDirectory()
                .Select(
                    info =>
                        DateTime.ParseExact(Path.GetFileNameWithoutExtension(info.Name), DateFormat,
                            CultureInfo.InvariantCulture));
        }

        public virtual Queue<DateTime> GetSortedDateQueue()
        {
            var dateList = GetDateListFromArchiveFilesInLoadCacheDirectory().ToList();
            dateList.Sort();

            return new Queue<DateTime>(dateList);
        }

        public bool CheckCacheFilesAvailability()
        {
            return GetArchiveFilesInLoadCacheDirectory().Any();
        }

        public DateTime? GetMostRecentDateToLoadAccordingToFilesystem()
        {
            var dateList = GetDateListFromArchiveFilesInLoadCacheDirectory().ToList();

            if (!dateList.Any())
                return null;

            dateList.Sort();
            return dateList[dateList.Count -1];
        }
        
        public DateTime? GetEarliestDateToLoadAccordingToFilesystem()
        {
            var dateList = GetDateListFromArchiveFilesInLoadCacheDirectory().ToList();

            if (!dateList.Any())
                return null;

            dateList.Sort();
            return dateList[0];
        }

        protected void ArchiveFiles(FileInfo[] files, DateTime archiveDate)
        {
            if (!files.Any()) return;

            if (ArchiveType == CacheArchiveType.None)
                throw new ArgumentException("When using CacheArchiveType.None you should not use ArchiveFiles, instead just copy them into the relevant Cache directory yourself.  Remember that you must have 1 file per day and the filename must be the date according to the DateFormat e.g. 2001-01-01.csv or 2001-01-01.txt or whatever");

            var archiveFilepath = GetArchiveFileInfoForDate(archiveDate);
            var archiveDirectory = archiveFilepath.DirectoryName;
            if (archiveDirectory == null)
                throw new Exception("The directory for the archive within the cache is being reported as null, which should not be possible.");

            if (!Directory.Exists(archiveDirectory))
                Directory.CreateDirectory(archiveDirectory);

            // todo: should control whether using existing files is allowed or whether should throw if we the archive already exists
            var zipArchiveMode = archiveFilepath.Exists ? ZipArchiveMode.Update : ZipArchiveMode.Create;
            if (ArchiveType == CacheArchiveType.Zip)
                using (var zipArchive = ZipFile.Open(archiveFilepath.FullName, zipArchiveMode))
                {
                    // Entries can't be inspected if the zip archive has been opened in create mode
                    if (zipArchiveMode == ZipArchiveMode.Update)
                    {
                        var entries = zipArchive.Entries;
                        // don't add an entry where one already exists for a particular dataFile
                        foreach (var dataFile in files.Where(dataFile => entries.All(e => e.Name != dataFile.Name)))
                        {
                            zipArchive.CreateEntryFromFile(dataFile.FullName, dataFile.Name, CompressionLevel.Optimal);
                        }
                    }
                    else
                    {
                        // We are creating a new file, so don't have to check for the existence of entries.
                        foreach (var dataFile in files)
                        {
                            zipArchive.CreateEntryFromFile(dataFile.FullName, dataFile.Name, CompressionLevel.Optimal);
                        }
                        
                    }
                }
            else
                using (var tarArchive = TarArchive.CreateOutputTarArchive(new FileStream(archiveFilepath.FullName, FileMode.CreateNew)))
                {
                    // RootPath is case-sensitive *and* requires forward slashes!
                    tarArchive.RootPath = archiveDirectory.Replace(@"\", @"/");
                    foreach (var item in files)
                    {
                        var entry = TarEntry.CreateEntryFromFile(item.FullName);
                        tarArchive.WriteEntry(entry, true);
                    }
                }
        }
    }
}
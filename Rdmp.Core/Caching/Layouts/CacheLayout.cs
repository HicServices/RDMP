// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Caching.Layouts;

/// <summary>
///     Abstract base ICacheLayout, see ICacheLayout for description or BasicCacheLayout for example of how to use this
///     class.
/// </summary>
public abstract class CacheLayout : ICacheLayout
{
    public string DateFormat { get; }
    public CacheArchiveType ArchiveType { get; }
    public CacheFileGranularity CacheFileGranularity { get; }
    public ILoadCachePathResolver Resolver { get; }
    public DirectoryInfo RootDirectory { get; }

    /// <summary>
    ///     When archiving files what compression level should be used to add/update new files (only applies if
    ///     <see cref="ArchiveType" /> is zip).  Defaults to Optimal
    /// </summary>
    public CompressionLevel Compression { get; set; } = CompressionLevel.Optimal;

    public CacheLayout(DirectoryInfo rootDirectory, string dateFormat, CacheArchiveType cacheArchiveType,
        CacheFileGranularity granularity, ILoadCachePathResolver resolver)
    {
        DateFormat = dateFormat;
        ArchiveType = cacheArchiveType;
        CacheFileGranularity = granularity;
        Resolver = resolver;
        RootDirectory = rootDirectory;
    }

    public virtual void CreateIfNotExists(IDataLoadEventListener listener)
    {
        var downloadDirectory = Resolver.GetLoadCacheDirectory(RootDirectory);
        if (!downloadDirectory.Exists)
            downloadDirectory.Create();
    }

    public virtual bool CheckExists(DateTime archiveDate, IDataLoadEventListener listener)
    {
        var fi = GetArchiveFileInfoForDate(archiveDate, listener);
        return fi is { Exists: true };
    }

    public virtual FileInfo GetArchiveFileInfoForDate(DateTime archiveDate, IDataLoadEventListener listener)
    {
        var loadCacheDirectory = GetLoadCacheDirectory(listener);

        if (ArchiveType == CacheArchiveType.None)
        {
            var matching = loadCacheDirectory.GetFiles($"{archiveDate.ToString(DateFormat)}.*");

            return matching.Length switch
            {
                > 1 => throw new Exception(
                    $"Multiple files found in Cache that share the date {archiveDate}, matching files were:{string.Join(",", matching.Select(m => m.Name))}.  Cache directory is:{loadCacheDirectory}"),
                1 => matching[0],
                _ => null
            };

            //no matching archive is problem?
        }

        var filename = $"{archiveDate.ToString(DateFormat)}.{ArchiveType.ToString().ToLower()}";

        listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Looking for a file called '{filename}' in '{loadCacheDirectory.FullName}'"));

        return new FileInfo(Path.Combine(loadCacheDirectory.FullName, filename));
    }

    /// <summary>
    ///     The cache sub-directory for a particular load schedule within a load metadata. Uses a resolver for dataset-specific
    ///     cache layout knowledge
    /// </summary>
    /// <param name="listener"></param>
    /// <returns></returns>
    public virtual DirectoryInfo GetLoadCacheDirectory(IDataLoadEventListener listener)
    {
        if (Resolver == null)
            throw new Exception(
                $"No ILoadCachePathResolver has been set on CacheLayout {this}, this tells the system whether there are subdirectories and which one to use for a given ICacheLayout, if you don't have one use a new NoSubdirectoriesCachePathResolver() in your ICacheLayout constructor");

        if (RootDirectory == null)
            throw new NullReferenceException("RootDirectory has not been set yet");

        var downloadDirectory = Resolver.GetLoadCacheDirectory(RootDirectory) ?? throw new Exception(
            $"Resolver {Resolver} of type {Resolver.GetType().FullName} returned null from GetLoadCacheDirectory");
        if (downloadDirectory.Exists)
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Trace,
                    $"Download Directory Is:{downloadDirectory.FullName}"));
        }
        else
        {
            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                $"Download Directory Did Not Exist:{downloadDirectory.FullName}"));

            downloadDirectory.Create();

            listener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Created Download Directory:{downloadDirectory.FullName}"));
        }

        return downloadDirectory;
    }

    private IEnumerable<FileInfo> GetArchiveFilesInLoadCacheDirectory(IDataLoadEventListener listener)
    {
        var disciplineRoot = GetLoadCacheDirectory(listener);
        return disciplineRoot.EnumerateFiles(
            $"*.{(ArchiveType != CacheArchiveType.None ? ArchiveType.ToString() : "*")}");
    }

    private IEnumerable<DateTime> GetDateListFromArchiveFilesInLoadCacheDirectory(IDataLoadEventListener listener)
    {
        // remove the extension
        return GetArchiveFilesInLoadCacheDirectory(listener)
            .Select(
                info =>
                    DateTime.ParseExact(Path.GetFileNameWithoutExtension(info.Name), DateFormat,
                        CultureInfo.InvariantCulture));
    }

    public virtual Queue<DateTime> GetSortedDateQueue(IDataLoadEventListener listener)
    {
        var dateList = GetDateListFromArchiveFilesInLoadCacheDirectory(listener).ToList();
        dateList.Sort();

        return new Queue<DateTime>(dateList);
    }

    public bool CheckCacheFilesAvailability(IDataLoadEventListener listener)
    {
        return GetArchiveFilesInLoadCacheDirectory(listener).Any();
    }

    public DateTime? GetMostRecentDateToLoadAccordingToFilesystem(IDataLoadEventListener listener)
    {
        return GetDateListFromArchiveFilesInLoadCacheDirectory(listener).Max();
    }

    public DateTime? GetEarliestDateToLoadAccordingToFilesystem(IDataLoadEventListener listener)
    {
        return GetDateListFromArchiveFilesInLoadCacheDirectory(listener).Min();
    }

    protected void ArchiveFiles(FileInfo[] files, DateTime archiveDate, IDataLoadEventListener listener)
    {
        if (!files.Any()) return;

        if (ArchiveType == CacheArchiveType.None)
            throw new ArgumentException(
                "When using CacheArchiveType.None you should not use ArchiveFiles, instead just copy them into the relevant Cache directory yourself.  Remember that you must have 1 file per day and the filename must be the date according to the DateFormat e.g. 2001-01-01.csv or 2001-01-01.txt or whatever");

        var archiveFilepath = GetArchiveFileInfoForDate(archiveDate, listener);
        var archiveDirectory = archiveFilepath.DirectoryName ?? throw new Exception(
            "The directory for the archive within the cache is being reported as null, which should not be possible.");
        if (!Directory.Exists(archiveDirectory))
            Directory.CreateDirectory(archiveDirectory);

        // todo: should control whether using existing files is allowed or whether should throw if we the archive already exists
        if (ArchiveType == CacheArchiveType.Zip)
        {
            ZipArchiveMode zipArchiveMode;
            var ziptmp = $"{archiveFilepath.FullName}.tmp";
            if (archiveFilepath.Exists)
            {
                zipArchiveMode = ZipArchiveMode.Update;
                File.Copy(archiveFilepath.FullName, ziptmp, true);
            }
            else
            {
                zipArchiveMode = ZipArchiveMode.Create;
                File.Delete(ziptmp);
            }

            using (var zipArchive = ZipFile.Open(ziptmp, zipArchiveMode))
            {
                var existing = new HashSet<string>();
                // Entries can't be inspected if the zip archive has been opened in create mode
                if (zipArchiveMode == ZipArchiveMode.Update)
                    foreach (var zipArchiveEntry in zipArchive.Entries)
                        existing.Add(zipArchiveEntry.Name);

                foreach (var dataFile in files)
                    if (!existing.Contains(dataFile.Name))
                        zipArchive.CreateEntryFromFile(dataFile.FullName, dataFile.Name, Compression);
            }

            // On .Net Core 3.0 and later, we can use the 3-argument variant of .Move to do this atomically
            File.Move(ziptmp, archiveFilepath.FullName, true);
        }
    }
}
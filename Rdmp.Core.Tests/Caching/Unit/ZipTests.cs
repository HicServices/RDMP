// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using NUnit.Framework;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Caching.Pipeline.Destinations;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.Tests.Caching.Unit;

internal class ZipTests
{
    private class ZipTestLayout : CacheLayout
    {
        public ZipTestLayout(DirectoryInfo dir, string dateFormat, CacheArchiveType cacheArchiveType,
            CacheFileGranularity granularity, ILoadCachePathResolver resolver) : base(dir, dateFormat, cacheArchiveType,
            granularity, resolver)
        {
        }

        public new void ArchiveFiles(FileInfo[] fi, DateTime dt, IDataLoadEventListener l)
        {
            base.ArchiveFiles(fi, dt, l);
        }
    }

    [Test]
    public void CreateAndUpdateZip()
    {
        var _dir = TestContext.CurrentContext.WorkDirectory;
        var _zt = new ZipTestLayout(new DirectoryInfo(_dir), "yyyy-MM-dd", CacheArchiveType.Zip, CacheFileGranularity.Hour, new NoSubdirectoriesCachePathResolver());
        var _listener = ThrowImmediatelyDataLoadEventListener.Quiet;
        var when = DateTime.Now;
        var targetzip = _zt.GetArchiveFileInfoForDate(when, _listener);
        var files = new List<FileInfo>();

        // First create a zip file with one item in
        File.Delete(targetzip.FullName);
        files.Add(new FileInfo(Path.Combine(_dir, Path.GetRandomFileName())));
        using (var sw = new StreamWriter(files[0].FullName))
        {
            sw.WriteLine("Example data file");
        }

        _zt.ArchiveFiles(files.ToArray(), when, _listener);
        using (var zip = ZipFile.Open(targetzip.FullName, ZipArchiveMode.Read))
        {
            Assert.True(zip.Entries.Count == 1);
        }

        // Create a second file and add that to the zip too
        files.Add(new FileInfo(Path.Combine(_dir, Path.GetRandomFileName())));
        using (var sw = new StreamWriter(files[1].FullName))
        {
            sw.WriteLine("Another example data file");
        }

        _zt.ArchiveFiles(files.ToArray(), when, _listener);
        using (var zip = ZipFile.Open(targetzip.FullName, ZipArchiveMode.Read))
        {
            Assert.True(zip.Entries.Count == 2);
        }

        // Re-add just the first file: resulting zip should still contain both files
        _zt.ArchiveFiles(files.GetRange(0, 1).ToArray(), when, _listener);
        using (var zip = ZipFile.Open(targetzip.FullName, ZipArchiveMode.Read))
        {
            Assert.True(zip.Entries.Count == 2);
        }

        files.ForEach(s => File.Delete(s.FullName));
        File.Delete(targetzip.FullName);
    }
}
// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.DataProvider.FromCache;

internal interface IArchivedFileExtractor
{
    void Extract(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory,
        IDataLoadEventListener dataLoadJob);
}

internal abstract class ArchiveExtractor : IArchivedFileExtractor
{
    private readonly string _extension;

    protected abstract void DoExtraction(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory,
        IDataLoadEventListener dataLoadJob);

    protected ArchiveExtractor(string extension)
    {
        _extension = extension;
    }

    public void Extract(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory,
        IDataLoadEventListener dataLoadJob)
    {
        //ensure it is a legit zip file
        if (!job.Value.Extension.Equals(_extension))
            throw new NotSupportedException(
                $"Unexpected job file extension -{job.Value.Extension} (expected {_extension})");

        //tell the UI/listener that we have identified the archive
        dataLoadJob.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
            $"Archive identified:{job.Value.FullName}"));

        try
        {
            DoExtraction(job, destinationDirectory, dataLoadJob);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error occurred extracting zip archive {job.Value.FullName}", ex);
        }
    }
}

internal class ZipExtractor : ArchiveExtractor
{
    public ZipExtractor() : base(".zip")
    {
    }


    protected override void DoExtraction(KeyValuePair<DateTime, FileInfo> job, DirectoryInfo destinationDirectory,
        IDataLoadEventListener dataLoadEventListener)
    {
        using (var archive = new ZipArchive(new FileStream(job.Value.FullName, FileMode.Open)))
        {
            archive.ExtractToDirectory(destinationDirectory.FullName);
        }
    }
}
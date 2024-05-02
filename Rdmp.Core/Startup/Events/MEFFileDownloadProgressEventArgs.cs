// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;

namespace Rdmp.Core.Startup.Events;

/// <summary>
///     EventArgs for MEF downloading during Startup.cs
///     <para>Records whether the file was successfully downloaded and the number of dlls saved so far.</para>
/// </summary>
public class MEFFileDownloadProgressEventArgs
{
    public MEFFileDownloadProgressEventArgs(DirectoryInfo downloadDirectory, int dllsSeenInCatalogue,
        int currentDllNumber, string fileBeingProcessed, bool includesPdbFile, MEFFileDownloadEventStatus status,
        Exception exception = null)
    {
        DownloadDirectory = downloadDirectory;
        DllsSeenInCatalogue = dllsSeenInCatalogue;
        CurrentDllNumber = currentDllNumber;
        FileBeingProcessed = fileBeingProcessed;
        IncludesPdbFile = includesPdbFile;
        Status = status;
        Exception = exception;
    }

    public DirectoryInfo DownloadDirectory { get; set; }

    public int DllsSeenInCatalogue { get; set; }
    public int CurrentDllNumber { get; set; }

    public MEFFileDownloadEventStatus Status { get; set; }

    public string FileBeingProcessed { get; set; }
    public bool IncludesPdbFile { get; set; }
    public Exception Exception { get; set; }
}
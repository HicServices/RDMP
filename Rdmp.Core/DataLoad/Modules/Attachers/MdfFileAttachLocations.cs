// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using Rdmp.Core.DataLoad.Modules.Exceptions;

namespace Rdmp.Core.DataLoad.Modules.Attachers;

/// <summary>
///     Container class for recording the various directories involved in attaching a microsoft database file (MDF) to an
///     Sql Server Instance.
/// </summary>
internal class MdfFileAttachLocations
{
    public MdfFileAttachLocations(DirectoryInfo originDirectory,
        string databaseDirectoryFromPerspectiveOfDatabaseServer, string copyToDirectoryOrNullIfDatabaseIsLocalhost)
    {
        ArgumentNullException.ThrowIfNull(databaseDirectoryFromPerspectiveOfDatabaseServer);

        var copyToDirectory = copyToDirectoryOrNullIfDatabaseIsLocalhost ??
                              databaseDirectoryFromPerspectiveOfDatabaseServer;

        var filesThatWeCouldLoad = originDirectory.GetFiles("*.mdf").ToArray();

        switch (filesThatWeCouldLoad.Length)
        {
            case 0:
                throw new FileNotFoundException(
                    $"Could not find any MDF files in the directory {originDirectory.FullName}");
            case > 1:
                throw new MultipleMatchingFilesException(
                    $"Did not know which MDF file to attach, found multiple :{string.Join(",", filesThatWeCouldLoad.Select(f => f.Name))}");
        }

        OriginLocationMdf = filesThatWeCouldLoad[0].FullName;

        //verify log file exists
        OriginLocationLdf = Path.Combine(Path.GetDirectoryName(OriginLocationMdf),
            $"{Path.GetFileNameWithoutExtension(OriginLocationMdf)}_log.ldf");

        if (!File.Exists(OriginLocationLdf))
            throw new FileNotFoundException($"Cannot attach database, LOG file was not found:{OriginLocationLdf}",
                OriginLocationLdf);

        CopyToMdf = Path.Combine(copyToDirectory, Path.GetFileName(OriginLocationMdf));
        CopyToLdf = Path.Combine(copyToDirectory, Path.GetFileName(OriginLocationLdf));
        AttachMdfPath =
            MergeDirectoryAndFileUsingAssumedDirectorySeparator(databaseDirectoryFromPerspectiveOfDatabaseServer,
                OriginLocationMdf);
        AttachLdfPath =
            MergeDirectoryAndFileUsingAssumedDirectorySeparator(databaseDirectoryFromPerspectiveOfDatabaseServer,
                OriginLocationLdf);
    }

    public string OriginLocationMdf { get; set; }
    public string OriginLocationLdf { get; set; }

    public string CopyToMdf { get; set; }
    public string CopyToLdf { get; set; }

    public string AttachMdfPath { get; set; }
    public string AttachLdfPath { get; set; }


    private static char GetCorrectDirectorySeparatorCharBasedOnString(string partialPath)
    {
        var containUnixStyle = partialPath.Contains('/');
        var containsNTFSStyle = partialPath.Contains('\\');
        if (containUnixStyle && containsNTFSStyle)
            throw new Exception(
                "Override partial path contains both '/' and '\\', unable to correctly guess which file system is in use ");
        return containUnixStyle ? '/' : '\\';
    }

    public static string MergeDirectoryAndFileUsingAssumedDirectorySeparator(string directory, string file)
    {
        var directorySeparator = GetCorrectDirectorySeparatorCharBasedOnString(directory);
        return directory.TrimEnd(directorySeparator) + directorySeparator + Path.GetFileName(file);
    }
}
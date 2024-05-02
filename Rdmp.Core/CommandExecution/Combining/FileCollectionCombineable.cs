// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using Rdmp.Core.Curation.Data.Serialization;

namespace Rdmp.Core.CommandExecution.Combining;

/// <summary>
///     Describes file(s) which the user is seeking to combine with other object(s) (e.g. during drag and drop)
/// </summary>
public class FileCollectionCombineable : ICombineToMakeCommand
{
    /// <summary>
    ///     The files which have been selected for combining
    /// </summary>
    public FileInfo[] Files { get; set; }


    /// <summary>
    ///     True if the <see cref="Files" /> are serialized <see cref="ShareDefinition" /> files
    /// </summary>
    public bool IsShareDefinition { get; set; }

    /// <summary>
    ///     Creates a new instance in which the user is seeking to combine the given <paramref name="files" />
    /// </summary>
    /// <param name="files"></param>
    public FileCollectionCombineable(FileInfo[] files)
    {
        Files = files;
        IsShareDefinition = files.Length == 1 && files[0].Extension == ".sd";
    }

    /// <inheritdoc />
    public string GetSqlString()
    {
        return null;
    }
}
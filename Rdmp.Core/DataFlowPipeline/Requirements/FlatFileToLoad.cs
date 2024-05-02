// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;

namespace Rdmp.Core.DataFlowPipeline.Requirements;

/// <summary>
///     Wrapper for FileInfo that can be used in the IPipelineRequirement interface to indicate that component expects a
///     FileInfo that is specifically going to have data loaded
///     out of it.  Having an IPipelineRequirement for a FileInfo on a component could be confusing, we might also want to
///     allow multiple different types of FileInfo.  Having
///     this wrapper ensures that there is no confusion about what a FlatFileToLoad Initialization Object is for.
/// </summary>
public class FlatFileToLoad
{
    /// <summary>
    ///     Creates a new instance pointed at the given <paramref name="file" />
    /// </summary>
    /// <param name="file"></param>
    public FlatFileToLoad(FileInfo file)
    {
        File = file;
    }

    /// <summary>
    ///     The file you are trying to load
    /// </summary>
    public FileInfo File { get; set; }

    /// <summary>
    ///     Returns the filename of the file you are trying to load
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return File == null ? base.ToString() : File.Name;
    }
}
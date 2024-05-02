// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;

namespace Rdmp.Core.ReusableLibraryCode;

/// <summary>
///     Checks whether two DirectoryInfo objects are the same based on FullName
/// </summary>
internal sealed class DirectoryInfoComparer : IEqualityComparer<DirectoryInfo>
{
    public bool Equals(DirectoryInfo x, DirectoryInfo y)
    {
        return ReferenceEquals(x, y) || x?.FullName == y?.FullName;
    }

    public int GetHashCode(DirectoryInfo obj)
    {
        return obj.FullName.GetHashCode();
    }
}

internal static class DirectoryInfoExtensions
{
    public static void CopyAll(this DirectoryInfo source, DirectoryInfo target)
    {
        if (string.Equals(source.FullName, target.FullName, StringComparison.InvariantCultureIgnoreCase)) return;

        // Check if the target directory exists, if not, create it.
        if (Directory.Exists(target.FullName) == false) Directory.CreateDirectory(target.FullName);

        // Copy each file into its new directory.
        foreach (var fi in source.GetFiles()) fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

        // Copy each subdirectory using recursion.
        foreach (var diSourceSubDir in source.GetDirectories())
        {
            var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
            diSourceSubDir.CopyAll(nextTargetSubDir);
        }
    }
}
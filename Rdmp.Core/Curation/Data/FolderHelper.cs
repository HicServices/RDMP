// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;

namespace Rdmp.Core.Curation.Data;

/// <summary>
///     The virtual 'folder' in which to display objects.  This should be a helpful subdivision e.g. "\core
///     datasets\labsystems\"
///     <para>
///         CatalogueFolder are represented in the user interface as a tree of folders (calculated at runtime). You can't
///         create an empty CatalogueFolder,
///         just declare an <see cref="IHasFolder" /> (e.g. <see cref="Catalogue" />) as being in a new folder and it will
///         be automatically shown.
///     </para>
///     <para>
///         CatalogueFolder is a static class that contains helper methods to help prevent illegal paths and to calculate
///         hierarchy based on multiple <see cref="IHasFolder" />
///         (See <see cref="BuildFolderTree{T}(T[], FolderNode{T})" />)
///     </para>
/// </summary>
public static class FolderHelper
{
    /// <summary>
    ///     The topmost folder under which all other folders reside
    /// </summary>
    public const string Root = "\\";


    public static string Adjust(string candidate)
    {
        if (!IsValidPath(candidate, out var reason)) throw new NotSupportedException(reason);
        candidate = candidate.ToLower();
        candidate = candidate.TrimEnd('\\');

        if (string.IsNullOrWhiteSpace(candidate)) candidate = Root;

        return candidate;
    }

    public static bool IsValidPath(string candidatePath, out string reason)
    {
        reason = null;

        if (string.IsNullOrWhiteSpace(candidatePath))
            reason =
                "An attempt was made to set Catalogue Folder to null, every Catalogue must have a folder, set it to \\ if you want the root";
        else if (!candidatePath.StartsWith("\\"))
            reason = $"All catalogue paths must start with \\.  Invalid path was:{candidatePath}";
        else if (candidatePath.Contains("\\\\")) //if it contains double slash
            reason = $"Catalogue paths cannot contain double slashes '\\\\', Invalid path was:{candidatePath}";
        else if (candidatePath.Contains('/')) //if it contains double slash
            reason =
                $"Catalogue paths must use backwards slashes not forward slashes, Invalid path was:{candidatePath}";

        return reason == null;
    }

    /// <summary>
    ///     Returns true if the specified path is valid for a <see cref="IHasFolder" />.  Not blank, starts with '\' etc.
    /// </summary>
    /// <param name="candidatePath"></param>
    /// <returns></returns>
    public static bool IsValidPath(string candidatePath)
    {
        return IsValidPath(candidatePath, out _);
    }

    public static FolderNode<T> BuildFolderTree<T>(T[] objects, FolderNode<T> currentBranch = null)
        where T : class, IHasFolder
    {
        currentBranch ??= new FolderNode<T>(Root);
        var currentBranchFullName = currentBranch.FullName;

        foreach (var g in objects.GroupBy(g => g.Folder).ToArray())
            if (g.Key.Equals(currentBranchFullName, StringComparison.CurrentCultureIgnoreCase))
            {
                // all these are in the exact folder we are looking at, they are our children
                currentBranch.ChildObjects.AddRange(g);
            }
            else
            {
                // these objects are in a subdirectory of us.  Find the next subdirectory name
                // bearing in mind we may be at '\' and be seing '\dog\cat\fish' as the next
                var idx = g.Key.IndexOf(currentBranchFullName, StringComparison.CurrentCultureIgnoreCase) +
                          currentBranchFullName.Length;

                // if we have objects that do not live under this full path thats a problem
                // or its also a problem if we found a full match to the end of the string
                // this branch deals with sub folders and that would mean the current group
                // are not in any subfolders
                if (idx == -1 || idx == g.Key.Length - 1)
                    throw new Exception(
                        $"Unable to build folder groups.  Current group was not a child of the current branch.  Branch was '{currentBranch.FullName}' while Group was '{g.Key}'");

                var subFolders = g.Key[idx..];
                var nextFolder = subFolders.Split('\\', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ??
                                 throw new Exception(
                                     $"Unable to build folder groups.  Current group had malformed Folder name.  Branch was '{currentBranch.FullName}' while Group was '{g.Key}'");

                // we may already have created this as part of a subgroup e.g. seeing \1\2 then seeing \1 alone (we don't want multiple copies of \1 folder).
                var existing = currentBranch.ChildFolders.FirstOrDefault(f =>
                    f.Name.Equals(nextFolder, StringComparison.CurrentCultureIgnoreCase));

                if (existing == null)
                {
                    // we don't have one already so create it
                    existing = new FolderNode<T>(nextFolder, currentBranch);
                    currentBranch.ChildFolders.Add(existing);
                }

                BuildFolderTree(g.ToArray(), existing);
            }

        return currentBranch;
    }
}
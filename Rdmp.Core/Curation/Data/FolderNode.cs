// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rdmp.Core.Curation.Data.Cohort;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// A virtual folder that can have subdirectories and stores ojbects of type <typeparamref name="T"/> e.g.
/// Catalogue folders.  <see cref="FolderNode{T}"/> objects are typically created through <see cref="FolderHelper.BuildFolderTree{T}(T[], FolderNode{T})"/>
/// dynamically based on the current <see cref="IHasFolder.Folder"/> strings.
/// </summary>
/// <typeparam name="T"></typeparam>
public class
    FolderNode<T> : IFolderNode,
        IOrderable /*Orderable interface ensures that folders always appear before datasets in tree*/
    where T : class, IHasFolder
{
    public string Name { get; set; }
    public List<T> ChildObjects { get; set; } = new();
    public List<FolderNode<T>> ChildFolders { get; set; } = new();

    public FolderNode<T> Parent { get; set; }

    public string FullName => GetFullName();

    int IOrderable.Order
    {
        get => -1;
        set => throw new NotSupportedException();
    }

    public FolderNode(string name, FolderNode<T> parent = null)
    {
        Name = name;
        Parent = parent;
    }

    private string GetFullName()
    {
        // build the name by prepending each parent
        // but start with our name
        var sb = new StringBuilder(Name);

        var p = Parent;

        while (p != null)
        {
            if(p.Name.Equals(FolderHelper.Root))
            {
                sb.Insert(0, p.Name);
            }
            else
            {
                sb.Insert(0, $"{p.Name}\\");
            }

            p = p.Parent;
        }

        return sb.ToString();
    }

    public FolderNode<T> this[string key] => GetChild(key);

    private FolderNode<T> GetChild(string key)
    {
        return ChildFolders.FirstOrDefault(c => c.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
               ?? throw new ArgumentOutOfRangeException($"Could not find a child folder with the key '{key}'");
    }

    public override string ToString() => Name;

    public override bool Equals(object obj) => obj.GetType() == typeof(FolderNode<T>) && ((FolderNode<T>)obj).Name == FullName;

    public override int GetHashCode() => HashCode.Combine(FullName);
}
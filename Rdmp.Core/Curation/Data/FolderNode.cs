// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rdmp.Core.Curation.Data
{
    public class FolderNode<T> where T: class, IHasFolder
    {
        public string Name { get; set; }
        public List<T> ChildObjects { get; set; } = new();
        public List<FolderNode<T>> ChildFolders { get; set; } = new();

        public FolderNode<T> Parent { get; set; }

        public string FullName => GetFullName();

        public FolderNode(string name, FolderNode<T> parent = null)
        {
            Name = name;
            Parent = parent;
        }

        private string GetFullName()
        {
            // build the name by prepending each parent
            // but start with our name
            StringBuilder sb = new(Name);

            var p = Parent;            

            while(p != null)
            {
                if(p.Name.Equals(FolderHelper.Root))
                {
                    sb.Insert(0, p.Name);
                }
                else
                {
                    sb.Insert(0, p.Name + "\\");
                }
                                
                p = p.Parent;
            }

            return sb.ToString();
        }

        public FolderNode<T> this[string key]
        {
            get => GetChild(key);
        }

        private FolderNode<T> GetChild(string key)
        {
            return ChildFolders.FirstOrDefault(c => c.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase))
                ?? throw new ArgumentOutOfRangeException($"Could not find a child folder with the key '{key}'");
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is FolderNode<T> node &&
                   FullName == node.FullName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FullName);
        }
    }
}

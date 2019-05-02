// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Reflection;

namespace MapsDirectlyToDatabaseTable.Versioning
{
    /// <inheritdoc/>
    public abstract class Patcher:IPatcher
    {
        /// <inheritdoc/>
        public virtual Assembly GetDbAssembly()
        {
            return GetType().Assembly;
        }

        /// <inheritdoc/>
        public string ResourceSubdirectory { get; private set; }

        /// <inheritdoc/>
        public int Tier { get; }

        public string Name => GetDbAssembly().GetName().Name + (string.IsNullOrEmpty(ResourceSubdirectory) ? "" : "/" + ResourceSubdirectory);
        public string LegacyName { get; protected set; }

        protected Patcher(int tier,string resourceSubdirectory)
        {
            Tier = tier;
            ResourceSubdirectory = resourceSubdirectory;
        }
        
        public SortedDictionary<string, Patch> GetAllPatchesInAssembly()
        {
            return Patch.GetAllPatchesInAssembly(this);
        }
    }

}
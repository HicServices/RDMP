// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using ReusableLibraryCode.Extensions;

namespace CatalogueLibrary.Repositories.Managers
{
    /// <inheritdoc/>
    public class PluginManager : IPluginManager
    {
        private readonly ICatalogueRepository _repository;

        public PluginManager(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        public Plugin[] GetCompatiblePlugins()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if(location == null)
                throw new Exception("Assembly had no listed Location");

            var fileVersion = FileVersionInfo.GetVersionInfo(location).FileVersion;
            var version = new Version(fileVersion);

            var plugins = _repository.GetAllObjects<Plugin>().Where(p => p.PluginVersion.IsCompatibleWith(version, 3));
            var uniquePlugins = plugins.GroupBy(p => new { name = p.Name, ver = new Version(p.PluginVersion.Major, p.PluginVersion.Minor, p.PluginVersion.Build) })
                .ToDictionary(g => g.Key, p => p.OrderByDescending(pv => pv.PluginVersion).First());
            return uniquePlugins.Values.ToArray();
        }
    }
}
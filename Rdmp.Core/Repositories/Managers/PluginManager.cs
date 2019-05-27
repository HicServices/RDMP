// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Rdmp.Core.Curation.Data;
using ReusableLibraryCode.Extensions;

namespace Rdmp.Core.Repositories.Managers
{
    /// <inheritdoc/>
    public class PluginManager : IPluginManager
    {
        private readonly ICatalogueRepository _repository;


        public PluginManager(ICatalogueRepository repository)
        {
            _repository = repository;
        }

        public Curation.Data.Plugin[] GetCompatiblePlugins()
        {
            var location = Assembly.GetExecutingAssembly().Location;
            if(location == null)
                throw new Exception("Assembly had no listed Location");

            var fileVersion = FileVersionInfo.GetVersionInfo(location).FileVersion;
            var runningSoftwareVersion = new Version(fileVersion);

            //nupkg that are compatible with the running software
            var lma = _repository.GetAllObjects<LoadModuleAssembly>().Where(a=>new Version(a.DllFileVersion).IsCompatibleWith(runningSoftwareVersion,2));
            
            //latest versions
            var latestVersionsOfPlugins = from p in lma.Select(l=>l.Plugin)
                      group p by p.GetShortName() into grp
                      select grp.OrderByDescending(p => p.PluginVersion).FirstOrDefault();
                        
            return latestVersionsOfPlugins.ToArray();
        }
    }
}
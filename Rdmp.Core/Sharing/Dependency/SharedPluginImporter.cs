// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Sharing.Dependency;

/// <summary>
/// Facilitates importing plugins from a remote contributor and creating the local copies of the Plugin dlls in the local CatalogueRepository database.
/// </summary>
public class SharedPluginImporter
{
    private readonly ShareManager _shareManager;

    public SharedPluginImporter(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        _shareManager = new ShareManager(repositoryLocator);
    }

    public Curation.Data.Plugin Import(Stream stream) =>
        _shareManager.ImportSharedObject(stream, true).OfType<Curation.Data.Plugin>().Single();
}
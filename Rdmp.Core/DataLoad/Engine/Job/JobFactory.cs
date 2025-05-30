// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job;

/// <summary>
/// Basic IJobFactory for creating an 'OnDemand', one off, self contained (not date based) IDataLoadJob.
/// </summary>
public class JobFactory : IJobFactory
{
    private readonly ILoadMetadata _loadMetadata;
    private readonly ILogManager _logManager;

    public JobFactory(ILoadMetadata loadMetadata, ILogManager logManager)
    {
        _loadMetadata = loadMetadata;
        _logManager = logManager;
    }

    public IDataLoadJob Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        HICDatabaseConfiguration configuration)
    {
        var description = _loadMetadata.Name;
        var loadDirectory = new LoadDirectory(_loadMetadata.LocationOfForLoadingDirectory, _loadMetadata.LocationOfForArchivingDirectory, _loadMetadata.LocationOfExecutablesDirectory, _loadMetadata.LocationOfCacheDirectory);

        return new DataLoadJob(repositoryLocator, description, _logManager, _loadMetadata, loadDirectory, listener,
            configuration);
    }
}
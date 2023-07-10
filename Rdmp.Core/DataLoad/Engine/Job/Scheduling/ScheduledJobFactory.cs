// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.Logging;
using Rdmp.Core.Repositories;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.Job.Scheduling;

public abstract class ScheduledJobFactory : IJobFactory
{
    protected readonly int? OverrideNumberOfDaysToLoad;
    protected readonly string JobDescription;
    protected readonly ILoadMetadata LoadMetadata;
    protected readonly ILogManager LogManager;

    protected ScheduledJobFactory(int? overrideNumberOfDaysToLoad, ILoadMetadata loadMetadata, ILogManager logManager)
    {
        OverrideNumberOfDaysToLoad = overrideNumberOfDaysToLoad;
        JobDescription = loadMetadata.Name;
        LoadMetadata = loadMetadata;
        LogManager = logManager;
    }


    public abstract bool HasJobs();


    public IDataLoadJob Create(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,HICDatabaseConfiguration configuration)
    {
        var job = CreateImpl(repositoryLocator,listener,configuration);

        if (job?.DatesToRetrieve == null || !job.DatesToRetrieve.Any())
            return null; // No dates to load

        return job;
    }

    protected abstract ScheduledDataLoadJob CreateImpl(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener, HICDatabaseConfiguration configuration);
}
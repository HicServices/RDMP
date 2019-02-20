// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using CatalogueLibrary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Repositories;
using DataLoadEngine.DatabaseManagement.EntityNaming;
using HIC.Logging;
using ReusableLibraryCode.Progress;

namespace DataLoadEngine.Job.Scheduling
{
    /// <summary>
    /// DataLoadJob that is part of an ongoing data load where only specific dates are loaded.  Typically this involves advancing the head of a LoadProgress
    /// (e.g. 'Load the next 5 days of LoadProgress - Tayside Biochemistry Load').
    /// </summary>
    public class ScheduledDataLoadJob : DataLoadJob
    {
        public ILoadProgress LoadProgress { get; set; }
        public List<DateTime> DatesToRetrieve { get; set; }
        
        public ScheduledDataLoadJob(IRDMPPlatformRepositoryServiceLocator repositoryLocator, string description, ILogManager logManager, ILoadMetadata loadMetadata, ILoadDirectory LoadDirectory, IDataLoadEventListener listener,HICDatabaseConfiguration configuration)
            : base(repositoryLocator,description, logManager, loadMetadata, LoadDirectory, listener,configuration)
        {
        }
    }
}
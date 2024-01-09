// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.Validation.Constraints.Primary;

namespace Rdmp.Core.Curation.Data.DataLoad;

/// <summary>
/// Entrypoint to the loading metadata for one or more Catalogues. This includes name, description, scheduled start dates etc.  A <see cref="LoadMetadata"/>
/// contains at least one <see cref="ProcessTask"/> which is an ETL step e.g. Unzip files called *.zip / Dowload all files from FTP server X.
/// </summary>
public interface ILoadMetadata : INamed, ILoggedActivityRootObject
{
    ICatalogueRepository CatalogueRepository { get; }

    /// <summary>
    /// Data loads can be either one offs (e.g. load all csv files in ForLoading) or iterative (load all data from the cache between 2001-01-01 and 2002-01-01).
    /// If a data load is iterative then it will have one or more <see cref="ILoadProgress"/> which describe how far through the loading process it is.
    /// </summary>
    ILoadProgress[] LoadProgresses { get; }

    /// <summary>
    /// The root working directory for a load.  Should have subdirectories like Data, Executables etc
    /// <para>For structured access to this use a new <see cref="ILoadDirectory"/></para>
    /// </summary>
    string LocationOfFlatFiles { get; set; }

    /// <summary>
    /// Set to true to ignore the requirement for live tables to need the backup archive trigger
    /// </summary>
    bool IgnoreTrigger { get; }

    /// <summary>
    /// Optional - Overrides the <see cref="ServerDefaults"/> RAWDataLoadServer with an explicit RAW server to use this load only.
    /// </summary>
    ExternalDatabaseServer OverrideRAWServer { get; }

    /// <summary>
    /// List of all the user configured steps in a data load.  For example you could have 2 ProcessTasks, one that downloads files from an FTP server and one that loads RAW.
    /// </summary>
    IOrderedEnumerable<IProcessTask> ProcessTasks { get; }

    /// <summary>
    /// Returns all datasets this load is responsible for supplying data to.  This determines which <see cref="TableInfo"/> are
    /// available during RAW=>STAGING=>LIVE migration (the super set of all tables underlying all catalogues).
    /// 
    /// <para>See also <see cref="ICatalogue.LoadMetadata_ID"/></para>
    /// </summary>
    /// <returns></returns>
    IEnumerable<ICatalogue> GetAllCatalogues();


    /// <summary>
    /// Returns the single server that contains all the live data tables in all the <see cref="ICatalogue"/> that are loaded by the <see cref="LoadMetadata"/>.
    /// All datasets in a load must be on the same database server.
    /// </summary>
    /// <returns></returns>
    DiscoveredServer GetDistinctLiveDatabaseServer();

    /// <summary>
    /// 
    /// </summary>
    DateTime? LastLoadTime { get; set; }
}
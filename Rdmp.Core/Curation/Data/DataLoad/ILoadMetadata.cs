// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.Curation.Data.DataLoad
{
    /// <inheritdoc cref="LoadMetadata"/>
    public interface ILoadMetadata : INamed
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
        /// The unique logging server for auditing the load (found by querying <see cref="Catalogue.LiveLoggingServer"/>)
        /// </summary>
        /// <returns></returns>
        DiscoveredServer GetDistinctLoggingDatabase();

        /// <inheritdoc cref="GetDistinctLoggingDatabase()"/>
        DiscoveredServer GetDistinctLoggingDatabase(out IExternalDatabaseServer serverChosen);

        /// <summary>
        /// Returns the single server that contains all the live data tables in all the <see cref="ICatalogue"/> that are loaded by the <see cref="LoadMetadata"/>.
        /// All datasets in a load must be on the same database server.
        /// </summary>
        /// <returns></returns>
        DiscoveredServer GetDistinctLiveDatabaseServer();

        /// <summary>
        /// Returns the unique value of <see cref="Catalogue.LoggingDataTask"/> amongst all catalogues loaded by the <see cref="LoadMetadata"/>
        /// </summary>
        /// <returns></returns>
        string GetDistinctLoggingTask();
    }
}
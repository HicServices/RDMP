using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <inheritdoc cref="LoadMetadata"/>
    public interface ILoadMetadata : ILoadProgressHost,INamed
    {
        /// <summary>
        /// The root working directory for a load.  Should have subdirectories like Data, Executables etc
        /// <para>For structured access to this use a new <see cref="IHICProjectDirectory"/></para>
        /// </summary>
        string LocationOfFlatFiles { get; set; }

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
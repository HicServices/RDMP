using System;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.DataLoad;
using CatalogueLibrary.Data.EntityNaming;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.DatabaseManagement.EntityNaming
{
    /// <summary>
    /// Wrapper for StandardDatabaseHelper (which tells you where RAW, STAGING and LIVE databases are during data load execution).  This class exists for two reasons
    /// 
    /// Firstly to decide (based on IAttachers) whether RAW tables need to be scripted or whether they will appear magically during DLE execution (e.g. by attaching 
    /// an MDF file).
    /// 
    /// Secondly to allow for overriding the RAW database server (which defaults to localhost).  It is a good idea to have RAW on a different server to LIVE/STAGING
    /// in order to reduce the risk incorrectly referencing tables in LIVE in Adjust RAW scripts etc.
    /// </summary>
    public class HICDatabaseConfiguration
    {
        public StandardDatabaseHelper DeployInfo { get; set; }
        public bool RequiresStagingTableCreation { get; set; }

        public INameDatabasesAndTablesDuringLoads DatabaseNamer
        {
            get { return DeployInfo.DatabaseNamer; }
        }

        /// <summary>
        /// Preferred Constructor, creates RAW, STAGING, LIVE connection strings based on the data access points in the LoadMetadata, also respects the ServerDefaults for RAW override (if any)
        /// </summary>
        /// <param name="lmd"></param>
        /// <param name="namer"></param>
        public HICDatabaseConfiguration(ILoadMetadata lmd, INameDatabasesAndTablesDuringLoads namer = null):
            this(lmd.GetDistinctLiveDatabaseServer(), namer, new ServerDefaults(((CatalogueRepository)lmd.Repository)))
        {
            
        }

        /// <summary>
        /// Constructor for use in tests, if possible use the LoadMetadata constructor instead
        /// </summary>
        /// <param name="liveServer">The live server where the data is held, IMPORTANT: this must contain InitialCatalog parameter</param>
        /// <param name="namer">optionally lets you specify how to pick database names for the temporary bubbles STAGING and RAW</param>
        /// <param name="defaults">optionally specifies the location to get RAW overrides from</param>
        public HICDatabaseConfiguration(DiscoveredServer liveServer, INameDatabasesAndTablesDuringLoads namer = null, IServerDefaults defaults = null)
        {
            //respects the override of LIVE server
            var builderToLive = (SqlConnectionStringBuilder)liveServer.Builder;

            if (string.IsNullOrWhiteSpace(builderToLive.InitialCatalog))
                throw new Exception("Cannot load live without having a unique live named database");

            // Default namer
            if (namer == null)
                namer = new FixedStagingDatabaseNamer(builderToLive.InitialCatalog);
            
            IExternalDatabaseServer overrideRAWServer = null;
            SqlConnectionStringBuilder rawServer = null;
            
            //if there are defaults
            if(defaults != null)
                overrideRAWServer = defaults.GetDefaultFor(ServerDefaults.PermissableDefaults.RAWDataLoadServer);//get the raw default if there is one

            //if there was defaults and a raw default server
            if(overrideRAWServer != null)
                rawServer = (SqlConnectionStringBuilder)DataAccessPortal.GetInstance().ExpectServer(overrideRAWServer, DataAccessContext.DataLoad,false).Builder;//get the raw server connection

            //populates the servers -- note that an empty rawServer value passed to this method makes it the localhost
            DeployInfo = new StandardDatabaseHelper(builderToLive, namer,rawServer);
            
            RequiresStagingTableCreation = true;
        }
    }
}
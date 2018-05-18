using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.EntityNaming;
using ReusableLibraryCode.DataAccess;
using ReusableLibraryCode.DatabaseHelpers.Discovery;

namespace DataLoadEngine.DatabaseManagement
{
    /// <summary>
    /// Stores the location of all the databases (RAW, STAGING, LIVE) available during a Data Load (See LoadMetadata).
    /// </summary>
    public class StandardDatabaseHelper
    {
        private readonly string _rootDatabaseName;
        public INameDatabasesAndTablesDuringLoads DatabaseNamer { get; set; }

        public Dictionary<LoadBubble, DiscoveredDatabase> DatabaseInfoList = new Dictionary<LoadBubble, DiscoveredDatabase>();

        //Constructor
        internal StandardDatabaseHelper(SqlConnectionStringBuilder liveDatabaseBuilder, INameDatabasesAndTablesDuringLoads namer,SqlConnectionStringBuilder rawServerBuilder = null)
        {
            _rootDatabaseName = liveDatabaseBuilder.InitialCatalog;
            DatabaseNamer = namer;

            if (rawServerBuilder == null)
                rawServerBuilder = new SqlConnectionStringBuilder(){DataSource = Environment.MachineName,IntegratedSecurity = true};

            foreach (LoadBubble stage in new []{LoadBubble.Raw,LoadBubble.Staging, LoadBubble.Live, })
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(stage == LoadBubble.Raw ? rawServerBuilder.ConnectionString : liveDatabaseBuilder.ConnectionString);
                builder.InitialCatalog = DatabaseNamer.GetDatabaseName(_rootDatabaseName, stage);
                DatabaseInfoList.Add(stage, new DiscoveredServer(builder).ExpectDatabase(builder.InitialCatalog));
            }
        }

        //Overload Constructor to use DataAccessPoint translation
        internal StandardDatabaseHelper(IDataAccessPoint dataAccessPoint, INameDatabasesAndTablesDuringLoads namer, SqlConnectionStringBuilder rawServerBuilder = null)
            : this(
                (SqlConnectionStringBuilder)DataAccessPortal.GetInstance().ExpectDatabase(dataAccessPoint, DataAccessContext.DataLoad).Server.Builder,
                namer,
                rawServerBuilder
            )
        {

        }

        // Indexer declaration.
        // If index is out of range, the temps array will throw the exception.
        public DiscoveredDatabase this[LoadBubble index]
        {
            get
            {
                return DatabaseInfoList[index];
            }
        }
    }
}